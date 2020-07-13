using Godot;

public class Grenade : AbstractGrenade
{
	protected override byte GRENADE_DAMAGE => 60;

	protected override byte GRENADE_TIME => 2;

	protected override float EXPLOSION_WAIT_TIME => 0.48f;
	private CollisionShape _rigidShape;
	private MeshInstance _grenadeMesh;
	private Area _blastArea;
	private Particles _explosionParticle;

	public override void _Ready()
	{
		_rigidShape = GetNode<CollisionShape>("Collision_Shape");
		_grenadeMesh = GetNode<MeshInstance>("Grenade");
		_blastArea = GetNode<Area>("Blast_Area");
		_explosionParticle = GetNode<Particles>("Explosion");

		_explosionParticle.Emitting = false;
		_explosionParticle.OneShot = true;
	}

	public override void _PhysicsProcess(float delta)
	{
		if (_grenadeTimer < GRENADE_TIME)
		{
			_grenadeTimer += delta;
			return;
		}
		else
			if (_explosionWaitTimer <= 0)
			{
				_explosionParticle.Emitting = true;

				_grenadeMesh.Visible = false;
				_rigidShape.Disabled = true;
				
				Mode = RigidBody.ModeEnum.Static;

				var bodies = _blastArea.GetOverlappingBodies();
				foreach (var item in bodies)
				{
					if (((Spatial)item).HasMethod("BulletHit"))
					{
						if (item is RigidBodyHitTest aBody)
							aBody.BulletHit(GRENADE_DAMAGE, GlobalTransform);
						else if (item is Target target)
							target.BulletHit(GRENADE_DAMAGE, GlobalTransform);
					}
				}
			}

			if (_explosionWaitTimer < EXPLOSION_WAIT_TIME)
			{
				_explosionWaitTimer += delta;
				if (_explosionWaitTimer >= EXPLOSION_WAIT_TIME)
					QueueFree();
			}
	}
}
