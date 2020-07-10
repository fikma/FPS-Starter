using Godot;

public class PickupHealth : AbstractUsableItem
{
	public readonly byte[] HEALTH_AMOUNTS = { 70, 30 };

	public override void _Ready()
	{
		GetNode<Area>("Holder/Health_Pickup_Trigger").Connect("body_entered", this, "TriggerBodyEntered");

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
				GetNode<CollisionShape>("Holder/Health_Pickup_Trigger/Shape_Kit").Disabled = !enable;
				GetNode<Spatial>("Holder/Health_Kit").Visible = enable;
				break;
			case 1:
				GetNode<CollisionShape>("Holder/Health_Pickup_Trigger/Shape_Kit_Small").Disabled = !enable;
				GetNode<Spatial>("Holder/Health_Kit_Small").Visible = enable;
				break;
		}
	}

	protected override void TriggerBodyEntered(Player body)
	{
		if (body.HasMethod("AddHealth"))
		{
			body.AddHealth(HEALTH_AMOUNTS[KitSize]);
			_respawnTimer = RESPAWN_TIME;
			KitSizeChangeValues(KitSize, false);
		}
	}
}
