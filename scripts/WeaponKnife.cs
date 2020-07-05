using Godot;

public class WeaponKnife : AbcWeapon
{
    public override int DAMAGE => 40;
    public override string IDLE_ANIM_NAME => "Knife_idle";
    public override string FIRE_ANIM_NAME => "Knife_fire";

    public override int AMMO_IN_MAG => 1;

    public override bool CAN_RELOAD => false;

    public override bool CAN_REFILL => false;

    public override string RELOAD_ANIM_NAME => "";

    public WeaponKnife()
    {
        AmmoInWeapon = 1;
        SpareAmmo = 1;
    }

    public override void FireWeapon()
    {
        Area area = GetNode<Area>("Area");
        var bodies = area.GetOverlappingBodies();

        foreach(var body in bodies)
        {
            if (body == PlayerNode) continue;

            if ((body as Spatial).HasMethod("BulletHit"))
            {
                var tmpBody = body as RigidBodyHitTest;
                tmpBody.BulletHit(DAMAGE, area.GlobalTransform);
            }
        }
    }

    public override bool EquipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IDLE_ANIM_NAME)
        {
            IsWeaponEnabled = true;
            return true;
        }

        if (PlayerNode.AnimationManager.CurrentState == "Idle_unarmed")
            PlayerNode.AnimationManager.SetAnimation("Knife_equip");

        return false;
    }

    public override bool UnequipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IDLE_ANIM_NAME)
            PlayerNode.AnimationManager.SetAnimation("Knife_unequip");

        if (PlayerNode.AnimationManager.CurrentState == "Idle_unarmed")
        {
            IsWeaponEnabled = false;
            return true;
        }

        return false;
    }

    public override bool ReloadWeapon()
    {
        return false;
    }
}