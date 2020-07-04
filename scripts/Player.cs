using System;
using Godot;
using Godot.Collections;

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

	public AnimationPlayerManager AnimationManager;

	private string _currentWeaponName = "UNARMED";

	private Dictionary<string, Spatial> _weapons = new Dictionary<string, Spatial>
	{
		{ "UNARMED", null }, { "KNIFE", null },
	};

	private readonly Dictionary<int, string> WEAPON_NUMBER_TO_NAME = new Dictionary<int, string>
	{
		{0, "UNARMED"}, {1, "KNIFE"}
	};

	private readonly Dictionary<string, int> WEAPON_NAME_TO_NUMBER = new Dictionary<string, int>
	{
		{"UNARMED", 0}, {"KNIFE", 1}
	};

	private bool _changingWeapon = false;
	private string _changingWeaponName = "UNARMED";

	public int Health = 100;

	private Label _uiStatusLabel;

	public override void _Ready()
	{
		_camera = GetNode<Camera>("Rotation_Helper/Camera");
		_rotationalHelper = GetNode<Spatial>("Rotation_Helper");

		AnimationManager = GetNode<AnimationPlayerManager>("Rotation_Helper/Model/Animation_Player");
		AnimationManager.CallbackFunction = FireBullet;

		Input.SetMouseMode(Input.MouseMode.Captured);

		_weapons["KNIFE"] = GetNode<WeaponKnife>("Rotation_Helper/Gun_Fire_Points/Knife_Point");

		var gunAimPointPos = GetNode<Spatial>("Rotation_Helper/Gun_Aim_Point").GlobalTransform.origin;

		foreach(var weapon in _weapons.Keys)
		{
			Spatial weaponNode = _weapons[weapon];
			if (weaponNode != null)
			{
				var aWeaponNode = weaponNode as WeaponKnife;
				aWeaponNode.PlayerNode = this;
				aWeaponNode.LookAt(gunAimPointPos, Vector3.Up);
				aWeaponNode.RotateObjectLocal(Vector3.Up, Mathf.Deg2Rad(180));
			}
		}

		_currentWeaponName = "UNARMED";
		_changingWeaponName = "UNARMED";

		_uiStatusLabel = GetNode<Label>("HUD/Panel/Gun_label");
		_flashLight = GetNode<SpotLight>("Rotation_Helper/Flashlight");
	}

	public override void _PhysicsProcess(float delta)
	{
		ProcessInput(delta);
		ProcessMovement(delta);
		ProcessChangingWeapons(delta);
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

	public void FireBullet()
	{
		if (_changingWeapon == true) return;

		(_weapons[_currentWeaponName] as WeaponKnife).FireWeapon();
	}

	private void ProcessChangingWeapons(float delta)
	{
		if (_changingWeapon == true)
		{
			var weaponUnequiped = false;
			var currentWeapon = _weapons[_currentWeaponName] as WeaponKnife;

			if (currentWeapon == null)
				weaponUnequiped = true;
			else
			{
				if (currentWeapon.IsWeaponEnabled == true)
					weaponUnequiped = currentWeapon.UnequipWeapon();
				else
					weaponUnequiped = true;
			}

			if (weaponUnequiped == true)
			{
				var weaponEquiped = false;
				var weaponToEquip = _weapons[_changingWeaponName] as WeaponKnife;

				if (weaponToEquip == null)
					weaponEquiped = true;
				else
				{
					if (weaponToEquip.IsWeaponEnabled == false)
						weaponEquiped = weaponToEquip.EquipWeapon();
					else
						weaponEquiped = true;

				}

				if (weaponEquiped == true)
				{
					_changingWeapon = false;
					_currentWeaponName = _changingWeaponName;
					_changingWeaponName = "";
				}
			}
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
		if (Input.IsActionJustPressed("flashlight"))
		{
			if (_flashLight.IsVisibleInTree()) _flashLight.Hide();
			else _flashLight.Show();
		}

		// process changing weapon
		var weaponChangeNumber = WEAPON_NAME_TO_NUMBER[_currentWeaponName];

		if (Input.IsKeyPressed((int)KeyList.Key1))
			weaponChangeNumber = 0;
		if (Input.IsKeyPressed((int)KeyList.Key2))
			weaponChangeNumber = 1;

		if (Input.IsActionJustPressed("shift_weapon_positive"))
			weaponChangeNumber += 1;
		if (Input.IsActionJustPressed("shift_weapon_negative"))
			weaponChangeNumber -= 1;

		weaponChangeNumber = Mathf.Clamp(weaponChangeNumber, 0, WEAPON_NAME_TO_NUMBER.Count - 1);

		if(_changingWeapon.Equals(false))
			if (!WEAPON_NUMBER_TO_NAME[weaponChangeNumber].Equals(_currentWeaponName))
			{
				_changingWeaponName = WEAPON_NUMBER_TO_NAME[weaponChangeNumber];
				_changingWeapon = true;
			}

		// firing the weapon
		if (Input.IsActionPressed("fire"))
			if (_changingWeapon.Equals(false))
			{
				var currentWeapon = _weapons[_currentWeaponName] as WeaponKnife;
				if (currentWeapon != null)
					if (AnimationManager.CurrentState == currentWeapon.IdleAnimName)
						AnimationManager.SetAnimation(currentWeapon.FireAnimName);
			}
	}
}
