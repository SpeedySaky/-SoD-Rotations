using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;


public class PriestShadow : Rotation
{

    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private DateTime lastHands = DateTime.MinValue;
    private TimeSpan Hands = TimeSpan.FromSeconds(14);
    private DateTime lastPants = DateTime.MinValue;
    private TimeSpan Pants = TimeSpan.FromSeconds(65);
    private DateTime lastChest = DateTime.MinValue;
    private TimeSpan Chest = TimeSpan.FromSeconds(8);


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


    public override void Initialize()
    {
        // Can set min/max levels required for this rotation.

        lastDebugTime = DateTime.Now;
        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

        // Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 800;
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
        var mana = me.ManaPercent;
        var target = Api.Target;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources

        // Target distance from the player

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsLooting() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if (Api.Spellbook.CanCast("Renew") && !me.Auras.Contains("Renew") && healthPercentage < 80)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Renew");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Renew"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Power Word: Fortitude") && !me.Auras.Contains("Power Word: Fortitude"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Fortitude");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Fortitude"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Inner Fire") && !me.Auras.Contains("Inner Fire"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Inner Fire");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Inner Fire"))
            {
                return true;
            }
        }

        var reaction = me.GetReaction(target);

        if (!target.IsDead() &&
            (reaction != UnitReaction.Friendly &&
             reaction != UnitReaction.Honored &&
             reaction != UnitReaction.Revered &&
             reaction != UnitReaction.Exalted) &&
            mana > 20 && !IsNPC(target))
            if (reaction != UnitReaction.Friendly)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Mind Blast");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Mind Blast"))

                    return true;
            }


        return base.PassivePulse();
    }


    public override bool CombatPulse()
    {

        var me = Api.Player;
        var target = Api.Target;
        var mana = me.ManaPercent;
        if (me.IsCasting() || me.IsChanneling() || me.IsLooting()) return false;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };

        if (me.HealthPercent <= 70 && (!Api.Inventory.OnCooldown(MP) && !Api.Inventory.OnCooldown(HP)))
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

        if (me.ManaPercent <= 50 && (!Api.Inventory.OnCooldown(MP) && !Api.Inventory.OnCooldown(HP)))
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

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);
        if (Api.Spellbook.CanCast("Power Word: Shield") && !me.Auras.Contains("Power Word: Shield") && mana > 15 && !me.Auras.Contains("Weakened Soul"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Shield");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Shield"))
            {
                return true;
            }
        }
        if (Api.HasMacro("Hands") && targethealth >= 30)
        {
            if ((DateTime.Now - lastHands) >= Hands)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Hands rune");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    lastHands = DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Hands rune is on cooldown. Skipping cast.");
            }
        }
        if (Api.HasMacro("Chest") && !target.Auras.Contains("Void Plague"))
        {
            if ((DateTime.Now - lastChest) >= Chest)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Chest rune");
                Console.ResetColor();

                if (Api.UseMacro("Chest"))
                {
                    lastChest = DateTime.Now;
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Chest rune is on cooldown. Skipping cast.");
            }
        }
        if (Api.HasMacro("Legs") && targethealth >= 30)
        {
            if ((DateTime.Now - lastPants) >= Pants)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Legs rune");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    lastHands = DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Legs rune is on cooldown. Skipping cast.");
            }
        }
        if (Api.Spellbook.CanCast("Shadow Word: Pain") && !target.Auras.Contains("Shadow Word: Pain") && targethealth >= 30 && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shadow Word: Pain");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Shadow Word: Pain"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Mind Blast") && targethealth >= 30 && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Mind Blast");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Mind Blast"))
            {
                return true;
            }
        }


        if (Api.HasMacro("Shoot"))


        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shoot");
            Console.ResetColor();

            if (Api.UseMacro("Shoot"))
            {
                return true;
            }
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



    }

}