using Godot;

public class WeaponRifle : Spatial
{
    private const int DAMAGE = 4;

    private const string IDLE_ANIM_NAME = "Rifle_idle";
    public string IdleAnimName { get => IDLE_ANIM_NAME; }
    private const string FIRE_ANIM_NAME = "Rifle_fire";
    public string FireAnimName { get => FIRE_ANIM_NAME; }

    public bool IsWeaponEnabled = false;

    public Player PlayerNode = null;

    public void FireWeapon()
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
    }

    public bool EquipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IdleAnimName)
        {
            IsWeaponEnabled = true;
            return true;
        }

        if (PlayerNode.AnimationManager.CurrentState == "Idle_unarmed")
            PlayerNode.AnimationManager.SetAnimation("Rifle_equip");

        return false;
    }

    public bool UnequipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IdleAnimName)
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