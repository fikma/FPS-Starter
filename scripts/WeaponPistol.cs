using Godot;

public class WeaponPistol : AbstractWeapon
{
    public override byte AMMO_IN_MAG => 10;

    public override byte DAMAGE => 15;

    private PackedScene _bulletScene = GD.Load<PackedScene>("Bullet_Scene.tscn");

    public override string IDLE_ANIM_NAME => "Pistol_idle";
    public override string FIRE_ANIM_NAME => "Pistol_fire";
    public override string RELOAD_ANIM_NAME => "Pistol_reload";

    public override bool CAN_RELOAD => true;

    public override bool CAN_REFILL => true;


    public WeaponPistol()
    {
        this.AmmoInWeapon = 10;
        this.SpareAmmo = 20;
    }

    public override bool EquipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IDLE_ANIM_NAME)
        {
            IsWeaponEnabled = true;
            return true; 
        }

        if (PlayerNode.AnimationManager.CurrentState == "Idle_unarmed")
            PlayerNode.AnimationManager.SetAnimation("Pistol_equip");

        return false;
    }

    public override void FireWeapon()
    {
        var clone = _bulletScene.Instance() as BulletScript;
        Spatial sceneRoot = GetTree().Root.GetChildren()[0] as Spatial;
        sceneRoot.AddChild(clone);

        clone.GlobalTransform = this.GlobalTransform;
        clone.Scale = new Vector3(4, 4, 4);
        clone.BULLET_DAMAGE = DAMAGE;

        AmmoInWeapon -= 1;

        PlayerNode.CreateSound("Pistol_shot", this.GlobalTransform.origin);
    }

    public override bool UnequipWeapon()
    {
        if (PlayerNode.AnimationManager.CurrentState == IDLE_ANIM_NAME)
            if (PlayerNode.AnimationManager.CurrentState != "Pistol_unequip")
                PlayerNode.AnimationManager.SetAnimation("Pistol_unequip");

        if (PlayerNode.AnimationManager.CurrentState == "Idle_unarmed")
        {
            IsWeaponEnabled = false;
            return true;
        }
        else
            return false;
    }
}