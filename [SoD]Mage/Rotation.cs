using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;



public class MageSoD : Rotation
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
        SlowTick = 800;
        FastTick = 400;

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
        var pet = me.Pet();

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;
        var targetDistance = target.Position.Distance2D(me.Position);
        ShadowApi shadowApi = new ShadowApi();

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.HasAura("Drink") || me.HasAura("Food")) return false;

        if (Api.Spellbook.CanCast("Frost Armor") && !me.HasAura("Frost Armor"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Frost Armor");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Frost Armor"))
                return true;
        }

        if (Api.Spellbook.CanCast("Arcane Intellect") && !me.HasPermanent("Arcane Intellect"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Arcane Intellect");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Arcane Intellect"))
                return true;
        }


        string[] waterTypes = { "Conjured Fresh Water", "Conjured Water", "Conjured Purified Water" };
        bool needsWater = true;

        foreach (string waterType in waterTypes)
        {
            if (shadowApi.Inventory.HasItem(waterType))
            {
                needsWater = false;
                break;
            }
        }

        // Now needsWater variable will indicate if the character needs water
        if (needsWater)
        {
            // Add logic here to conjure water or perform any action needed to acquire water
            // Example: Cast "Conjure Water" spell
            // Assuming the API allows for conjuring water in a similar way to casting spells
            if (Api.Spellbook.CanCast("Conjure Water"))
            {
                if (Api.Spellbook.Cast("Conjure Water"))
                {
                    Console.WriteLine("Conjured water.");
                    // Add further actions if needed after conjuring water
                }
            }
        }
        string[] foodTypes = { "Conjured Muffin", "Conjured Bread", "Conjured Rye" };
        bool needsFood = true;

        foreach (string foodType in foodTypes)
        {
            if (shadowApi.Inventory.HasItem(foodType))
            {
                needsFood = false;
                break;
            }
        }

        // Now needsWater variable will indicate if the character needs food
        if (needsFood)
        {
            if (Api.Spellbook.CanCast("Conjure Food"))
            {
                if (Api.Spellbook.Cast("Conjure Food"))
                {
                    Console.WriteLine("Conjured Food.");
                    // Add further actions if needed after conjuring water
                }

            }
        }




        var reaction = me.GetReaction(target);

        if (!target.IsDead() &&
            (reaction != UnitReaction.Friendly &&
             reaction != UnitReaction.Honored &&
             reaction != UnitReaction.Revered &&
             reaction != UnitReaction.Exalted) &&
            mana > 20 && !IsNPC(target))
        {
            Console.WriteLine("Trying to cast Pyroblast");

            // Try casting Pyroblast
            if (Api.Spellbook.CanCast("Pyroblast") && Api.Spellbook.Cast("Pyroblast"))
            {
                Console.WriteLine("Casting Pyroblast");
                return true;
            }
            else
            {
                // If Pyroblast fails, try casting Frostbolt
                if (Api.Spellbook.CanCast("Frostbolt"))
                {
                    Console.WriteLine("Casting Frostbolt");
                    Api.Spellbook.Cast("Frostbolt");
                    return true;
                }
            }
        }

        // If none of the conditions are met or casting both spells fail
        return false;

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

        // Power percentages for different resources
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

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsChanneling()) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;


        if (Api.Spellbook.CanCast("Evocation") && !Api.Spellbook.OnCooldown("Evocation") && mana <= 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Evocation");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Evocation"))
                return true;
        }
        if (Api.Player.InCombat() && Api.Target != null && Api.Target.IsValid())
        {

            // Single Target Abilities
            if (!target.IsDead())
            {
                if (Api.Spellbook.CanCast("Frost Nova") && targetDistance <= 8 && !Api.Spellbook.OnCooldown("Frost Nova"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Frost Nova");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Frost Nova"))
                        return true;
                }
                if (Api.Spellbook.CanCast("Pyroblast") && !Api.Spellbook.OnCooldown("Pyroblast") && !target.HasAura("Pyroblast") && targethealth < 80)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Pyroblast");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Pyroblast"))
                        return true;
                }
                if (Api.Spellbook.CanCast("Fire Blast") && mana > 30 && !target.IsDead() && !Api.Spellbook.OnCooldown("Fire Blast") && targetDistance <= 25)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Fireblast");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Fire Blast"))
                        return true;
                }



                if (Api.Spellbook.CanCast("Fireball") && mana > 30 && targethealth > 20)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Fireball");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Fireball"))
                        return true;
                }

                if (Api.Spellbook.CanCast("Frostbolt") && targethealth < 20)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Frostbolt");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Frostbolt"))
                        return true;
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
            }
        }

        // Check if in combat and if there are multiple targets nearby
        if (me.InCombat() && Api.UnfriendlyUnitsNearby(10, true) >= 2)
        {

            // Multi-Target Abilities

            if (!target.IsDead())
            {
                // Logic for multi-target abilities, e.g. AoE spells, debuffs, etc.
                // Example: if (me.CanCast("AoE_Spell") && target.Distance < 8)
                // {
                //     me.Cast("AoE_Spell");
                // }
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
        var mana = me.Mana;
        ShadowApi shadowApi = new ShadowApi();

        // Health percentage of the player
        var healthPercentage = me.HealthPercent;


        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();


        if (me.HasAura("Frost Armor")) // Replace "Thorns" with the actual aura name
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.ResetColor();
            var remainingTimeSeconds = me.AuraRemains("Frost Armor");
            var remainingTimeMinutes = remainingTimeSeconds / 60; // Convert seconds to minutes
            var roundedMinutes = Math.Round(remainingTimeMinutes / 1000, 1); // Round to one decimal place

            Console.WriteLine($"Remaining time for Frost Armor: {roundedMinutes} minutes");
            Console.ResetColor();
        }



        // Define food and water types
        string[] foodTypes = { "Conjured Muffin", "Conjured Bread", "Conjured Rye" };
        string[] waterTypes = { "Conjured Fresh Water", "Conjured Water", "Conjured Purified Water" };

        // Count food items in the inventory
        int foodCount = 0;
        foreach (string foodType in foodTypes)
        {
            int count = shadowApi.Inventory.ItemCount(foodType);
            foodCount += count;
        }

        // Count water items in the inventory
        int waterCount = 0;
        foreach (string waterType in waterTypes)
        {
            int count = shadowApi.Inventory.ItemCount(waterType);
            waterCount += count;
        }

        // Display the counts of food and water items
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Current Food Count: " + foodCount);
        Console.WriteLine("Current Water Count: " + waterCount);
        Console.ResetColor();



        Console.ResetColor();
    }
}