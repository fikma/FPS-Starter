using Godot;

public class WeaponKnife : AbcWeapon
{
    const int DAMAGE = 40;
    public override string IDLE_ANIM_NAME => "Knife_idle";
    public override string FIRE_ANIM_NAME => "Knife_fire";

    public override void FireWeapon()
    {
        Area area = GetNode<Area>("Area");
        var bodies = area.GetOverlappingBodies();

        foreach(var body in bodies)
        {
            if (body == PlayerNode) continue;

            var tmpBody = body as RigidBodyHitTest;
            if (tmpBody.HasMethod("BulletHit"))
                tmpBody.BulletHit(DAMAGE, area.GlobalTransform);
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
}