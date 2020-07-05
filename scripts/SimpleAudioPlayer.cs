using Godot;

public class SimpleAudioPlayer : Spatial
{
	private AudioStream _audioPistolShot = GD.Load<AudioStream>("res://assets/gun_revolver_pistol_shot_04.wav");
	private AudioStream _audioGunCock = GD.Load<AudioStream>("res://assets/gun_semi_auto_rifle_cock_02.wav");
	private AudioStream _audioRifleShot = GD.Load< AudioStream>("res://assets/gun_rifle_sniper_shot_01.wav");

	private AudioStreamPlayer _audioNode = null;

	public override void _Ready()
	{
		_audioNode = GetNode<AudioStreamPlayer>("Audio_Stream_Player");
		_audioNode.Connect("finished", this, "DestroySelf");
		_audioNode.Stop();
	}

	public void PlaySound(string soundName, Vector3 position = new Vector3())
	{
		if (_audioPistolShot == null || _audioGunCock == null || _audioRifleShot == null)
		{
			GD.Print("Audio not set!");
			QueueFree();
			return;
		}

		switch (soundName)
		{
			case "Pistol_shot":
				_audioNode.Stream = _audioPistolShot; break;
			case "Rifle_shot":
				_audioNode.Stream = _audioRifleShot; break;
			case "Gun_cock":
				_audioNode.Stream = _audioGunCock; break;
			default:
				GD.Print("Unknown stream");
				QueueFree();
				return;
		}

		_audioNode.Play();
	}

	private void DestroySelf()
	{
		_audioNode.Stop();
		QueueFree();
	}
}
