using Godot;

public class WeaponKnife : Spatial
{
    const int DAMAGE = 40;

    private const string IDLE_ANIM_NAME = "Knife_idle";
    public string IdleAnimName { get => IDLE_ANIM_NAME; }
    private const string FIRE_ANIM_NAME = "Knife_fire";
    public string FireAnimName { get => FIRE_ANIM_NAME; }

    public bool IsWeaponEnabled = false;

    public Player PlayerNode = null;

    public void FireWeapon()
    {
        Area area = GetNode<Area>("Area");
        var bodies = area.GetOverlappingBodies();

        foreach(var body in bodies)
        {
            if (body.Equals(PlayerNode)) continue;

            var tmpBody = body as RigidBodyHitTest;
            if (tmpBody.HasMethod("BulletHit"))
                tmpBody.BulletHit(DAMAGE, area.GlobalTransform);
        }
    }

    public bool EquipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState.Equals(IDLE_ANIM_NAME))
        {
            IsWeaponEnabled = true;
            return true;
        }

        if (PlayerNode.AnimationManager.CurrentState.Equals("Idle_unarmed"))
            PlayerNode.AnimationManager.SetAnimation("Knife_equip");

        return false;
    }

    public bool UnequipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState.Equals(IDLE_ANIM_NAME))
            PlayerNode.AnimationManager.SetAnimation("Knife_unequip");

        if (PlayerNode.AnimationManager.CurrentState.Equals("Idle_unarmed"))
        {
            IsWeaponEnabled = false;
            return true;
        }

        return false;
    }
}