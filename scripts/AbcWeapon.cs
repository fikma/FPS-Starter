using Godot;

/// <summary>
/// Abstract class untuk weapon
/// </summary>
public abstract class AbcWeapon : Spatial
{
    public int AmmoInWeapon;
    public int SpareAmmo;

    public abstract bool CAN_RELOAD { get; }

    public abstract bool CAN_REFILL { get; }

    public abstract int AMMO_IN_MAG { get; }

    public abstract int DAMAGE { get; }

    public abstract string IDLE_ANIM_NAME { get; }

    public abstract string FIRE_ANIM_NAME { get; }

    public abstract string RELOAD_ANIM_NAME { get; }

    public bool IsWeaponEnabled = false;

    public Player PlayerNode = null;

    public abstract void FireWeapon();

    public abstract bool EquipWeapon();

    public abstract bool UnequipWeapon();

    public virtual bool ReloadWeapon()
    {
        var canReload = false;
        if (PlayerNode.AnimationManager.CurrentState == IDLE_ANIM_NAME)
            canReload = true;

        if (SpareAmmo <= 0 || AmmoInWeapon == AMMO_IN_MAG)
            canReload = false;

        if (canReload)
        {
            var ammoNeeded = AMMO_IN_MAG - AmmoInWeapon;

            if (SpareAmmo >= ammoNeeded)
            {
                SpareAmmo -= ammoNeeded;
                AmmoInWeapon = AMMO_IN_MAG;
            }
            else
            {
                AmmoInWeapon += SpareAmmo;
                SpareAmmo = 0;
            }

            PlayerNode.AnimationManager.SetAnimation(RELOAD_ANIM_NAME);

            return true;
        }

        return false;
    }
}