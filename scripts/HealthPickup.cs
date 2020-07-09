using Godot;

public class HealthPickup : Spatial
{
	[Export(PropertyHint.Enum, "full size,small")]
	public int KitSize {get => _kitSize; set => KitSizeChange(value);}

	private int _kitSize = 0;

	protected bool _isReady = false;

	const int RESPAWN_TIME = 20;
	private float _respawnTimer = 0f;

	public readonly int[] HEALTH_AMMOUNTS = { 70, 30 };

	public override void _Ready()
	{
		GetNode<Area>("Holder/Health_Pickup_Trigger").Connect("body_entered", this, "TriggerBodyEntered");

		_isReady = true;

		KitSizeChangeValues(0, false);
		KitSizeChangeValues(1, false);
		KitSizeChangeValues(_kitSize, true);
	}

	public override void _PhysicsProcess(float delta)
	{
		if (_respawnTimer > 0)
		{
			_respawnTimer -= delta;
			if (_respawnTimer <= 0)
				KitSizeChangeValues(_kitSize, true);
		}
	}

	private void KitSizeChange(int value)
	{
		if (_isReady == true)
		{
			_kitSize = value;
		}
		else
			_kitSize = value;
	}

	private void KitSizeChangeValues(int size, bool enable)
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

	private void TriggerBodyEntered(Player body)
	{
		if (body.HasMethod("AddHealth"))
		{
			body.AddHealth(HEALTH_AMMOUNTS[_kitSize]);
			_respawnTimer = RESPAWN_TIME;
			KitSizeChangeValues(_kitSize, false);
		}
	}
}
