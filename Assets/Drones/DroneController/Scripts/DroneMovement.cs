using DroneController.Physics;

public class DroneMovement : DroneMovementScript {

    public override void Awake()
    {
        base.Awake(); //I
    }

    void FixedUpdate()
    {

        GetVelocity();
        ClampingSpeedValues();
		SettingControllerToInputSettings();
		if (FlightRecorderOverride == false)
		{
			MovementUpDown();
			MovementLeftRight();
			Rotation();
			MovementForward();
			BasicDroneHoverAndRotation();
		}
	}

    void Update () {
        RotationUpdateLoop_TrickRotation(); 
        Animations(); 
        DroneSound();
        CameraCorrectPickAndTranslatingInputToWSAD();
    }

}
