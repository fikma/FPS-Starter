using Godot;

public class Player : KinematicBody
{
	[Export]
	public float GRAVITY = -24.8f;
	[Export]
	public int MAX_SPEED = 20;
	[Export]
	public int JUMP_SPEED = 18;
	[Export]
	public float ACCELERATE = 4.5f;
	[Export]
	public float DEACCELERATE = 16.5f;
	[Export]
	public float MAX_SLOPE_ANGLE = 40;
    [Export]
    public float MaxSprintSpeed = 30.0f;
    [Export]
    public float SprintAccel = 18.0f;

    private bool _isSprinting = false;

	private Vector3 _velocity = new Vector3();
	Vector3 direction = new Vector3();

	private Camera _camera;
	private Spatial _rotationalHelper;
    private SpotLight _flashLight;

	float mouseSensitivity = 0.05f;

	public override void _Ready()
	{
		_camera = GetNode<Camera>("Rotation_Helper/Camera");
		_rotationalHelper = GetNode<Spatial>("Rotation_Helper");
        _flashLight = GetNode<SpotLight>("Rotation_Helper/Flashlight");

		Input.SetMouseMode(Input.MouseMode.Captured);
	}

	public override void _PhysicsProcess(float delta)
	{
		ProcessInput(delta);
		ProcessMovement(delta);
	}

	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseMotion && Input.GetMouseMode().Equals(Input.MouseMode.Captured))
		{
			var mouseEvent = (InputEventMouseMotion)@event;
			_rotationalHelper.RotateX(Mathf.Deg2Rad(mouseEvent.Relative.y * mouseSensitivity));
			RotateY(Mathf.Deg2Rad(-mouseEvent.Relative.x * mouseSensitivity));

			var cameraRotation = _rotationalHelper.RotationDegrees;
			cameraRotation.x = Mathf.Clamp(cameraRotation.x, -70, 70);
			_rotationalHelper.RotationDegrees = cameraRotation;
		}
	}

	private void ProcessMovement(float delta)
	{
		direction.y = 0;
		direction = direction.Normalized();

		_velocity.y += delta * GRAVITY;

		var horizontalVelocity = _velocity;
		horizontalVelocity.y = 0;

		var target = direction;
        if (_isSprinting)
            target *= MaxSprintSpeed;
        else
            target *= MAX_SPEED;

		float accel;
        if (direction.Dot(horizontalVelocity) > 0)
        {
            if (_isSprinting)
                accel = SprintAccel;
            else
                accel = ACCELERATE;
        }
		else
			accel = DEACCELERATE;

		horizontalVelocity =
			horizontalVelocity.LinearInterpolate(target, accel * delta);

		_velocity.x = horizontalVelocity.x;
		_velocity.z = horizontalVelocity.z;
		_velocity = MoveAndSlide(
			_velocity,
			Vector3.Up,
			false,
			4,
			Mathf.Deg2Rad(MAX_SLOPE_ANGLE));
	}

	private void ProcessInput(float delta)
	{
		// berjalan
		direction = new Vector3();
		Transform camXform = _camera.GlobalTransform;

		Vector2 inputMovementVector = new Vector2();

		if (Input.IsActionPressed("movement_forward"))
			inputMovementVector.y += 1;
		if (Input.IsActionPressed("movement_backward"))
			inputMovementVector.y -= 1;
		if (Input.IsActionPressed("movement_left"))
			inputMovementVector.x -= 1;
		if (Input.IsActionPressed("movement_right"))
			inputMovementVector.x += 1;

		inputMovementVector = inputMovementVector.Normalized();

		// basis vector sudah ter-normalized
		direction += -camXform.basis.z * inputMovementVector.y;
		direction += camXform.basis.x * inputMovementVector.x;

		// melompat
		if (IsOnFloor())
			if (Input.IsActionPressed("movement_jump"))
				_velocity.y = JUMP_SPEED;

		// menangkap/melepaskan cursor
		if (Input.IsActionPressed("ui_cancel"))
		{
			if (Input.GetMouseMode().Equals(Input.MouseMode.Captured))
				Input.SetMouseMode(Input.MouseMode.Captured);
			else
				Input.SetMouseMode(Input.MouseMode.Visible);
		}

        // Sprinting
        if (Input.IsActionPressed("movement_sprint")) _isSprinting = true;
        else _isSprinting = false;

        // Turning the flashlight
        if (Input.IsActionPressed("flashlight"))
        {
            if (_flashLight.IsVisibleInTree()) _flashLight.Hide();
            else _flashLight.Show();
        }
    }
}
