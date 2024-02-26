using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;


public class RogueNoStealth : Rotation
{

    private List<string> npcConditions = new List<string>
    {
        "Innkeeper", "Auctioneer", "Banker", "FlightMaster", "GuildBanker",
        "PlayerVehicle", "StableMaster", "Repair", "Trainer", "TrainerClass",
        "TrainerProfession", "Vendor", "VendorAmmo", "VendorFood", "VendorPoison",
        "VendorReagent", "WildBattlePet", "GarrisonMissionNPC", "GarrisonTalentNPC",
        "QuestGiver"
    };
    public bool IsValid(WowUnit unit)
    {
        if (unit == null || unit.Address == null)
        {
            return false;
        }
        return true;
    }
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;

    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    private DateTime lastQuickdraw = DateTime.MinValue;
    private TimeSpan QuickdrawCooldown = TimeSpan.FromSeconds(11);
    private DateTime lastBetween = DateTime.MinValue;
    private TimeSpan BetweenCooldown = TimeSpan.FromSeconds(10);

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
        // Variables for player and target instances
        var me = Api.Player;
        var target = Api.Target;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources
        var energy = me.Energy; // Energy
        var points = me.ComboPoints;

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;


        if (Api.Spellbook.CanCast("Sprint") && !Api.Spellbook.OnCooldown("Sprint"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Sprint");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Sprint"))
            {
                return true;
            }
        }

        return base.PassivePulse();

    }

    public override bool CombatPulse()
    {
        // Variables for player and target instances
        var me = Api.Player;
        var target = Api.Target;
        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;
        var energy = me.Energy; // Energy
        var points = me.ComboPoints;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };

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


        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting()) return false;
        if (me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };

        if (me.HealthPercent <= 70 &&  !Api.Inventory.OnCooldown(HP))
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


        if ((Api.Spellbook.CanCast("Kick") || Api.Spellbook.CanCast("Kidney Shot")) && (!Api.Spellbook.OnCooldown("Kick") || !Api.Spellbook.OnCooldown("Kidney Shot")) && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Kick");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Kick"))
            {
                return true;
            }
        }
        else if (Api.Spellbook.CanCast("Kidney Shot") && energy >= 25 && points >= 1)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Kidney Shot");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Kidney Shot"))
            {
                return true;
            }
        }


        //legs
        bool hasBetween = HasEnchantment(EquipmentSlot.Legs, "Between the Eyes");
        bool hasDance = HasEnchantment(EquipmentSlot.Legs, "Blade Dance");
        bool hasEnvenom = HasEnchantment(EquipmentSlot.Legs, "Envenom");
        //hands
        bool hasShadowstrike = HasEnchantment(EquipmentSlot.Hands, "Shadowstrike");
        bool hasMutilate = HasEnchantment(EquipmentSlot.Hands, "Mutilate");
        bool hasSaber = HasEnchantment(EquipmentSlot.Hands, "Saber Slash");
        bool hasShiv = HasEnchantment(EquipmentSlot.Hands, "Shiv");
        bool hasGauche = HasEnchantment(EquipmentSlot.Hands, "Main Gauche");
        //chest
        bool hasQuick = HasEnchantment(EquipmentSlot.Chest, "Quick Draw");
        bool hasSlaughter = HasEnchantment(EquipmentSlot.Chest, "Slaughter from the Shadows");
        bool hasBrew = HasEnchantment(EquipmentSlot.Chest, "Deadly Brew");
        bool hasWound = HasEnchantment(EquipmentSlot.Chest, "Just a Flesh Wound");
        //waist
        bool hasKnife = HasEnchantment(EquipmentSlot.Waist, "Poisoned Knife");
        bool hasShadowstep = HasEnchantment(EquipmentSlot.Waist, "Shadowstep");
        bool hasShuriken = HasEnchantment(EquipmentSlot.Waist, "Shuriken Toss");
        //feet
        bool hasSubtlety = HasEnchantment(EquipmentSlot.Feet, "Master of Subtlety");
        bool hasPunches = HasEnchantment(EquipmentSlot.Feet, "Rolling with the Punches");
        bool hasWaylay = HasEnchantment(EquipmentSlot.Feet, "Waylay");
        //Arows
        string[] Arrows = { "Thorium Headed Arrow", "Jagged Arrow", "Razor Arrow", "Sharp Arrow", "Rough Arrow" };

        if (Api.HasMacro("Chest"))
        {
            if (hasQuick && (DateTime.Now - lastQuickdraw) >= QuickdrawCooldown && energy >= 20 && targetDistance >= 8 && Api.Inventory.ItemCount(Arrows) >= 1)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Chest Rune");
                Console.ResetColor();

                if (Api.UseMacro("Chest"))
                {
                    lastQuickdraw = DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Quick Draw hasn't elapsed yet
                Console.WriteLine("Chest rune is on cooldown. Skipping cast.");
            }
        }

        if (Api.HasMacro("Legs"))
        {
            if (hasBetween && (DateTime.Now - lastBetween) >= BetweenCooldown && energy >= 35 && points >= 3)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Between your eyes Rune");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    lastQuickdraw = DateTime.Now;
                    return true;
                }
            }
            else if (hasEnvenom && target.Auras.Contains("Deadly Poison") && points >= 2 && energy >= 35)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Envenom Rune");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    return true;
                }
            }
            else if (hasDance && points >= 2 && Api.UnfriendlyUnitsNearby(10, true) >= 2)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Blade Dance Rune");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    return true;
                }
            }
        }


        if (Api.HasMacro("Hands"))
        {
            if (hasGauche && (DateTime.Now - Gauche) >= GaucheCD && energy >= 35)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Gauche Rune");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    Gauche = DateTime.Now;
                    return true;
                }
            }
            else if (hasMutilate && energy >= 40)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Mutilate Rune");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    return true;
                }
            }
            else if (hasSaber && energy >= 45 && !target.Auras.Contains("Saber Slash"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Blade Dance Rune");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    return true;
                }
            }

            else if (hasShiv && energy >= 20 && points < 5)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Blade Dance Rune");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    return true;
                }
            }
        }

        if (Api.HasMacro("Waist"))
        {
            if (hasKnife && energy >= 25 && (DateTime.Now - Knife) >= KnifeCD && points < 3)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Poisoned Knife Rune");
                Console.ResetColor();

                if (Api.UseMacro("Waist"))
                {
                    Knife = DateTime.Now;

                    return true;
                }
            }
            else if (hasShadowstep && targetDistance <= 25 && (DateTime.Now - Shadowstep) >= ShadowstepCD)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Shadowstep Rune");
                Console.ResetColor();

                if (Api.UseMacro("Waist"))
                {
                    Shadowstep = DateTime.Now;
                    return true;
                }
            }
            else if (hasShuriken && energy >= 30 && targetDistance <= 25 && Api.UnfriendlyUnitsNearby(10, true) >= 2)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Shuriken Rune");
                Console.ResetColor();

                if (Api.UseMacro("Waist"))
                {
                    return true;
                }
            }

        }
        if (Api.Spellbook.CanCast("Evasion") && !me.Auras.Contains("Evasion", false) && Api.UnfriendlyUnitsNearby(5, true) >= 2 && !Api.Spellbook.OnCooldown("Evasion"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Evasion");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Evasion"))
                return true;
        }
        if (Api.Spellbook.HasSpell("Blade Flurry") && Api.UnfriendlyUnitsNearby(5, true) >= 2 && !Api.Spellbook.OnCooldown("Blade Flurry") && energy >= 25)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Blade Flurry ");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Blade Flurry"))
                return true;
        }
        if (Api.Spellbook.HasSpell("Rupture") && points >= 2 && !target.Auras.Contains("Rupture") && energy >= 25)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Rupture ");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Rupture"))
                return true;
        }
        if (Api.Spellbook.HasSpell("Slice and Dice") && points >= 2 && !me.Auras.Contains("Slice and Dice") && energy >= 25)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Slice and Dice ");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Slice and Dice"))
                return true;
        }
        if (Api.Spellbook.CanCast("Eviscerate") && points >= 3 && energy >= 35)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Eviscerate ");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Eviscerate"))
                return true;
        }
        if (Api.Spellbook.CanCast("Sinister Strike") && energy >= 45)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Sinister Strike ");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Sinister Strike"))
                return true;
        }





        return base.CombatPulse();
    }

    private bool IsNPC(WowUnit unit)
    {
        if (!IsValid(unit))
        {
            // If the unit is not valid, consider it not an NPC
            return false;
        }

        foreach (var condition in npcConditions)
        {
            switch (condition)
            {
                case "Innkeeper" when unit.IsInnkeeper():
                case "Auctioneer" when unit.IsAuctioneer():
                case "Banker" when unit.IsBanker():
                case "FlightMaster" when unit.IsFlightMaster():
                case "GuildBanker" when unit.IsGuildBanker():
                case "StableMaster" when unit.IsStableMaster():
                case "Trainer" when unit.IsTrainer():
                case "Vendor" when unit.IsVendor():
                case "QuestGiver" when unit.IsQuestGiver():
                    return true;
            }
        }

        return false;
    }
    private void LogPlayerStats()
    {
        // Variables for player and target instances
        var me = Api.Player;
        var target = Api.Target;

        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        var energy = me.Energy; // Energy
        var points = me.ComboPoints;

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{energy}% Energy available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.WriteLine($"{points} points available");

        Console.ResetColor();


    }

}
