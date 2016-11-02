using UnityEngine;
using System.Collections;

public class PinRotationToCamera : MonoBehaviour {
    public Transform CopyRotationFrom;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.localEulerAngles = new Vector3(0f, CopyRotationFrom.localEulerAngles.y, 0f);
    }
}
