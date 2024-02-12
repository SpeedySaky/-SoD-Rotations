using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class Shaman : Rotation
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
    private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        return Api.Equipment.HasEnchantment(slot, enchantmentName);
    }
    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private DateTime lastRockbiterTime = DateTime.MinValue; public override void Initialize()
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
        var mana = me.ManaPercent;


        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        bool hasFlametongueEnchantment = HasEnchantment(EquipmentSlot.MainHand, "Flametongue 1");
        bool hasFlametongueEnchantment2 = HasEnchantment(EquipmentSlot.MainHand, "Flametongue 2");
        bool hasRockbiterEnchantment1 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 1");
        bool hasRockbiterEnchantment2 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 2");
        bool hasRockbiterEnchantment3 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 3");

        bool hasAnyRockbiterEnchantment = hasRockbiterEnchantment1 || hasRockbiterEnchantment2 || hasRockbiterEnchantment3;

        if (!hasFlametongueEnchantment && !hasFlametongueEnchantment2 && Api.Spellbook.CanCast("Flametongue Weapon"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Flametongue Weapon");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Flametongue Weapon"))
            {
                return true;
            }
        }
        else if (!hasFlametongueEnchantment && !hasFlametongueEnchantment2 && !hasAnyRockbiterEnchantment && Api.Spellbook.CanCast("Rockbiter Weapon"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Rockbiter Weapon");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Rockbiter Weapon"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Healing Wave") && healthPercentage <= 50 && mana > 45)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Wave");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Healing Wave"))
                return true;
        }
        if (Api.Spellbook.CanCast("Lightning Shield") && !me.Auras.Contains("Lightning Shield") && mana > 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Lighting Shield");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Lightning Shield"))
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
        var mana = me.ManaPercent;
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

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;


        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting()) return false;
        if (me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;



        if (Api.Player.InCombat() && Api.Target != null && Api.Target.IsValid())
        {

            // Single Target Abilities
            if (!target.IsDead())
            {
                if (Api.Spellbook.CanCast("Healing Wave") && healthPercentage <= 30 && mana >= 45)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Healing Wave");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Healing Wave"))
                        return true;
                }
                if (Api.Spellbook.CanCast("Lightning Shield") && !me.Auras.Contains("Lightning Shield") && mana > 30)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Lighting Shield");
                    Console.ResetColor();
                    if (Api.Spellbook.Cast("Lightning Shield"))
                    {
                        return true;
                    }
                }
                if (Api.Spellbook.CanCast("Flame Shock") && !Api.Spellbook.OnCooldown("Flame Shock") && !target.Auras.Contains("Flame Shock"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Flame Shock");
                    Console.ResetColor();
                    if (Api.Spellbook.Cast("Flame Shock"))
                    {
                        return true;
                    }
                }
                if (Api.Spellbook.CanCast("Earth Shock") && !Api.Spellbook.OnCooldown("Earth Shock") && (target.IsCasting() || target.IsChanneling()))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Earth Shock");
                    Console.ResetColor();
                    if (Api.Spellbook.Cast("Earth Shock"))

                        return true;

                }
                if (Api.Spellbook.CanCast("Lightning Bolt") && mana > 20)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Lightning Bolt");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Lightning Bolt"))
                        return true;
                }


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
        // Variables for player and target instances
        var me = Api.Player;
        var target = Api.Target;
        var mana = me.ManaPercent;

        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources
        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();






    }
}