using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

public class CreateBoxes : MonoBehaviour
{
	static int debugPackageCountLimit = -1;
	static int debugBoxCountLimit = -1;
	// # of blocks or -1 to disable breakpoint
	static float scaleForFontSize18_12 = 0.02f;
	static float scaleForFontSize12_8 = 0.03333f;
	static float scale = scaleForFontSize18_12;
	// ratio from pixels to in-game units. Depends on rerun of Java image generation.

	/** 
	 * Lays out a set of blocks which make up a single package, fixing the size of the block in
	 * the z and y axes and letting it go as far as it needs to in the x axis.
	 */
	private class PackageLayoutArea
	{
		List<string> files;
		string packageName;
		float initialX = 0;
		float initialY = 0;
		float initialZ = 0;
		float spanZ = 40; // for package spacing of 50
		float spanY = 15;
		float x = 0;
		float y = 0;
		float z = 0;
		float zspacing = 1f;
		float xspacing = 10;
		float yspacing = 1f;
		float maxX = 0; // for tracking max extents
		float maxY = 0;
		float maxZ = 0;
		GameObject borders = null;

		public PackageLayoutArea (string packageName)
		{
			this.packageName = packageName;
			files = new List<string> ();
		}

		public float getXSize()
		{
			return Math.Max (maxX - initialX, 1);
		}
		public float getZSize()
		{
			return Math.Max (maxZ - initialZ, 1);
		}

		public void addFile (string file)
		{
			files.Add (file);
		}

		/* Add all the files for this package before calling layout */
		public void doLayout (float xstart, float zstart)
		{
			this.initialX = xstart;
			this.initialZ = zstart;
			x = xstart;
			y = initialY;
			z = zstart;

			// First, let's identify any super-talls and put them in the back row where their tops will show anyway.
			// Then we can lay out the rest of the blocks with a somewhat sane grid pattern.
			List<CodeBlock> blocks = new List<CodeBlock> ();
			List<CodeBlock> tallBlocks = new List<CodeBlock> ();
			foreach (var f in files) {
				CodeBlock block = new CodeBlock (File.ReadAllLines (f), f);
				if (block.isTallBlock ()) {
					tallBlocks.Add (block);
				} else {
					blocks.Add (block);
				}
			}
			//Console.WriteLine ("Positioning " + packageName + " at initial location " + initialX + "," + initialZ);

			layOutBlocksInPackage (blocks);
			if (tallBlocks.Count > 0 && blocks.Count > 0 && (tallBlocks.Count + blocks.Count > 5)) {
				moveToNextXRow ();
			}
			layOutBlocksInPackage (tallBlocks);

			// Create a GameObject to contain the set of blocks and report on its extents, and reparent the children.
			// From (initialX, initialZ) to (maxX, maxZ) covers our full extents.
			borders = new GameObject ("border-" + this.packageName);

			float xsize = getXSize ();
			float zsize = getZSize ();
			float xpos = initialX + (xsize / 2f);
			float zpos = initialZ + (zsize / 2f);

			borders.transform.position = new Vector3 ( xpos, 0, zpos);
			borders.transform.localScale = new Vector3 (xsize, spanY, zsize);
			//Console.WriteLine ("Borders position " + borders.transform.position);

			foreach (var block in blocks) {
				block.cube.transform.SetParent (borders.transform);
			}
			foreach (var block in tallBlocks) {
				block.cube.transform.SetParent (borders.transform);
			}

			createPackageAreaFloor ();

			// and for debug purposes, move the whole set up so we can see it.
			//borders.transform.position = new Vector3 (borders.transform.position.x, borders.transform.position.y, borders.transform.position.z);
		}

		void createPackageAreaFloor ()
		{
			// Create a cube to enclose the set. Leave a tiny border so we can see whether or not it bumps other cubes.
			float spaceBorderWidth = 0.2f;
			GameObject packageCubeFloor = GameObject.CreatePrimitive (PrimitiveType.Cube);
			packageCubeFloor.transform.position = new Vector3 (borders.transform.position.x - (spaceBorderWidth / 2f) - (xspacing/2f), 0f, borders.transform.position.z - (spaceBorderWidth /2f));
			packageCubeFloor.transform.localScale = new Vector3 (borders.transform.localScale.x - spaceBorderWidth, 0.2f, borders.transform.localScale.z - spaceBorderWidth);
			packageCubeFloor.name = "floor-" + this.packageName;
			packageCubeFloor.transform.SetParent (borders.transform);
			packageCubeFloor.GetComponent<BoxCollider> ().enabled = false;

			// Set material
			packageCubeFloor.GetComponent<Renderer> ().material.mainTexture = Resources.Load("Rocks 4") as Texture2D; // TODO: buggy

			// Optimize lighting
			packageCubeFloor.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // prettier and faster
			packageCubeFloor.GetComponent<Renderer> ().receiveShadows = false; // prettier and faster
			packageCubeFloor.GetComponent<MeshRenderer> ().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			packageCubeFloor.GetComponent<MeshRenderer> ().useLightProbes = false;

			// TODO: It would be great to add a packageName plaque as a smaller cube centered inside the front of this floor,
			// with a texture applied at 1x tiling with the package name on it.  That means creating a new cube.


		}

		void moveToNextXRow ()
		{
			x += xspacing;
			z = initialZ;
			y = initialY;
		}

		void layOutBlocksInPackage (List<CodeBlock> blocks)
		{
			// pinned to a fixed size in the Z axis and Y axis, and an unbounded size in X
			float rowMaxY = 0;
			foreach (var block in blocks) {
				block.renderBlock (x, y, z);
				maxX = Math.Max (maxX, x + block.xscale + xspacing);
				maxZ = Math.Max (maxZ, z + block.zscale + zspacing);
				maxY = Math.Max (maxY, y + block.yscale);

				rowMaxY = Math.Max (rowMaxY, block.yscale);
				z += block.cube.transform.localScale.z + zspacing;
				if (z > (initialZ + spanZ)) {
					z = initialZ;
					y += rowMaxY + yspacing;
					rowMaxY = 0;
				}
				if (y > (initialY + spanY)) {
					moveToNextXRow ();
					rowMaxY = 0;
				}
			}
		}
	}

	static void layOutPackagesInWorld (string[] files)
	{
		// PackageLayoutArea
		Dictionary<string, PackageLayoutArea> areas = new Dictionary<string, PackageLayoutArea> ();

		float xinitial = 0;
		float zinitial = 0;
		float xpos = xinitial;
		float zpos = 0;
		int xspacing = 5; // or xsize of greatest package unit, whichever is greater. 
		// xspacing uses increments of 5 with a minimum of 5.  Math later multiplies by the increment when a box exceeds what fits in its row.
		int zspacing = 50;
		int zMaxUnits = 6;
		float maxXSizeInRow = 0;
		float maxZSizeInRow = 0;

		int packages = 0;


		// Generate all the areas first, setting their files.
		foreach (var filepath in files) {
			CodeBlock block = new CodeBlock (File.ReadAllLines (filepath), filepath); // arbitrary size so we can read the package name.
			PackageLayoutArea area = null;
			if (areas.ContainsKey (block.package)) {
				area = areas [block.package];
			} else {
				area = new PackageLayoutArea (block.package);
				areas [block.package] = area;
			}
			area.addFile (filepath);
			packages++;
		}

		//zMaxUnits = (int) Math.Sqrt (packages) / 4; // determines aspect ratio of entire layout in world.

		// Then lay them out now that we know they're complete.
		// Use a grid layout in the predictable (bounded) direction, adjusting spacing between 
		// rows after the end of each row, to adapt to the uncertain depth in the unbounded direction.
		foreach (PackageLayoutArea area in areas.Values) {
			area.doLayout (xpos, zpos);
			maxXSizeInRow = Math.Max (maxXSizeInRow, area.getXSize());
			zpos += zspacing;
			if (zpos >= (zinitial + (zspacing * zMaxUnits))) {
				zpos = zinitial;
				xpos += Math.Max(xspacing, (int)Math.Ceiling((maxXSizeInRow+1f)/xspacing)*xspacing); // hint of padding so rows don't actually bump.
				maxXSizeInRow = 0;
			}
			if (debugPackageCountLimit != -1 && ++packages > debugPackageCountLimit) {
				break;
			}

		}
	}



	private class CodeBlock
	{
		private int width { get; set; }

		private int height { get; set; }

		public GameObject cube { get; set; }

		public float xscale { get; set; }

		public float yscale { get; set; }

		public float zscale { get; set; }

		public string filename { get; set; }

		public string classname { get; set; }

		public string package { get; set; }

		public List<string> methods = new List<string> ();

		public CodeBlock (string[] lines, string filename)
		{
			package = lines [0].Trim ().Split ('\t') [1];
			classname = lines [1].Trim ().Split ('\t') [1];
			this.filename = filename;

			int w, h;
			if (Int32.TryParse (lines [2].Trim ().Split ('\t') [1], out w)) {
				width = w;
			} else {
				Console.WriteLine ("String could not be parsed: " + lines [2] + ".");
			}

			if (Int32.TryParse (lines [3].Trim ().Split ('\t') [1], out h)) {
				height = h;
			} else {
				Console.WriteLine ("String could not be parsed: " + lines [3] + ".");
			}

			for (var i = 0; i < lines.Count (); i++) {
				methods.Add (lines [i].Trim ().Split ('\t') [1]);
			}
			xscale = 1f;
			yscale = (float)(scale * height);
			zscale = (float)(scale * width);

		}

		public bool isTallBlock ()
		{
			return ((float)height / (float)width > 2.5f);
		}

		public string getPngPathAndFilename ()
		{
			return filename.Substring (0, filename.Length - 4) + ".png";
		}

		public void renderBlock (float xpos, float ypos, float zpos)
		{
			// the xpos and zpos are centered in the block.  
			// the ypos coming in is bottom-aligned already. ypos=0 puts bottom of box on the floor.

			cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
			cube.name = "cube-" + classname;
			cube.transform.localScale = new Vector3 (xscale, yscale, zscale);
			cube.transform.position = new Vector3 (xpos, ypos + 1, zpos);
			cube.GetComponent<BoxCollider> ().enabled = false;

			//Console.WriteLine ("Cube " + classname + " size " + xscale + "," + yscale + "," + zscale + "; pos " + xpos + "," + ypos + "," + zpos);

			// Set up to render texture right-side-up only.
			cube.AddComponent <SignUVTextureMapper> ();
			cube.GetComponent<SignUVTextureMapper> ().Rebuild (xscale, yscale, zscale);

			var resourceImagePath = getPngPathAndFilename ();
			if (!System.IO.File.Exists (resourceImagePath)) {
				throw new Exception ("No File Found: " + resourceImagePath);
			}

			// Load the texture into memory and assign it to the renderer.
			// Texture2D(int width, int height, TextureFormat format, bool mipmap, bool linear)
			// Using mipmaps improved the frame rate from 10-15 fps to 20-30 fps on dense renders.
			// Not sure of the effects of differing texture color depths.  Mipmaps make frame rate more consistent and better though.
			Texture2D t2d = new Texture2D (1, 1, TextureFormat.RGB565, true);
			//Texture2D t2d = new Texture2D (1, 1, TextureFormat.RGB24, false);
			t2d.anisoLevel = 0; // off for performance
			t2d.LoadImage (File.ReadAllBytes (resourceImagePath));
			cube.GetComponent<Renderer> ().material.shader = Shader.Find ("Mobile/Diffuse"); // performance, unsure if it helped much.
			cube.GetComponent<Renderer> ().material.mainTexture = t2d;
			cube.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // prettier and faster
			cube.GetComponent<Renderer> ().receiveShadows = false; // prettier and faster
			cube.GetComponent<MeshRenderer> ().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			cube.GetComponent<MeshRenderer> ().useLightProbes = false;

		}
	}

	// example code
	static void circleOfSpheres ()
	{
		int numberOfObjects = 20;
		float radius = 5f;
		GameObject prefab = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		for (int i = 0; i < numberOfObjects; i++) {
			float angle = i * Mathf.PI * 2 / numberOfObjects;
			Vector3 pos = new Vector3 (Mathf.Cos (angle), 1, Mathf.Sin (angle)) * radius;
			Instantiate (prefab, pos, Quaternion.identity);
		}
		prefab.SetActive (false);
	}

	// example code
	static void wallOfBoxes ()
	{
		for (int y = 0; y < 5; y++) {
			for (int x = 0; x < 5; x++) {
				GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
				//cube.AddComponent<Rigidbody>();
				cube.transform.position = new Vector3 (x * 2, y * 2, 20);
			}
		}
	}

	static void gridLayout (string[] files)
	{
		int initialX = -40;
		int initialY = -30;
		int initialZ = -100;
		int maxY = 80;
		float x = initialX;
		float y = initialY;
		float z = initialZ;
		int boxCount = 0;
		float rowMaxY = 0;
		foreach (var f in files) {
			CodeBlock block = new CodeBlock (File.ReadAllLines (f), f);
			block.renderBlock (x, y, z);
			rowMaxY = Math.Max (rowMaxY, block.cube.transform.localScale.y);
			z += block.cube.transform.localScale.z + 3;
			if (z > -1 * initialZ) {
				z = initialZ;
				y += rowMaxY + 1;
				rowMaxY = 0;
			}
			if (y > maxY) {
				x += block.cube.transform.localScale.x + 10;
				z = initialZ;
				y = initialY;
				rowMaxY = 0;
			}
			boxCount++;
			if (debugBoxCountLimit != -1 && boxCount > debugBoxCountLimit) {
				break;
				// don't overload video memory on my macbook :)
			}
		}
	}





	static void createCodeBlocks ()
	{
		//Application.persistentDataPath   Application.dataPath   Assets/

		try {
			var filepaths = Directory.GetFiles ("/Users/jenny/dev/codevis/Assets/data/", "*.txt", SearchOption.AllDirectories);
			// gridLayout (filepaths);
			layOutPackagesInWorld (filepaths);

			Console.WriteLine ("{0} files found.", filepaths.Count ().ToString ());
		} catch (UnauthorizedAccessException UAEx) {
			Console.WriteLine (UAEx.Message);
		} catch (PathTooLongException PathEx) {
			Console.WriteLine (PathEx.Message);
		}
		
	}
	
	// Use this for initialization
	void Start ()
	{
		UnitySystemConsoleRedirector.Redirect ();
		QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
//		wallOfBoxes ();
//		circleOfSpheres ();
		createCodeBlocks ();		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
