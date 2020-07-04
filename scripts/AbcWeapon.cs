using Godot;

public abstract class AbcWeapon : Spatial
{
    public abstract int DAMAGE { get; }

    public abstract string IDLE_ANIM_NAME { get; }

    public abstract string FIRE_ANIM_NAME { get; }

    public bool IsWeaponEnabled = false;

    public Player PlayerNode = null;

    public abstract void FireWeapon();

    public abstract bool EquipWeapon();

    public abstract bool UnequipWeapon();
}