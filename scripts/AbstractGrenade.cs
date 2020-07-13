using Godot;

public abstract class AbstractGrenade : RigidBody
{
    protected abstract byte GRENADE_DAMAGE { get; }
    protected abstract byte GRENADE_TIME { get; }
    protected float _grenadeTimer = 0;

    protected abstract float EXPLOSION_WAIT_TIME { get; }

    protected float _explosionWaitTimer = 0;
}
