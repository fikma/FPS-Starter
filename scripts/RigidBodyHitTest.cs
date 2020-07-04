using Godot;

public class RigidBodyHitTest : RigidBody
{
    private const int BASE_BULLET_BOOST = 9;

    public void BulletHit(int damage, Transform bulletGlobalTransform)
    {
        var directionVector = bulletGlobalTransform.basis.z.Normalized() * BASE_BULLET_BOOST;
        ApplyImpulse(
            (bulletGlobalTransform.origin - GlobalTransform.origin).Normalized(),
            directionVector * damage
        );
    }
}