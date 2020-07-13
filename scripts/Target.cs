using Godot;

public class Target : StaticBody
{
	private const sbyte TARGET_HEALTH = 40;

	private sbyte _currentHealth = 40;

	private Spatial _brokenTargetHolder;

	private CollisionShape _targetCollisionShape;

	private const byte TARGET_RESPAWN_TIME = 14;

	private float _targetRespawnTimer = 0;

	[Export]
	public PackedScene DestroyedTarget;

	
	private Label _healthInfo;

	public override void _Ready()
	{
		_brokenTargetHolder = GetParent().GetNode<Spatial>("Broken_Target_Holder");
		_targetCollisionShape = GetNode<CollisionShape>("Collision_Shape");
		_healthInfo = GetParent().GetNode<Label>("Label");
	}

	public override void _PhysicsProcess(float delta)
	{
			_healthInfo.Text = _currentHealth.ToString();
		if (_targetRespawnTimer > 0)
		{
			_targetRespawnTimer -= delta;
			if (_targetRespawnTimer <= 0)
			{
				foreach (var child in _brokenTargetHolder.GetChildren())
				{
					if (child is Spatial aChild)
						aChild.QueueFree();
				}

				_targetCollisionShape.Disabled = false;
				Visible = true;
				_currentHealth = TARGET_HEALTH; 

			}
		}
	}

	public void BulletHit(byte damage, Transform bulletTransform)
	{
		_currentHealth -= (sbyte)damage;

		if (_currentHealth <= 0)
		{
			if (DestroyedTarget.Instance() is Spatial clone)
			{
				_brokenTargetHolder.AddChild(clone);

				foreach (var rigid in clone.GetChildren())
				{
					if (rigid is RigidBody aRigid)
					{
						var centerInRigidSpace = _brokenTargetHolder.GlobalTransform.origin - aRigid.GlobalTransform.origin;
						var direction = (aRigid.Transform.origin - centerInRigidSpace).Normalized();

						aRigid.ApplyImpulse(centerInRigidSpace, direction * 12 * damage);
					}
				}

				_targetRespawnTimer = TARGET_RESPAWN_TIME;

				_targetCollisionShape.Disabled = true;
				Visible = false;
			}
		}
	}
}
