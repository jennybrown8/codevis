using UnityEngine; 
using System.Collections;
using System.Collections.Generic;

/**
 * Based on a squashed cube, creates a signboard with a right-side-up textured face
 * and narrow edges (narrow in the X scale).
 */
[RequireComponent (typeof (MeshFilter))] 
[RequireComponent (typeof (MeshRenderer))]
public class SignUVTextureMapper : MonoBehaviour {
	public void Rebuild(float xboxsize, float yboxsize, float zboxsize) {
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		Mesh mesh = new Mesh ();
		meshFilter.mesh = mesh;

		float xsize = 1;
		float ysize = 1;
		float zsize = 1;

		mesh.vertices = new Vector3[]{
			// face 1 (xy plane, z=0)
			new Vector3(0,0,0), 
			new Vector3(xsize,0,0), 
			new Vector3(xsize,ysize,0), 
			new Vector3(0,ysize,0), 
			// face 2 (zy plane, x=1)
			new Vector3(xsize,0,0), 
			new Vector3(xsize,0,zsize), 
			new Vector3(xsize,ysize,zsize), 
			new Vector3(xsize,ysize,0), 
			// face 3 (xy plane, z=1)
			new Vector3(xsize,0,zsize), 
			new Vector3(0,0,zsize), 
			new Vector3(0,ysize,zsize), 
			new Vector3(xsize,ysize,zsize), 
			// face 4 (zy plane, x=0)
			new Vector3(0,0,zsize), 
			new Vector3(0,0,0), 
			new Vector3(0,ysize,0), 
			new Vector3(0,ysize,zsize), 
			// face 5  (zx plane, y=1)
			new Vector3(0,ysize,0), 
			new Vector3(xsize,ysize,0), 
			new Vector3(xsize,ysize,zsize), 
			new Vector3(0,ysize,zsize), 
			// face 6 (zx plane, y=0)
			new Vector3(0,0,0), 
			new Vector3(0,0,zsize), 
			new Vector3(xsize,0,zsize), 
			new Vector3(xsize,0,0), 
		};
		
		int faces = 6; // here a face = 2 triangles
		
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		
		for (int i = 0; i < faces; i++) {
			int triangleOffset = i*4;
			triangles.Add(0+triangleOffset);
			triangles.Add(2+triangleOffset);
			triangles.Add(1+triangleOffset);
			
			triangles.Add(0+triangleOffset);
			triangles.Add(3+triangleOffset);
			triangles.Add(2+triangleOffset);
			
			if (i == 0 || i == 2 || i == 4 || i == 5) {
				// take a tiny corner (usually white) for narrow edges
				uvs.Add(new Vector2(0f,0f));
				uvs.Add(new Vector2(0.0f,0f));
				uvs.Add(new Vector2(0.0f,0.0f));
				uvs.Add(new Vector2(0f,0.0f));
				
				//uvs.Add(new Vector2(0f,0f));
				//uvs.Add(new Vector2(0.1f,0f));
				//uvs.Add(new Vector2(0.1f,0.1f));
				//uvs.Add(new Vector2(0f,0.1f));
			} else {
				// use the whole texture as a large face
				uvs.Add(new Vector2(0,0));
				uvs.Add(new Vector2(1,0));
				uvs.Add(new Vector2(1,1));
				uvs.Add(new Vector2(0,1));
			}
		}
		
		mesh.triangles = triangles.ToArray();
		
		mesh.uv = uvs.ToArray();
		
		GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
		
		mesh.RecalculateNormals(); 
		mesh.RecalculateBounds (); 
		mesh.Optimize();
	} 
}