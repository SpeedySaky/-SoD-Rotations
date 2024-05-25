using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;



public class FeralDruidSoD : Rotation
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
    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;


    public override void Initialize()
    {
        // Can set min/max levels required for this rotation.

        lastDebugTime = DateTime.Now;
        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

        // Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 1550;
        FastTick = 500;

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
        var mana = me.ManaPercent;
        var target = Api.Target;

        var reaction = me.GetReaction(target);
        var meTarget = me.Target;
        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.Auras.Contains("Drink") || me.Auras.Contains("Food") || me.IsMounted()) return false;
        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }

        if (me.IsValid())
        {
            if (Api.Spellbook.CanCast("Mark of the Wild") && !me.Auras.Contains("Mark of the Wild"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Mark of the Wild");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Mark of the Wild"))

                    return true;
            }

            if (Api.Spellbook.CanCast("Thorns") && !me.Auras.Contains("Thorns"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Thorns");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Thorns"))

                    return true;
            }

            if (Api.Spellbook.CanCast("Omen of Clarity") && !me.Auras.Contains("Omen of Clarity"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Omen of Clarity");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Omen of Clarity"))

                    return true;
            }
            if (Api.Spellbook.CanCast("Rejuvenation") && healthPercentage <= 60 && !me.Auras.Contains("Rejuvenation"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Rejuvenation");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Rejuvenation"))
                    return true;
            }

            if (Api.Spellbook.CanCast("Regrowth") && healthPercentage <= 40 && !me.Auras.Contains("Regrowth"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Regrowth");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Regrowth"))
                    return true;
            }
            if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 30)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Healing Touch");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Healing Touch"))
                    return true;
            }
        }





        return base.PassivePulse();
    }

    public override bool CombatPulse()
    {


        var me = Api.Player;
        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;
        var target = Api.Target;
        var targethealth = target.HealthPercent;
        var energy = me.Energy;
        var points = me.ComboPoints;
        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };
        if (!me.IsValid() || !target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if (me.HealthPercent <= 70 && (!Api.Inventory.OnCooldown(MP) || !Api.Inventory.OnCooldown(HP)))
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

        if (me.ManaPercent <= 50 && (!Api.Inventory.OnCooldown(MP) || !Api.Inventory.OnCooldown(HP)))
        {
            foreach (string manapot in MP)
            {
                if (HasItem(manapot))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Using mana potion");
                    Console.ResetColor();
                    if (Api.Inventory.Use(manapot))
                    {
                        return true;
                    }
                }
            }
        }

        if (Api.Spellbook.CanCast("War Stomp") && !Api.Spellbook.OnCooldown("War Stomp") && healthPercentage <= 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting War Stomp");
            Console.ResetColor();
            if (Api.Spellbook.Cast("War Stomp"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Rejuvenation") && !me.Auras.Contains("Rejuvenation") && healthPercentage <= 30 && mana > 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Rejuvenation");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Rejuvenation"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 30 && mana > 25 && me.Auras.Contains("Fury of Stormrage"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Touch");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Healing Touch"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 30 && mana > 25)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Touch");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Healing Touch"))
            {
                return true;
            }
        }
      
        if (!me.Auras.Contains(768, false) && Api.Spellbook.CanCast(768))
        {
            if (Api.Spellbook.CanCast(768) && !me.Auras.Contains(768, false))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Cat Form");
                Console.ResetColor();
                if (Api.Spellbook.Cast(768))
                {
                    return true;
                }
            }

        }

        if (Api.Spellbook.CanCast("Tiger's Fury") && !me.Auras.Contains("Tiger's Fury") && !Api.Spellbook.OnCooldown("Tiger's Fury"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Tiger's Fury");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Tiger's Fury"))
            {
                return true;
            }
        }
        if (Api.HasMacro("Legs") && points >= 1 && energy >= 25 && !me.Auras.Contains(407988) && me.Auras.Contains("Cat Form", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Savage Roar");
            Console.ResetColor();

            if (Api.UseMacro("Legs"))
                return true;
        }
        if (Api.HasMacro("Mangle") && points < 3 && energy >= 45 && me.Auras.Contains("Cat Form", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Mangle (Cat) with {energy} Energy");
            Console.ResetColor();

            if (Api.UseMacro("Mangle"))
                return true;
        }

        if (Api.Spellbook.CanCast("Claw") && points < 3 && energy >= 45 && me.Auras.Contains("Cat Form", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Claw (Cat) with {energy} Energy");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Claw"))
                return true;
        }

        if (Api.Spellbook.CanCast("Rip") && !target.Auras.Contains("Rip") && target.HealthPercent >= 20 && energy > 30 && points >= 3 && me.Auras.Contains("Cat Form", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Casting Rip with {points} Points and {energy} Energy");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Rip"))
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
        var me = Api.Player;

        var mana = me.ManaPercent;
        var healthPercentage = me.HealthPercent;


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();
        Console.ResetColor();

   

    }
}