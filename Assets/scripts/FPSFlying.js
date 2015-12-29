// Minecraft style flight controller. Jump once, then jump while jumping to initiate flying
// (basically, double-tap space bar).

// You must have the CharacterMotor and FPSInputController on your player object for this to work,
// plus map an UpDown axis in the Edit>Project Settings>Input manager.
// Typical would be to set Positive key to "space" and Negative key to "left shift".
var characterController : CharacterController;
var moveDirection = Vector3.zero;
var horizontalSpeed = 20.0;
var verticalSpeed = 6.0;
function Awake () {
	characterController = GetComponent(CharacterController);
}
function FixedUpdate() {
	var yval = 0.0;
	if(Input.GetButton("UpDown")) {
		// if user is holding down the up or down movement key, go ahead and give velocity.
		// if they stop holding the key, the character immediately stops moving.
		yval = Input.GetAxis("UpDown");
	}
	moveDirection = new Vector3(Input.GetAxis("Horizontal"), yval, Input.GetAxis("Vertical"));
	moveDirection = transform.TransformDirection(moveDirection);
	moveDirection.x *= horizontalSpeed;
	moveDirection.z *= horizontalSpeed;
	moveDirection.y *= verticalSpeed;
		
	var flags = characterController.Move(moveDirection * Time.deltaTime);
}
@script RequireComponent (CharacterController)
