using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class JumpToFlyForRigidbodyFPC : MonoBehaviour {
    private MonoBehaviour fpc;

	// Use this for initialization
	void Start ()
    {
        fpc = GetComponent("RigidbodyFirstPersonController") as MonoBehaviour;
        if (fpc == null)
        {
            Debug.LogError("JumpToFlyForRigidbodyFPC is on an object with no RigidbodyFirstPersonController script.  Error.");
            return;
        }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("JumpToFlyForRigidbodyFPC is on an object which doesn't have a Rigidbody.  Error.");
            return;
        }

    }


    // Update is called once per frame
    void Update ()
    {
	
	}


    void OnCollision()
    {

    }

    void fly()
    {

    }

    void walk()
    {

    }

}
