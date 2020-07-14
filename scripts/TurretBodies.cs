using Godot;
using System;

public class TurretBodies : StaticBody
{
	[Export]
	public NodePath PathToTurretRoot;
	
	public void BulletHit(byte damage, Vector3 bulletHitPos)
	{
		if (PathToTurretRoot != null)
			GetNode<Turret>(PathToTurretRoot).BulletHit(damage, bulletHitPos);
	}
}
