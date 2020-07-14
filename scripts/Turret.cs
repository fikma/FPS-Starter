using Godot;

public class Turret : Spatial
{
	[Export]
	public bool UseRaycast = false;

	private const byte TURRET_DAMAGE_BULLET = 20;
	private const byte TURRET_DAMAGE_RAYCAST = 5;

	private const float FLASH_TIME = 0.1f;
	private float _flashTimer = 0;

	private const float FIRE_TIME = 0.8f;
	private float _fireTimer = 0;

	private Spatial _nodeHead;
	private RayCast _nodeRaycast;
	private MeshInstance _nodeFlashOne;
	private MeshInstance _nodeFlashTwo;

	private byte _ammoInTurret = 20;
	private const byte AMMO_IN_FULL_TURRET = 20;
	private const byte AMMO_RELOAD_TIME = 4;
	private float _ammoReloadTimer = 0;

	private Player _currentTarget = null;

	private bool _isActive = false;

	private const byte PLAYER_HEIGHT = 3;

	private CPUParticles _smokeParticles;

	private byte _turretHealth = 60;
	private const byte MAX_TURRET_HEALTH = 60;

	private const byte DESTROYED_TIME = 20;
	private float _destroyedTimer = 0;

	private PackedScene _bulletScene = GD.Load<PackedScene>("res://Bullet_Scene.tscn");

	public override void _Ready()
	{
		GetNode<Area>("Vision_Area").Connect("body_entered", this, "BodyEnteredVision");
		GetNode<Area>("Vision_Area").Connect("body_exited", this, "BodyExitedVision");

		_nodeHead = GetNode<Spatial>("Head");
		_nodeRaycast = GetNode<RayCast>("Head/Ray_Cast");
		_nodeFlashOne = GetNode<MeshInstance>("Head/Flash");
		_nodeFlashTwo = GetNode<MeshInstance>("Head/Flash_2");

		_nodeRaycast.AddException(this);
		_nodeRaycast.AddException(GetNode<StaticBody>("Base/Static_Body"));
		_nodeRaycast.AddException(GetNode<StaticBody>("Head/Static_Body"));
		_nodeRaycast.AddException(GetNode<Area>("Vision_Area"));

		_nodeFlashOne.Visible = false;
		_nodeFlashTwo.Visible = false;

		_smokeParticles = GetNode<CPUParticles>("Smoke");
		_smokeParticles.Emitting = false;

		_turretHealth = MAX_TURRET_HEALTH;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (_isActive == true)
			if (_flashTimer > 0)
			{
				_flashTimer -= delta;
				if (_flashTimer <= 0)
				{
					_nodeFlashOne.Visible = false;
					_nodeFlashTwo.Visible = false;
				}
			}

			if (_currentTarget != null)
			{
				_nodeHead.LookAt(_currentTarget.GlobalTransform.origin + new Vector3(0, PLAYER_HEIGHT, 0), Vector3.Up);
				if (_turretHealth > 0)
				{
					if (_ammoInTurret > 0)
					{
						if (_fireTimer > 0)
							_fireTimer -= delta;
						else
							FireBullet();
					}
					else
					{
						if (_ammoReloadTimer > 0)
							_ammoReloadTimer -= 0;
						else
							_ammoInTurret = AMMO_IN_FULL_TURRET;
					}
				}
			}
		if (_turretHealth <= 0)
		{
			if (_destroyedTimer > 0)
				_destroyedTimer -= delta;
			else
			{
				_turretHealth = MAX_TURRET_HEALTH;
				_smokeParticles.Emitting = false;
			}
		}
	}

	private void FireBullet()
	{
		if (UseRaycast == true)
		{
			_nodeRaycast.LookAt(_currentTarget.GlobalTransform.origin + new Vector3(0, PLAYER_HEIGHT, 0), Vector3.Up);
			_nodeRaycast.ForceRaycastUpdate();

			if (_nodeRaycast.IsColliding())
			{
				var body = _nodeRaycast.GetCollider();
				if (body.HasMethod("BulletHit"))
					(body as Player).BulletHit(TURRET_DAMAGE_RAYCAST, _nodeRaycast.GetCollisionPoint());
				
				_ammoInTurret -= 1;
			}
		}
		else
		{
			var clone = _bulletScene.Instance() as BulletScript;
			var sceneRoot = GetTree().Root.GetChildren()[0] as Spatial;
			sceneRoot.AddChild(clone);

			clone.GlobalTransform = GetNode<Spatial>("Head/Barrel_End").GlobalTransform;
			clone.Scale = new Vector3(8, 8, 8);
			clone.BULLET_DAMAGE = TURRET_DAMAGE_BULLET;
			clone.BULLET_SPEED = 60;

			_ammoInTurret -= 1;
		}

		_nodeFlashOne.Visible = true;
		_nodeFlashTwo.Visible = true;

		_flashTimer = FLASH_TIME;
		_fireTimer = FIRE_TIME;

		if (_ammoInTurret <= 0)
			_ammoReloadTimer = AMMO_RELOAD_TIME;
	}

	public void BodyEnteredVision(Node body)
	{
		if (_currentTarget == null)
			if (body is KinematicBody)
			{
				_currentTarget = body as Player;
				_isActive = true;
				GD.Print("got here");
			}
	}

	public void BodyExitedVision(Node body)
	{
		if (_currentTarget != null)
			if (body == _currentTarget)
			{
				_currentTarget = null;
				_isActive = false;

				_flashTimer = 0;
				_fireTimer = 0;
				_nodeFlashOne.Visible = false;
				_nodeFlashTwo.Visible = false;
			}
	}

	internal void BulletHit(byte damage, Vector3 position)
	{
		_turretHealth -= damage;

		if (_turretHealth <= 0)
		{
			_smokeParticles.Emitting = true;
			_destroyedTimer = DESTROYED_TIME;
		}
	}
}
