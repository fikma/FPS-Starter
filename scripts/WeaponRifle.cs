using Godot;

public class WeaponRifle : AbcWeapon
{
    public override int DAMAGE => 4;

    public override string IDLE_ANIM_NAME => "Rifle_idle";
    public override string FIRE_ANIM_NAME => "Rifle_fire";
    public override string RELOAD_ANIM_NAME => "Rifle_reload";

    public override int AMMO_IN_MAG => 50;

    public override bool CAN_RELOAD => true;

    public override bool CAN_REFILL => true;


    public WeaponRifle()
    {
        AmmoInWeapon = 50;
        SpareAmmo = 100;
    }
    public override void FireWeapon()
    {
        var ray = GetNode<RayCast>("Ray_Cast");
        ray.ForceRaycastUpdate();

        if (ray.IsColliding())
        {
            var body = ray.GetCollider();
            if (body == PlayerNode) { }
            else if (body.HasMethod("BulletHit"))
            {
                var aBody = body as RigidBodyHitTest;
                aBody.BulletHit(DAMAGE, ray.GlobalTransform);
            }
        }

        AmmoInWeapon -= 1;
    }

    public override bool EquipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IDLE_ANIM_NAME)
        {
            IsWeaponEnabled = true;
            return true;
        }

        if (PlayerNode.AnimationManager.CurrentState == "Idle_unarmed")
            PlayerNode.AnimationManager.SetAnimation("Rifle_equip");

        return false;
    }

    public override bool UnequipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IDLE_ANIM_NAME)
            if (PlayerNode.AnimationManager.CurrentState != "Rifle_unequip")
                PlayerNode.AnimationManager.SetAnimation("Rifle_unequip");

        if (PlayerNode.AnimationManager.CurrentState == "Idle_unarmed")
        {
            IsWeaponEnabled = false;
            return true;
        }

        return false;
    }
}