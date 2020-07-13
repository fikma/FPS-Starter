using Godot;

public class PickupAmmo : AbstractUsableItem
{
    private readonly byte[] AMMO_AMOUNTS = { 3, 1 };
    private readonly byte[] GRENADE_AMOUNTS = { 2, 1 };

    public override void _Ready()
    {
        GetNode<Area>("Holder/Ammo_Pickup_Trigger").Connect("body_entered", this, "TriggerBodyEntered");

        _isReady = true;

        KitSizeChangeValues(0, false);
        KitSizeChangeValues(1, false);

        KitSizeChangeValues(KitSize, true);
    }

    protected override void KitSizeChangeValues(byte size, bool enable)
    {
        switch (size)
        {
            case 0:
                GetNode<CollisionShape>("Holder/Ammo_Pickup_Trigger/Shape_Kit").Disabled = !enable;
                GetNode<Spatial>("Holder/Ammo_Kit").Visible = enable;
                break;
            case 1:
                GetNode<CollisionShape>("Holder/Ammo_Pickup_Trigger/Shape_Kit_Small").Disabled = !enable;
                GetNode<Spatial>("Holder/Ammo_Kit_Small").Visible = enable;
                break;
        }
    }

    protected override void TriggerBodyEntered(Player body)
    {
        if (body.HasMethod("AddAmmo"))
        {
            body.AddAmmo(AMMO_AMOUNTS[KitSize]);
            _respawnTimer = RESPAWN_TIME;
            KitSizeChangeValues(KitSize, false);
        }

        if (body.HasMethod("AddGrenade"))
        {
            body.AddGrenade(GRENADE_AMOUNTS[KitSize]);
            _respawnTimer = RESPAWN_TIME;
            KitSizeChangeValues(KitSize, false);
        }

    }
}
