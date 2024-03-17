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
    private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        return Api.Equipment.HasEnchantment(slot, enchantmentName);
    }
    //runes 

    // Gloves


    // Chest

    // Pants
    private DateTime lastLivingFlameRune = DateTime.MinValue;
    private TimeSpan LivingFlameRuneCooldown = TimeSpan.FromSeconds(65);

    private DateTime lastArcaneSurgeRune = DateTime.MinValue;
    private TimeSpan ArcaneSurgeRuneCooldown = TimeSpan.FromSeconds(130);

    private DateTime lastIcyVeinsRune = DateTime.MinValue;
    private TimeSpan IcyVeinsRuneCooldown = TimeSpan.FromSeconds(190);


    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private TimeSpan PyroCD = TimeSpan.FromSeconds(10);
    private DateTime lastPyro = DateTime.MinValue;

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

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;
        var hasaura = me.Auras.Contains("Curse of Stalvan") || me.Auras.Contains("Curse of Blood");

        if (hasaura && Api.Spellbook.CanCast("Remove Lesser Curse"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Decursing");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Remove Lesser Curse"))
            {
                return true;
            }
        }
        //string[] GemTypes = { "Mana Agate", "Mana Sapphire", "Mana Emerald", "Mana Ruby", "Mana Citrine", "Mana Jade" };
        //  bool needsgem = true;

        //  foreach (string gemType in GemTypes)
        //  {
        //if (shadowApi.Inventory.HasItem(gemType))
        //  {
        // needsgem = false;
        //   break;
        // }
        // }
        //   if (Api.Spellbook.CanCast("Conjure Mana Agate") && needsgem)
        //  {
        // if (Api.Spellbook.Cast("Conjure Mana Agate"))
        //   {
        // Console.WriteLine("Conjure Mana Gem.");
        // Add further actions if needed after conjuring water
        //}
        //}
        if (Api.Spellbook.CanCast("Ice Armor") && !me.Auras.Contains("Ice Armor"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Ice Armor");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Ice Armor"))
                return true;
        }
        else if (Api.Spellbook.CanCast("Frost Armor") && !me.Auras.Contains("Frost Armor") && !me.Auras.Contains("Ice Armor"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Frost Armor");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Frost Armor"))
                return true;
        }

        if (Api.Spellbook.CanCast("Arcane Intellect") && !me.Auras.Contains("Arcane Intellect", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Arcane Intellect");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Arcane Intellect"))
                return true;
        }


        string[] waterTypes = { "Conjured Mana Strudel", "Conjured Mountain Spring Water", "Conjured Crystal Water", "Conjured Sparkling Water", "Conjured Mineral Water", "Conjured Spring Water", "Conjured Purified Water", "Conjured Fresh Water", "Conjured Water" };
        string[] foodTypes = { "Conjured Mana Strudel", "Conjured Cinnamon Roll", "Conjured Sweet Roll", "Conjured Sourdough", "Conjured Pumpernickel", "Conjured Rye", "Conjured Bread", "Conjured Muffin" };
        bool needsWater = true;

        foreach (string waterType in waterTypes)
        {
            if (HasItem(waterType))
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
            if (Api.Spellbook.CanCast("Pyroblast"))
            {
                Api.Spellbook.Cast("Pyroblast")
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
        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };
        // Belt
        bool hasFrostBoltRune = HasEnchantment(EquipmentSlot.Waist, "Spellfrost Bolt");
        bool hasFrostfireBoltRune = HasEnchantment(EquipmentSlot.Waist, "Frostfire Bolt");
        bool hasHotStreakRune = HasEnchantment(EquipmentSlot.Waist, "Hot Streak");
        bool hasMissileBarrageRune = HasEnchantment(EquipmentSlot.Waist, "Missile Barrag");

        // Boots
        bool hasSpellPowerRune = HasEnchantment(EquipmentSlot.Feet, "Spell Power");
        bool hasChronostaticPreservationRune = HasEnchantment(EquipmentSlot.Feet, "Chronostatic Preservation");
        bool hasBrainFreezeRune = HasEnchantment(EquipmentSlot.Feet, "Brain Freeze");
        // Gloves
        bool hasIceLanceRune = HasEnchantment(EquipmentSlot.Hands, "Ice Lance");
        bool hasLivingBombRune = HasEnchantment(EquipmentSlot.Hands, "Living Bomb");
        bool hasArcaneBlastRune = HasEnchantment(EquipmentSlot.Hands, "Arcane Blast");
        bool hasRewindTimeRune = HasEnchantment(EquipmentSlot.Hands, "Rewind Time");

        // Chest
        bool hasEnlightenmentRune = HasEnchantment(EquipmentSlot.Chest, "Enlightenment");
        bool hasBurnoutRune = HasEnchantment(EquipmentSlot.Chest, "Burnout");
        bool hasFingersOfFrostRune = HasEnchantment(EquipmentSlot.Chest, "Fingers of Frost");
        bool hasRegenerationRune = HasEnchantment(EquipmentSlot.Chest, "Regeneration");

        // Pants
        bool hasLivingFlameRune = HasEnchantment(EquipmentSlot.Legs, "Living Flame");
        bool hasArcaneSurgeRune = HasEnchantment(EquipmentSlot.Legs, "Arcane Surge");
        bool hasIcyVeinsRune = HasEnchantment(EquipmentSlot.Legs, "Icy Vein");
        bool hasMassRegenerationRune = HasEnchantment(EquipmentSlot.Legs, "Mass Regeneration");
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
        string[] GemTypes = { "Mana Jade", "Mana Citrine", "Mana Ruby", "Mana Emerald", "Mana Sapphire", "Mana Agate" };

        if (me.Mana <= 30 && !Api.Inventory.OnCooldown(GemTypes))
        {
            foreach (string gem in GemTypes)
            {
                if (HasItem(gem))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Using {gem}");
                    Console.ResetColor();

                    if (Api.Inventory.Use(gem))
                    {
                        return true;
                    }
                }
            }
        }

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsChanneling()) return false;
        if (me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;
        var hasaura = me.Auras.Contains("Curse of Stalvan") || me.Auras.Contains("Curse of Blood");

        if (hasaura && Api.Spellbook.CanCast("Remove Lesser Curse"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Decursing");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Remove Lesser Curse"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Counterspell") && !Api.Spellbook.OnCooldown("Counterspell") && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Counterspell");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Counterspell"))
            {
                return true;
            }
        }
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
                if (hasIceLanceRune && target.Auras.Contains("Frozen") && Api.HasMacro("Hands") && mana > 8)
                {

                    Console.WriteLine("Casting Ice Lance rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                        return true;
                }

                if (hasLivingBombRune && Api.HasMacro("Hands") && mana > 22 && target.Auras.Contains("Living Bomb"))
                {

                    Console.WriteLine("Casting Living Bomb rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                        return true;
                }

                if (hasArcaneBlastRune && Api.HasMacro("Hands") && mana > 7)
                {

                    Console.WriteLine("Casting Living Bomb rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                        return true;
                }

                if (hasFrostBoltRune && Api.HasMacro("Belt") && mana > 12)
                {

                    Console.WriteLine("Casting Spellfrost Bolt rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                        return true;
                }

                if (hasFrostfireBoltRune && Api.HasMacro("Belt") && mana > 14)
                {

                    Console.WriteLine("Casting Frostfire Bolt rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                        return true;
                }

                if (hasHotStreakRune && me.Aura.Contains("Hot Streak"))
                {

                    Console.WriteLine("Casting Pyroblast with Hot Streak");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Pyroblast"))
                        return true;
                }

                if (hasBrainFreezeRune && me.Aura.Contains("Brain Freeze"))
                {

                    Console.WriteLine("Casting Fireball with Brain Freeze");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Fireball"))
                        return true;
                }

                if (hasIcyVeinsRune && Api.HasMacro("Legs") && mana > 3 && (DateTime.Now - lastIcyVeinsRune) >= IcyVeinsRuneCooldown)
                {

                    Console.WriteLine("Casting Icy Veins rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Legs"))
                        lastIcyVeinsRune = DateTime.Now;

                    return true;
                }

                if (hasLivingFlameRune && Api.HasMacro("Legs") && mana > 11 && (DateTime.Now - lastLivingFlameRune) >= LivingFlameRuneCooldown)
                {

                    Console.WriteLine("Casting Living Flame rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Legs"))
                        lastLivingFlameRune = DateTime.Now;

                    return true;
                }

                if (hasArcaneSurgeRune && Api.HasMacro("Legs") && (DateTime.Now - lastArcaneSurgeRune) >= ArcaneSurgeRuneCooldown)
                {

                    Console.WriteLine("Casting Arcane Surge rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Legs"))
                        lastArcaneSurgeRune = DateTime.Now;

                    return true;
                }


                if (Api.Spellbook.CanCast("Frost Nova") && targetDistance <= 8 && !Api.Spellbook.OnCooldown("Frost Nova"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Frost Nova");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Frost Nova"))
                        return true;
                }

                if (Api.Spellbook.CanCast("Pyroblast") && !target.Auras.Contains("Pyroblast") && targethealth > 20)
                {
                    lastPyro = DateTime.Now;
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

        // Health percentage of the player
        var healthPercentage = me.HealthPercent;


        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();


        if (me.Auras.Contains("Frost Armor")) // Replace "Thorns" with the actual aura name
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.ResetColor();
            var remainingTimeSeconds = me.Auras.TimeRemaining("Frost Armor");
            var remainingTimeMinutes = remainingTimeSeconds / 60; // Convert seconds to minutes
            var roundedMinutes = Math.Round(remainingTimeMinutes / 1000, 1); // Round to one decimal place

            Console.WriteLine($"Remaining time for Frost Armor: {roundedMinutes} minutes");
            Console.ResetColor();
        }



        // Define food and water types
        string[] waterTypes = { "Conjured Mana Strudel", "Conjured Mountain Spring Water", "Conjured Crystal Water", "Conjured Sparkling Water", "Conjured Mineral Water", "Conjured Spring Water", "Conjured Purified Water", "Conjured Fresh Water", "Conjured Water" };
        string[] foodTypes = { "Conjured Mana Strudel", "Conjured Cinnamon Roll", "Conjured Sweet Roll", "Conjured Sourdough", "Conjured Pumpernickel", "Conjured Rye", "Conjured Bread", "Conjured Muffin" };
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
        var hasaura = me.Auras.Contains("Curse of Stalvan");

        if (hasaura)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Have curse debuff");
            Console.ResetColor();
        }
        // Belt
        bool hasFrostBoltRune = HasEnchantment(EquipmentSlot.Waist, "Spellfrost Bolt");
        bool hasFrostfireBoltRune = HasEnchantment(EquipmentSlot.Waist, "Frostfire Bolt");
        bool hasHotStreakRune = HasEnchantment(EquipmentSlot.Waist, "Hot Streak");
        bool hasMissileBarrageRune = HasEnchantment(EquipmentSlot.Waist, "Missile Barrage");

        // Boots
        bool hasSpellPowerRune = HasEnchantment(EquipmentSlot.Feet, "Spell Power");
        bool hasChronostaticPreservationRune = HasEnchantment(EquipmentSlot.Feet, "Chronostatic Preservation");
        bool hasBrainFreezeRune = HasEnchantment(EquipmentSlot.Feet, "Brain Freeze");
        // Gloves
        bool hasIceLanceRune = HasEnchantment(EquipmentSlot.Hands, "Ice Lance");
        bool hasLivingBombRune = HasEnchantment(EquipmentSlot.Hands, "Living Bomb");
        bool hasArcaneBlastRune = HasEnchantment(EquipmentSlot.Hands, "Arcane Blast");
        bool hasRewindTimeRune = HasEnchantment(EquipmentSlot.Hands, "Rewind Time");

        // Chest
        bool hasEnlightenmentRune = HasEnchantment(EquipmentSlot.Chest, "Enlightenment");
        bool hasBurnoutRune = HasEnchantment(EquipmentSlot.Chest, "Burnout");
        bool hasFingersOfFrostRune = HasEnchantment(EquipmentSlot.Chest, "Fingers of Frost");
        bool hasRegenerationRune = HasEnchantment(EquipmentSlot.Chest, "Regeneration");

        // Pants
        bool hasLivingFlameRune = HasEnchantment(EquipmentSlot.Legs, "Living Flame");
        bool hasArcaneSurgeRune = HasEnchantment(EquipmentSlot.Legs, "Arcane Surge");
        bool hasIcyVeinsRune = HasEnchantment(EquipmentSlot.Legs, "Icy Veins");
        bool hasMassRegenerationRune = HasEnchantment(EquipmentSlot.Legs, "Mass Regeneration");


        Console.ResetColor();
    }
}