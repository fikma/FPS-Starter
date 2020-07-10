using Godot;

public class RigidBodyHitTest : RigidBody
{
    private const byte BASE_BULLET_BOOST = 9;

    public void BulletHit(byte damage, Transform bulletGlobalTransform)
    {
        var directionVector = bulletGlobalTransform.basis.z.Normalized() * BASE_BULLET_BOOST;
        ApplyImpulse(
            (bulletGlobalTransform.origin - GlobalTransform.origin).Normalized(),
            directionVector * damage
        );
    }
}