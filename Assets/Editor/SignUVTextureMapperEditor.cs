using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (SignUVTextureMapper))] 
public class SignUVTextureMapperEditor : Editor {
	[MenuItem ("GameObject/Create Other/SignUVTextureMapper")]
	static void Create(){
		GameObject gameObject = new GameObject("SignUVTextureMapper");
		SignUVTextureMapper s = gameObject.AddComponent<SignUVTextureMapper>();
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		s.Rebuild(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z );
	}
	
	public override void OnInspectorGUI ()
	{
		SignUVTextureMapper obj;
		
		obj = target as SignUVTextureMapper;
		
		if (obj == null)
		{
			return;
		}
		
		base.DrawDefaultInspector();
		EditorGUILayout.BeginHorizontal ();
		
		// Rebuild mesh when user click the Rebuild button
		if (GUILayout.Button("Rebuild")){
			obj.Rebuild(obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z );
		}
		EditorGUILayout.EndHorizontal ();
	}
}