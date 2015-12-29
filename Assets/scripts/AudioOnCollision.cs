using UnityEngine;
using System.Collections;

public class AudioOnCollision : MonoBehaviour {

	AudioSource audioSource;

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource>();
	}

	void OnCollisionEnter(Collision col) {
		if (audioSource == null) {
			return; // still initializing
		}
//		if (LeapUtil.isLeapHand(col.rigidbody)) {
			// skip sound, hands are squishy.
//		} else {
			//Debug.Log("Collision with " + col.gameObject + " with tags " + (col.rigidbody != null ? col.rigidbody.tag : ""));
			audioSource.Play();
//		}
	}



	// Update is called once per frame
	void Update () {
	
	}
}
