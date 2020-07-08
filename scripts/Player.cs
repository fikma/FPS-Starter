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

	internal Camera Camera;
	private Spatial _rotationalHelper;
	private SpotLight _flashLight;

	float mouseSensitivity = 0.05f;

	internal AnimationPlayerManager AnimationManager;

	private string _currentWeaponName = "UNARMED";

	private bool _reloadingWeapon = false;

    private PackedScene _simpleAudioPlayer =
        GD.Load<PackedScene>("res://Simple_Audio_Player.tscn");

	private Dictionary<string, AbcWeapon> _weapons = new Dictionary<string, AbcWeapon>
	{
		{ "UNARMED", null }, { "KNIFE", null }, { "RIFLE", null }, { "PISTOL", null },
	};

	private readonly Dictionary<int, string> WEAPON_NUMBER_TO_NAME = new Dictionary<int, string>
	{
		{0, "UNARMED"}, {1, "KNIFE"}, {2, "RIFLE"}, {3, "PISTOL"},
	};

	private readonly Dictionary<string, int> WEAPON_NAME_TO_NUMBER = new Dictionary<string, int>
	{
		{"UNARMED", 0}, {"KNIFE", 1}, {"RIFLE", 2}, {"PISTOL", 3}
	};

	private bool _changingWeapon = false;
	private string _changingWeaponName = "UNARMED";

	public int Health = 100;

	private Label _uiStatusLabel;

	private int JOYPAD_SENSITIVITY = 2;

	private const float JOYPAD_DEADZONE = 0.15f;

	public override void _Ready()
	{
		Camera = GetNode<Camera>("Rotation_Helper/Camera");
		_rotationalHelper = GetNode<Spatial>("Rotation_Helper");

		AnimationManager = GetNode<AnimationPlayerManager>("Rotation_Helper/Model/Animation_Player");
		AnimationManager.CallbackFunction = FireBullet;

		Input.SetMouseMode(Input.MouseMode.Captured);

		_weapons["KNIFE"] = GetNode<WeaponKnife>("Rotation_Helper/Gun_Fire_Points/Knife_Point");
		_weapons["RIFLE"] = GetNode<WeaponRifle>("Rotation_Helper/Gun_Fire_Points/Rifle_Point");
		_weapons["PISTOL"] = GetNode<WeaponPistol>("Rotation_Helper/Gun_Fire_Points/Pistol_Point");

		var gunAimPointPos = GetNode<Spatial>("Rotation_Helper/Gun_Aim_Point").GlobalTransform.origin;

		foreach(var weapon in _weapons.Values)
		{
			if (weapon != null)
			{
				weapon.PlayerNode = this;
				weapon.LookAt(gunAimPointPos, Vector3.Up);
				weapon.RotateObjectLocal(Vector3.Up, Mathf.Deg2Rad(180));
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
		ProcessViewInput(delta);
		ProcessChangingWeapons(delta);
		ProcessReloading(delta);
		ProcessUI(delta);
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

	private void FireBullet()
	{
		if (_changingWeapon == true) return;

		_weapons[_currentWeaponName].FireWeapon();
	}

	private void ProcessChangingWeapons(float delta)
	{
		if (_changingWeapon == true)
		{
			var weaponUnequiped = false;
			var currentWeapon = _weapons[_currentWeaponName] as AbcWeapon;

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
				var weaponToEquip = _weapons[_changingWeaponName];

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
		// ========================================================
		// berjalan
		direction = new Vector3();
		Transform camXform = Camera.GlobalTransform;

		Vector2 inputMovementVector = new Vector2();

		if (Input.IsActionPressed("movement_forward"))
			inputMovementVector.y += 1;
		if (Input.IsActionPressed("movement_backward"))
			inputMovementVector.y -= 1;
		if (Input.IsActionPressed("movement_left"))
			inputMovementVector.x -= 1;
		if (Input.IsActionPressed("movement_right"))
			inputMovementVector.x += 1;

		if (Input.GetConnectedJoypads().Count > 0)
		{
			var joypadVec = Vector2.Zero;
			var osName = OS.GetName();

			if (osName == "Windows" || osName == "X11")
				joypadVec = new Vector2(Input.GetJoyAxis(0, 0), -Input.GetJoyAxis(0, 1));
			else if (osName == "OSX")
				joypadVec = new Vector2(Input.GetJoyAxis(0, 1), Input.GetJoyAxis(0, 2));
			
			if (joypadVec.Length() < JOYPAD_DEADZONE)
				joypadVec = Vector2.Zero;
			else
				joypadVec = joypadVec.Normalized() * ((joypadVec.Length() - JOYPAD_DEADZONE) / (1 - JOYPAD_DEADZONE));
			
			inputMovementVector += joypadVec;
		}

		inputMovementVector = inputMovementVector.Normalized();
		// ========================================================

		// ========================================================
		// basis vector sudah ter-normalized
		direction += -camXform.basis.z * inputMovementVector.y;
		direction += camXform.basis.x * inputMovementVector.x;
		// ========================================================

		// ========================================================
		// melompat
		if (IsOnFloor())
			if (Input.IsActionPressed("movement_jump"))
				_velocity.y = JUMP_SPEED;
		// ========================================================

		// ========================================================
		// menangkap/melepaskan cursor
		if (Input.IsActionPressed("ui_cancel"))
		{
			if (Input.GetMouseMode().Equals(Input.MouseMode.Captured))
				Input.SetMouseMode(Input.MouseMode.Captured);
			else
				Input.SetMouseMode(Input.MouseMode.Visible);
		}
		// ========================================================

		// ===============================================================
		// Sprinting
		if (Input.IsActionPressed("movement_sprint")) _isSprinting = true;
		else _isSprinting = false;
		// ===============================================================

		// Turning the flashlight
		if (Input.IsActionJustPressed("flashlight"))
		{
			if (_flashLight.IsVisibleInTree()) _flashLight.Hide();
			else _flashLight.Show();
		}

		// =======================================================================================
		// process changing weapon
		var weaponChangeNumber = WEAPON_NAME_TO_NUMBER[_currentWeaponName];

		if (Input.IsKeyPressed((int)KeyList.Key1))
			weaponChangeNumber = 0;
		if (Input.IsKeyPressed((int)KeyList.Key2))
			weaponChangeNumber = 1;
		if (Input.IsKeyPressed((int)KeyList.Key3))
			weaponChangeNumber = 2;
		if (Input.IsKeyPressed((int)KeyList.Key4))
			weaponChangeNumber = 3;

		if (Input.IsActionJustPressed("shift_weapon_positive"))
			weaponChangeNumber += 1;
		if (Input.IsActionJustPressed("shift_weapon_negative"))
			weaponChangeNumber -= 1;

		weaponChangeNumber = Mathf.Clamp(weaponChangeNumber, 0, WEAPON_NAME_TO_NUMBER.Count - 1);

		if(_changingWeapon == false)
			if (_reloadingWeapon == false)
				if (WEAPON_NUMBER_TO_NAME[weaponChangeNumber] != _currentWeaponName)
				{
					_changingWeaponName = WEAPON_NUMBER_TO_NAME[weaponChangeNumber];
					_changingWeapon = true;
				}
		// ========================================================================================

		// ========================================================================================
		// firing the weapon
		if (Input.IsActionPressed("fire"))
			if (_reloadingWeapon == false)
				if (_changingWeapon == false)
				{
					var currentWeapon = _weapons[_currentWeaponName];
					if (currentWeapon != null)
						if (currentWeapon.AmmoInWeapon > 0)
						{
							if (AnimationManager.CurrentState == currentWeapon.IDLE_ANIM_NAME)
								AnimationManager.SetAnimation(currentWeapon.FIRE_ANIM_NAME);
							else
								_reloadingWeapon = true;
						}
				}
		// ================================================================================

		//=================================================================================
		// realoading weapon
		if (_reloadingWeapon == false)
			if (_changingWeapon == false)
				if (Input.IsActionJustPressed("reload"))
				{
					var currentWeapon = _weapons[_currentWeaponName];
					if (currentWeapon != null)
						if (currentWeapon.CAN_RELOAD == true)
						{
							var currentAnimState = AnimationManager.CurrentState;
							var isReloading = false;

							foreach(var weapon in _weapons.Values)
							{
								if (weapon != null)
									if (currentAnimState == weapon.RELOAD_ANIM_NAME)
										isReloading = true;
							}

							if (isReloading == false)
								_reloadingWeapon = true;
						}
				}
		// ==================================================================================
	}

	private void ProcessUI(float delta)
	{
		if (_currentWeaponName == "UNARMED" || _currentWeaponName == "KNIFE")
			_uiStatusLabel.Text = "Health: " + GD.Str(Health);
		else
		{
			var currentWeapon = _weapons[_currentWeaponName];
			_uiStatusLabel.Text = "Health" + GD.Str(Health) +
				"\nAmmo: " + GD.Str(currentWeapon.AmmoInWeapon) + "/" + GD.Str(currentWeapon.SpareAmmo);
		}
	}

	private void ProcessReloading(float delta)
	{
		if(_reloadingWeapon == true)
		{
			var currentWeapon = _weapons[_currentWeaponName];
			if (currentWeapon != null)
				currentWeapon.ReloadWeapon();
			_reloadingWeapon = false;
		}
	}

    internal void CreateSound(string soundName, Vector3 position = new Vector3())
    {
        var audioClone = _simpleAudioPlayer.Instance() as SimpleAudioPlayer;
        var sceneRoot = GetTree().Root.GetChildren()[0] as Spatial;
        sceneRoot.AddChild(audioClone);
        audioClone.PlaySound(soundName, position);
    }

	private void ProcessViewInput(float delta)
	{
		if (Input.GetMouseMode() == Input.MouseMode.Captured)
			return;
		

		if (Input.GetConnectedJoypads().Count > 0)
		{
			var joypadVec = Vector2.Zero;
			var osName = OS.GetName();
		
			if (osName == "Windows" || osName == "X11")
				joypadVec = new Vector2(Input.GetJoyAxis(0, 2), Input.GetJoyAxis(0, 3));
			else if (osName == "OSX")
				joypadVec = new Vector2(Input.GetJoyAxis(0, 3), Input.GetJoyAxis(0, 4));
			
			if (joypadVec.Length() < JOYPAD_DEADZONE)
				joypadVec = Vector2.Zero;
			else
				joypadVec = joypadVec.Normalized() * ((joypadVec.Length() - JOYPAD_DEADZONE) / (1 - JOYPAD_DEADZONE));
			
			_rotationalHelper.RotateX(Mathf.Deg2Rad(joypadVec.y * JOYPAD_SENSITIVITY));

			RotateY(Mathf.Deg2Rad(joypadVec.x * JOYPAD_SENSITIVITY * -1));

			var cameraRotation = _rotationalHelper.RotationDegrees;
			cameraRotation.x = Mathf.Clamp(cameraRotation.x, -70, 70);
			_rotationalHelper.RotationDegrees = cameraRotation;
		}
	}
}
