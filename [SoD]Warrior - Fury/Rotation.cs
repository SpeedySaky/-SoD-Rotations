using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
public class FuryWarriorSoD : Rotation
{

    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;


    public bool IsValid(WowUnit unit)
    {
        if (unit == null || unit.Address == null)
        {
            return false;
        }
        return true;
    }


    public override void Initialize()
    {
        // Can set min/max levels required for this rotation.

        lastDebugTime = DateTime.Now;
        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

        // Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 600;
        FastTick = 200;

        // You can also use this method to add to various action lists.

        // This will add an action to the internal passive tick.
        // bool: needTarget -> If true action will not fire if player does not have a target
        // Func<bool>: function -> Action to attempt, must return true or false.
        PassiveActions.Add((true, () => false));

        // This will add an action to the internal combat tick.
        // bool: needTarget -> If true action will not fire if player does not have a target
        // Func<bool>: function -> Action to attempt, must return true or false.
        CombatActions.Add((true, () => false));



    }
    public override bool PassivePulse()
    {
        var me = Api.Player;
        var healthPercentage = me.HealthPercent;
        var target = Api.Target;
        var rage = me.Rage;

        if (!IsValid(target))
            return true;

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;
        var targetDistance = target.Position.Distance2D(me.Position);


        if (!target.IsDead())
        {
            if (Api.Spellbook.CanCast("Charge") && targetDistance >= 8 && targetDistance <= 23 && !Api.Spellbook.OnCooldown("Charge"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Charge");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Charge"))
                    return true;
            }
        }
        return base.PassivePulse();
    }


    public override bool CombatPulse()
    {
        var me = Api.Player;
        var healthPercentage = me.HealthPercent;
        var rage = me.Rage;
        var target = Api.Target;
        var targethealth = target.HealthPercent;
        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if (me.HealthPercent <= 70 && !Api.Inventory.OnCooldown(HP))
        {
            foreach (string hpot in HP)
            {
                if (HasItem(hpot))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Using Healing potion");
                    Console.ResetColor();
                    if (Api.Inventory.Use(hpot))
                    {
                        return true;
                    }
                }
            }
        }



        if (!me.HasAura("Bloodrage") && Api.Spellbook.CanCast("Bloodrage") && !Api.Spellbook.OnCooldown("Bloodrage") && healthPercentage >= 75)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Bloodrage");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Bloodrage"))

                return true;

        }
        if (!me.HasAura("Battle Shout") && Api.Spellbook.CanCast("Battle Shout") && !Api.Spellbook.OnCooldown("Battle Shout"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Battle Shout");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Battle Shout"))

                return true;

        }
        if (Api.Spellbook.CanCast("Hamstring") && targethealth <= 30 && !target.HasAura("Hamstring"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Hamstring");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hamstring"))

                return true;
        }
        if (Api.Spellbook.CanCast("Rend") && targethealth >= 30 && !target.HasAura("Rend"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Rend");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Rend"))

                return true;
        }
        if (Api.Spellbook.CanCast("Demoralizing Shout") && !target.HasAura("Demoralizing Shout"))  && Api.UnfriendlyUnitsNearby(8, true))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Demoralizing Shout");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Demoralizing Shout"))

                return true;
        }
        if (Api.Spellbook.CanCast("Thunder Clap") && !target.HasAura("Thunder Clap"))  && Api.UnfriendlyUnitsNearby(8, true))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Thunder Clap");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Thunder Clap"))

                return true;
        }



        if (Api.Spellbook.CanCast("Overpower") && rage >= 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Overpower");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Overpower"))

                return true;

        }
        if (Api.Spellbook.CanCast("Heroic Strike") && rage >= 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Heroic Strike");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Heroic Strike"))

                return true;

        }
        return base.CombatPulse();
    }



    private void LogPlayerStats()
    {
        var me = Api.Player;

        var rage = me.Rage;
        var healthPercentage = me.HealthPercent;


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{rage} Rage available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();



    }

}