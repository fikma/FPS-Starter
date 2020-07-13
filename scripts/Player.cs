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

	public Camera Camera;
	private Spatial _rotationalHelper;
	private SpotLight _flashLight;

	float mouseSensitivity = 0.05f;

	public AnimationPlayerManager AnimationManager;

	private string _currentWeaponName = "UNARMED";

	private bool _reloadingWeapon = false;

	private PackedScene _simpleAudioPlayer =
		GD.Load<PackedScene>("res://Simple_Audio_Player.tscn");

	private Dictionary<string, AbstractWeapon> _weapons = new Dictionary<string, AbstractWeapon>
	{
		{ "UNARMED", null }, { "KNIFE", null }, { "RIFLE", null }, { "PISTOL", null },
	};

	private readonly Dictionary<byte, string> WEAPON_NUMBER_TO_NAME = new Dictionary<byte, string>
	{
		{0, "UNARMED"}, {1, "KNIFE"}, {2, "RIFLE"}, {3, "PISTOL"},
	};

	private readonly Dictionary<string, byte> WEAPON_NAME_TO_NUMBER = new Dictionary<string, byte>
	{
		{"UNARMED", 0}, {"KNIFE", 1}, {"RIFLE", 2}, {"PISTOL", 3}
	};

	private bool _changingWeapon = false;
	private string _changingWeaponName = "UNARMED";

	public byte MAX_HEALTH = 150;
	public byte Health = 100;

	private Label _uiStatusLabel;

	private byte JOYPAD_SENSITIVITY = 2;

	private const float JOYPAD_DEADZONE = 0.15f;

	private float _mouseScrollValue = 0;
	private const float MOUSE_SCROLL_SENSITIVITY = 0.08f;

	private readonly Dictionary<string, byte> GRENADE_AMOUNTS = new Dictionary<string, byte>
	{
		{"Grenade", 2}, {"StickyGrenade", 2}
	};

	private string _currentGrenade = "Grenade";
	private PackedScene _grenadeScene = GD.Load("res://Grenade.tscn") as PackedScene;
	private PackedScene _stickyGrenadeScene = GD.Load("res://Sticky_Grenade.tscn") as PackedScene;
	private const byte GRENADE_THROW_FORCE = 50;

	private RigidBodyHitTest _grabbedObject = null;
	private const byte OBJECT_THROW_FORCE = 120;
	private const byte OBJECT_GRAB_DISTANCE = 7;
	private const byte OBJECT_GRAB_RAY_DISTANCE = 10;

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
		if (_grabbedObject == null)
		{
			ProcessChangingWeapons(delta);
			ProcessReloading(delta);
		}
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

		// ========================================================================================
		// ganti senjata dengan scroll wheel
		if (@event is InputEventMouseButton && Input.GetMouseMode() == Input.MouseMode.Captured)
		{
			var e = (InputEventMouseButton)@event;
			if (e.ButtonIndex == (int)ButtonList.WheelUp || e.ButtonIndex == (int)ButtonList.WheelDown)
			{
				if (e.ButtonIndex == (int)ButtonList.WheelUp)
					_mouseScrollValue += MOUSE_SCROLL_SENSITIVITY;
				else if (e.ButtonIndex == (int)ButtonList.WheelDown)
					_mouseScrollValue -= MOUSE_SCROLL_SENSITIVITY;

				_mouseScrollValue = Mathf.Clamp(_mouseScrollValue, 0, WEAPON_NUMBER_TO_NAME.Count - 1);

				if (_changingWeapon == false)
					if (_reloadingWeapon == false)
					{
						var roundScrollValue = (byte)System.Math.Round(_mouseScrollValue);
						if (WEAPON_NUMBER_TO_NAME[roundScrollValue] != _currentWeaponName)
						{
							_changingWeaponName = WEAPON_NUMBER_TO_NAME[roundScrollValue];
							_changingWeapon = true;
							_mouseScrollValue = roundScrollValue;
						}
					}
			}
		}
		// ========================================================================================
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
			var currentWeapon = _weapons[_currentWeaponName];

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
		if (Input.IsActionJustPressed("ui_cancel"))
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

		weaponChangeNumber = (byte)Mathf.Clamp(weaponChangeNumber, 0, WEAPON_NAME_TO_NUMBER.Count - 1);

		if(_changingWeapon == false)
			if (_reloadingWeapon == false)
				if (WEAPON_NUMBER_TO_NAME[weaponChangeNumber] != _currentWeaponName)
				{
					_changingWeaponName = WEAPON_NUMBER_TO_NAME[weaponChangeNumber];
					_changingWeapon = true;
					_mouseScrollValue = weaponChangeNumber;
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

		// ==================================================================================
		// Changing and throwing grenade
		if (Input.IsActionJustPressed("change_grenade"))
			if (_currentGrenade == "Grenade") _currentGrenade = "StickyGrenade";
			else if (_currentGrenade == "StickyGrenade") _currentGrenade = "Grenade";
		
		if (Input.IsActionJustPressed("fire_grenade"))
			if (GRENADE_AMOUNTS[_currentGrenade] > 0)
			{
				GRENADE_AMOUNTS[_currentGrenade] -= 1;
				
				AbstractGrenade grenadeClone = null;
				if (_currentGrenade == "Grenade")
					grenadeClone = _grenadeScene.Instance() as Grenade;
				else if (_currentGrenade == "StickyGrenade")
					grenadeClone = _stickyGrenadeScene.Instance() as StickyGrenade;

				if (grenadeClone != null)
				{
					GetTree().Root.AddChild(grenadeClone);
					grenadeClone.GlobalTransform = GetNode<Spatial>("Rotation_Helper/Grenade_Toss_Pos").GlobalTransform;
					grenadeClone.ApplyImpulse(Vector3.Zero, grenadeClone.GlobalTransform.basis.z * GRENADE_THROW_FORCE);
				}

			}
		// ==================================================================================
		// ==================================================================================
		if (Input.IsActionJustPressed("fire") && _currentWeaponName == "UNARMED")
		{
			if (_grabbedObject == null)
			{
				var state = GetWorld().DirectSpaceState;

				var centerPosition = GetViewport().Size / 2;
				var rayFrom = Camera.ProjectRayOrigin(centerPosition);
				var rayTo = rayFrom + Camera.ProjectRayNormal(centerPosition)  * OBJECT_GRAB_RAY_DISTANCE;

				var rayResult = state.IntersectRay(rayFrom, rayTo, new Array {this, GetNode<Area>("Rotation_Helper/Gun_Fire_Points/Knife_Point/Area")});
				if (rayResult.Count > 0)
					if (rayResult["collider"] is RigidBody)
					{
						_grabbedObject = rayResult["collider"] as RigidBodyHitTest;
						(_grabbedObject as RigidBody).Mode = RigidBody.ModeEnum.Static;
						_grabbedObject.CollisionLayer = 0;
						_grabbedObject.CollisionMask = 0;
					}
			}
			else
			{
				_grabbedObject.Mode = RigidBody.ModeEnum.Rigid;
				_grabbedObject.ApplyImpulse(Vector3.Zero, -Camera.GlobalTransform.basis.z.Normalized() * OBJECT_THROW_FORCE);
				_grabbedObject.CollisionLayer = 1;
				_grabbedObject.CollisionMask = 1;

				_grabbedObject = null;
			}
		}

		if (_grabbedObject != null)
		{
            Transform globalTransform = _grabbedObject.GlobalTransform;
            globalTransform.origin = Camera.GlobalTransform.origin + (-Camera.GlobalTransform.basis.z.Normalized() * OBJECT_GRAB_DISTANCE);
			_grabbedObject.GlobalTransform = globalTransform;
		}
		// ==================================================================================
	}

	private void ProcessUI(float delta)
	{
		if (_currentWeaponName == "UNARMED" || _currentWeaponName == "KNIFE")
			_uiStatusLabel.Text = "Health: " + GD.Str(Health) + "\n" + _currentGrenade + ": " + GRENADE_AMOUNTS[_currentGrenade].ToString();
		else
		{
			var currentWeapon = _weapons[_currentWeaponName];
			_uiStatusLabel.Text = "Health" + GD.Str(Health) +
				"\nAmmo: " + GD.Str(currentWeapon.AmmoInWeapon) + "/" + GD.Str(currentWeapon.SpareAmmo) +
				"\n" + _currentGrenade + ": " + GRENADE_AMOUNTS[_currentGrenade].ToString();
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

	public void CreateSound(string soundName, Vector3 position = new Vector3())
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

	public void AddHealth(byte amount)
	{
		Health += amount;
		Health = (byte)Mathf.Clamp(Health, 0, MAX_HEALTH);
	}

	public void AddAmmo(byte amount)
	{
		if (_currentWeaponName != "UNARMED")
			if (_weapons[_currentWeaponName].CAN_REFILL == true)
				_weapons[_currentWeaponName].SpareAmmo += (ushort)(_weapons[_currentWeaponName].AMMO_IN_MAG * amount);
	}

	public void AddGrenade(byte additionalGrenade)
	{
		GRENADE_AMOUNTS[_currentGrenade] += additionalGrenade;
		GRENADE_AMOUNTS[_currentGrenade] = (byte)Mathf.Clamp(GRENADE_AMOUNTS[_currentGrenade], 0, 4);
	}
}
