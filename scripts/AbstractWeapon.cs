using Godot;

/// <summary>
/// Abstract class untuk weapon
/// </summary>
public abstract class AbstractWeapon : Spatial
{
	public byte AmmoInWeapon;
	public ushort SpareAmmo;

	public abstract bool CAN_RELOAD { get; }

	public abstract bool CAN_REFILL { get; }

	public abstract byte AMMO_IN_MAG { get; }

	public abstract byte DAMAGE { get; }

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
			var ammoNeeded = (byte)(AMMO_IN_MAG - AmmoInWeapon);

			if (SpareAmmo >= ammoNeeded)
			{
				SpareAmmo -= ammoNeeded;
				AmmoInWeapon = AMMO_IN_MAG;
			}
			else
			{
				AmmoInWeapon = (byte)(AmmoInWeapon + SpareAmmo);
				SpareAmmo = 0;
			}

			PlayerNode.AnimationManager.SetAnimation(RELOAD_ANIM_NAME);

			PlayerNode.CreateSound("Gun_cock", PlayerNode.Camera.GlobalTransform.origin);

			return true;
		}

		return false;
	}
}
