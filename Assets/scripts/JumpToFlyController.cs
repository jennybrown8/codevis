using UnityEngine;
using System.Collections;


public class JumpToFlyController : MonoBehaviour {
	private CharacterMotor walkingMotor;
	private MonoBehaviour walkingScript;
	private MonoBehaviour flyingScript;
	private CharacterController characterController;

	private bool isFlying;
	//private bool isScriptEnableDone;

	// Use this for initialization
	void Start () {
		isFlying = false;
		//isScriptEnableDone = false;
		walkingMotor = GetComponent<CharacterMotor>();
		characterController = GetComponent<CharacterController>();
		flyingScript = GetComponent ("FPSFlying") as MonoBehaviour;
		walkingScript = GetComponent ("FPSInputController") as MonoBehaviour;

		if (walkingScript == null || flyingScript == null || walkingMotor == null) {
			Debug.LogError("WalkFlyMaster cannot operate unless its parent object has a CharacterController, " + 
			               "a CharacterMotor, an FPSInputController, and an FPSFlyer.");
			return;
		}
		walk();

	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		// If on the lower 10% of the body, count it as a feet-landing hit.
		if(isFlying && (hit.point.y - characterController.transform.position.y  < (-1 * characterController.height * 0.90))) {
			isFlying = false;
		}

		// but terrain is not a rigidbody...
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic) {
			return;
		}
	}

	private void fly() {
		walkingMotor.enabled=false;
		walkingScript.enabled=false;
		flyingScript.enabled=true;
	}
	private void walk() {
		flyingScript.enabled=false;
		walkingMotor.enabled=true;
		walkingScript.enabled=true;
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetButtonDown("Jump") && walkingMotor.IsJumping()) {
			isFlying = true;
			walkingMotor.SetVelocity(new Vector3(0, 0));
			fly();
		}
		if (!isFlying) {
			walk();
		}
	
	}
}
