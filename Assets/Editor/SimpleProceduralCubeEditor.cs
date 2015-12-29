using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (SimpleProceduralCube))] 
public class SimpleProceduralCubeEditor : Editor {
	[MenuItem ("GameObject/Create Other/SimpleProceduralCube")]
	static void Create(){
		GameObject gameObject = new GameObject("SimpleProceduralCube");
		SimpleProceduralCube s = gameObject.AddComponent<SimpleProceduralCube>();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		s.Rebuild();
	}
	
	public override void OnInspectorGUI ()
	{
		SimpleProceduralCube obj;
		
		obj = target as SimpleProceduralCube;
		
		if (obj == null)
		{
			return;
		}
		
		base.DrawDefaultInspector();
		EditorGUILayout.BeginHorizontal ();
		
		// Rebuild mesh when user click the Rebuild button
		if (GUILayout.Button("Rebuild")){
			obj.Rebuild();
		}
		EditorGUILayout.EndHorizontal ();
	}
}