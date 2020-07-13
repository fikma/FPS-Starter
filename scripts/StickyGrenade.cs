using Godot;
using System;

public class StickyGrenade : AbstractGrenade
{
    protected override byte GRENADE_DAMAGE => 40;

    protected override byte GRENADE_TIME => 3;

    protected override float EXPLOSION_WAIT_TIME => 0.48f;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }
}
