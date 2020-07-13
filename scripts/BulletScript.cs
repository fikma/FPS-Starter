using Godot;

public class BulletScript : Spatial
{
	const byte BULLET_SPEED = 70;
	public byte BULLET_DAMAGE = 15;

	const byte KILL_TIMER = 4;
	private float _timer = 0;

	private bool _hitSomething;

	public override void _Ready()
	{
		GetNode<Area>("Area").Connect("body_entered", this, "Collided");
	}

	public override void _PhysicsProcess(float delta)
	{
		var forwardDir = GlobalTransform.basis.z.Normalized();
		GlobalTranslate(forwardDir * BULLET_SPEED * delta);

		_timer += delta;
		if (_timer >= KILL_TIMER)
			QueueFree();
	}

	public void Collided(PhysicsBody body)
	{
		if (_hitSomething == false)
			if (body.HasMethod("BulletHit"))
			{
				if (body is RigidBodyHitTest aBody)
					aBody.BulletHit(BULLET_DAMAGE, GlobalTransform);
				else if(body is Target bBody)
					bBody.BulletHit(BULLET_DAMAGE, GlobalTransform);

			}

		_hitSomething = true;
		QueueFree();
	}
}
