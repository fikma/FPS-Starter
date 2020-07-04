using Godot;
using Godot.Collections;

public class AnimationPlayerManager : AnimationPlayer
{
    private Dictionary<string, Array<string>> _states = new Dictionary<string, Array<string>>
    {
        {"Idle_unarmed", new Array<string>{ "Rifle_equip", "Knife_equip", "Idle_unarmed" }},

        {"Rifle_equip", new Array<string> { "Rifle_idle" } },
        {"Rifle_fire", new Array<string> { "Rifle_idle" } },
        {"Rifle_idle", new Array<string> { "Rifle_fire", "Rifle_reload", "Rifle_unequip", "Rifle_idle" } },
        {"Rifle_unequip", new Array<string> { "Idle_unarmed" } },

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

        { "Rifle_equip", 2},
        { "Rifle_fire", 6f},
        { "Rifle_unequip", 2},
        { "Rifle_idle", 1},
        { "Rifle_reload", 1.45f},
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

            case "Rifle_idle":
                break;
            case "Rifle_equip":
                SetAnimation("Rifle_idle"); break;
            case "Rifle_fire":
                SetAnimation("Rifle_idle"); break;
            case "Rifle_reload":
                SetAnimation("Rifle_idle"); break;
            case "Rifle_unequip":
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