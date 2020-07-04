using Godot;
using Godot.Collections;

public class AnimationPlayerManager : AnimationPlayer
{
    private Dictionary<string, Array<string>> _states = new Dictionary<string, Array<string>>
    {
        {"Idle_unarmed", new Array<string>{ "Knife_equip", "Idle_unarmed" }},

        {"Knife_equip", new Array<string> { "Knife_idle" } },
        {"Knife_fire", new Array<string> { "Knife_idle" } },
        {"Knife_idle", new Array<string> { "Knife_fire", "Knife_unequip", "Knife_idle" } },
        {"Knife_unequip", new Array<string> { "Idle_unarmed" } },
    };

    private Dictionary<string, float> _animationSpeeds = new Dictionary<string, float>
    {
        { "Idle_unarmed", 1 },

        { "Knife_equip", 1},
        { "Knife_fire", 1.35f},
        { "Knife_unequip", 1},
        { "Knife_idle", 1},
    };

    public string CurrentState = "";
    public System.Action CallbackFunction = null;

    public override void _Ready()
    {
        SetAnimation("Idle_unarmed");
        Connect("animation_finished", this, "OnAnimationEnded");
    }

    public bool SetAnimation(string animationName)
    {
        if (animationName.Equals(CurrentState))
        {
            GD.Print("AnimationPlayerManager.cs -- Warning: animasi sudah ", animationName);
            return true;
        }

        if (HasAnimation(animationName))
        {
            if (!CurrentState.Equals(""))
            {
                var possibleAnimations = _states[CurrentState];
                if (possibleAnimations.Contains(animationName))
                {
                    CurrentState = animationName;
                    Play(animationName, -1, _animationSpeeds[animationName]);
                    return true;
                }
                else
                {
                    GD.Print("AnimationPlayerManager.cs -- Warning: Tidak bisa ganti ke ", animationName, " dari ", CurrentState);
                    return false;
                }
            }
            else
            {
                CurrentState = animationName;
                Play(animationName, -1, _animationSpeeds[animationName]);
                return true;
            }
        }

        return false;
    }

    public void OnAnimationEnded(string animationName)
    {
        switch (CurrentState)
        {
            case "Idle_unarmed":
                break;

            case "Knife_idle":
                break;
            case "Knife_equip":
                SetAnimation("Knife_idle"); break;
            case "Knife_fire":
                SetAnimation("Knife_idle"); break;
            case "Knife_unequip":
                SetAnimation("Idle_unarmed"); break;
        }
    }

    public void AnimationCallback()
    {
        if (CallbackFunction.Equals(null))
            GD.Print("AnimationPlayerManager.gd -- WARNING: Tidak callback function untuk  animation tuk dipanggil!");
        else
            CallbackFunction();
    }
}