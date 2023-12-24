using System;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class Druid : Rotation
{

    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private TimeSpan starsurgeCooldown = TimeSpan.FromSeconds(6.5);
    private DateTime laststarsurgeTime = DateTime.MinValue;

    public override void Initialize()
    {
        lastDebugTime = DateTime.Now;
        LogPlayerStats();

        SlowTick = 600;
        FastTick = 200;
    }

    public override bool PassivePulse()
    {
        var me = Api.Player;
        if (!me.IsValid()) return false;
        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving()) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;

        var mana = me.Mana;
        var healthPercentage = me.HealthPercent;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }

        if (Api.Spellbook.CanCast("Mark of the Wild") && 
            (!me.HasAura("Mark of the Wild") || me.AuraRemains("Mark of the Wild") <= 30000))
        {
            Print("Casting Mark of the Wild", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Mark of the Wild"))
                return true;
        }

        if (Api.Spellbook.CanCast("Thorns") && !me.HasAura("Thorns"))
        {
            Print("Casting Thorns", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Thorns"))
                return true;
        }

        if (Api.Spellbook.CanCast("Omen of Clarity") && !me.HasAura("Omen of Clarity"))
        {
            Print("Casting Omen of Clarity", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Omen of Clarity"))
                return true;
        }

        var target = Api.Target;
        if (!target.IsValid() || target.IsDeadOrGhost()) return false;
        if (me.GetReaction(target) >= UnitReaction.Friendly) return false;

        if (Api.Spellbook.CanCast("Moonfire") && !target.HasAura("Moonfire"))
        {
            Print("Moonfire", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Moonfire"))
                return true;
            // If unable to cast Moonfire, proceed to the next spell
        }
        else if (Api.Spellbook.CanCast("Wrath"))
        {
            Print("Wrath", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Wrath"))
                return true;
        }
        else
        {
            // Handle if unable to cast Wrath or Moonfire
            Console.WriteLine("Unable to cast Moonfire or Wrath. Skipping cast.");
        }

        return base.PassivePulse();
    }

    public override bool CombatPulse()
    {
        var me = Api.Player;
        if (!me.IsValid() || me.IsDeadOrGhost()) return false;
        if (me.IsCasting() || me.IsChanneling()) return false;
        
        var mana = me.Mana; 
        var energy = me.Energy;
        var points = me.ComboPoints;
        var healthPercentage = me.HealthPercent;
        
        var target = Api.Target;
        if (!target.IsValid() || target.IsDeadOrGhost()) return false;
        if (me.GetReaction(target) >= UnitReaction.Friendly) return false;

        var targethealth = target.HealthPercent;
        
        if (Api.Spellbook.CanCast("Rejuvenation") && !me.HasAura("Rejuvenation") && healthPercentage <= 70 && mana > 15)
        {
            Print("Casting Rejuvenation", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Rejuvenation"))
                return true;
        }

        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana > 25 && me.HasAura("Fury of Stormrage"))
        {
            Print("Casting Healing Touch", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Healing Touch"))
                return true;
        }

        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana > 25)
        {
            Print("Casting Healing Touch", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Healing Touch"))
                return true;
        }

        if ((!target.HasAura("Sunfire") || target.AuraRemains("Sunfire") <= 2500 )&& targethealth > 30 && Api.HasMacro("Sunfire"))
        {
            Print("Casting Sunfire", ConsoleColor.Green);
            if (Api.UseMacro("Sunfire"))
            {
                return true;
            }
        }

        if (Api.HasMacro("Starsurge"))
        {
            if ((DateTime.Now - laststarsurgeTime) < starsurgeCooldown)
                Print("Starsurge is on cooldown. Skipping cast.", ConsoleColor.DarkYellow); // If the cooldown period for Chimera Shot hasn't elapsed yet
            else
            {
                Print("Casting Starsurge", ConsoleColor.Green);
                if (Api.UseMacro("Starsurge"))
                {
                    laststarsurgeTime = DateTime.Now;
                    return true;
                }
            }
        }

        if (Api.Spellbook.CanCast("Moonfire") && targethealth > 30 && 
            (!target.HasAura("Moonfire") || target.AuraRemains("Moonfire") <= 2500))
        {
            Print("Casting Moonfire", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Moonfire"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Wrath"))
        {
            Print("Casting Wrath", ConsoleColor.Green);
            if (Api.Spellbook.Cast("Wrath"))
                return true;
        }

        return base.CombatPulse();
    }

    private void LogPlayerStats()
    {
        var me = Api.Player;

        var mana = me.Mana;
        var healthPercentage = me.HealthPercent;

        Console.ForegroundColor = ConsoleColor.Red;
        Print(new []
        {
            $"{mana} Mana available" , 
            $"{healthPercentage}% Health available"
        }, ConsoleColor.Red);

        if (me.HasAura("Fury of Stormrage"))
            Print("Casting Fury of Stormrage", ConsoleColor.Green);

    }

    private void Print(string message, ConsoleColor color)
    {
        if(string.IsNullOrEmpty(message)) return;
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = old;
    }

    private void Print(string[] messages, ConsoleColor color)
    {
        if(messages is {Length: <= 0}) return;
        
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        for (var i = 0; i < messages.Length; i++)
            Console.WriteLine(messages[i]);
        Console.ForegroundColor = old;
    }
}