using Godot;

public abstract class AbstractUsableItem : Spatial 
{
    [Export(PropertyHint.Enum, "full size,small")]
    public byte KitSize { get => _kitSize; set => KitSizeChange(value); }
    private byte _kitSize = 0;

    protected bool _isReady = false;

    protected const byte RESPAWN_TIME = 20;
    protected float _respawnTimer = 0f;

    protected void KitSizeChange(byte value)
    {
        if (_isReady == true)
        {
            KitSizeChangeValues(KitSize, false);
            _kitSize = value;

            KitSizeChangeValues(KitSize, true);
        }
        else
            _kitSize = value;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_respawnTimer > 0)
        {
            _respawnTimer -= delta;
            if (_respawnTimer <= 0)
                KitSizeChangeValues(KitSize, true);
        }
    }

    protected abstract void KitSizeChangeValues(byte size, bool enable);

    protected abstract void TriggerBodyEntered(Player body);
}
