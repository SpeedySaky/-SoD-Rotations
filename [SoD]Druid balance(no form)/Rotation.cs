using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.WowBots;
using wShadow.WowBots.PartyInfo;


public class SodBalanceDruid : Rotation
{
    private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        return Api.Equipment.HasEnchantment(slot, enchantmentName);
    }
    private List<string> npcConditions = new List<string>
    {
        "Innkeeper", "Auctioneer", "Banker", "FlightMaster", "GuildBanker",
        "PlayerVehicle", "StableMaster", "Repair", "Trainer", "TrainerClass",
        "TrainerProfession", "Vendor", "VendorAmmo", "VendorFood", "VendorPoison",
        "VendorReagent", "WildBattlePet", "GarrisonMissionNPC", "GarrisonTalentNPC",
        "QuestGiver"
    };
    private CreatureType GetCreatureType(WowUnit unit)
    {
        return unit.Info.GetCreatureType();
    }
    public bool IsValid(WowUnit unit)
    {
        if (unit == null || unit.Address == null)
        {
            return false;
        }
        return true;
    }
    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    private Dictionary<string, DateTime> potionCooldowns = new Dictionary<string, DateTime>();

    private int debugInterval = 20; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private DateTime Starsurge = DateTime.MinValue;
    private TimeSpan StarsurgeCD = TimeSpan.FromSeconds(6);

    public override void Initialize()
    {
        // Can set min/max levels required for this rotation.

        lastDebugTime = DateTime.Now;
        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

        // Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 600;
        FastTick = 150;

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

        if ( me.IsDead() || me.IsGhost() || me.IsCasting()  || me.IsChanneling() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;
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

            if (Api.Spellbook.CanCast("Regrowth") && healthPercentage <= 40 && !me.Auras.Contains("Regrowth") && !me.IsMoving())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Regrowth");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Regrowth"))
                    return true;
            }
            if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 30 && !me.IsMoving())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Healing Touch");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Healing Touch"))
                    return true;
            }
            if (Api.Spellbook.CanCast("Moonkin Form") && !me.Auras.Contains("Moonkin Form", false) && mana>35)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Moonkin Form");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Moonkin Form"))

                    return true;
            }
        }
        if (target.IsValid())
        { 
        if (!target.IsDead() && (reaction != UnitReaction.Friendly &&
             reaction != UnitReaction.Honored &&
             reaction != UnitReaction.Revered &&
             reaction != UnitReaction.Exalted) &&
            mana > 20 && !IsNPC(target))
        {

            if (Api.Spellbook.CanCast("Moonfire") && !target.Auras.Contains("Moonfire"))
            {


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Moonfire");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Moonfire"))
                    return true; // Successful cast of Wrath
                                 // If unable to cast Moonfire, proceed to the next spell
            }
            else
            if (Api.Spellbook.CanCast("Wrath") && !me.IsMoving())
            {

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Wrath");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Wrath"))
                {
                    return true; // Successful cast of Wrath
                }
            }


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
        if (!target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        bool hasSunfire = HasEnchantment(EquipmentSlot.Hands, "Sunfire");
        bool hasStarsurge = HasEnchantment(EquipmentSlot.Legs, "Starsurge");
        bool hasStormrage = HasEnchantment(EquipmentSlot.Chest, "Fury of Stormrage");

        if (UsePotions())
        {
            return true; // Exit early if a potion was used
        }

        if (Api.Spellbook.CanCast("Rejuvenation") && !me.Auras.Contains("Rejuvenation") && healthPercentage <= 70 && mana >= 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Rejuvenation");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Rejuvenation"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana >= 20 && me.Auras.Contains("Fury of Stormrage"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Touch");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Healing Touch"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana >= 20 && !me.IsMoving())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Touch");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Healing Touch"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Moonkin Form") && !me.Auras.Contains("Moonkin Form", false) && mana>35)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Moonkin Form");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Moonkin Form"))

                return true;
        }
        if (Api.Spellbook.CanCast("Starfire") && me.Auras.Contains(417157))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Starfire");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Starfire"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Moonfire") && !target.Auras.Contains("Moonfire") && targethealth > 30 && mana >= 5)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Moonfire");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Moonfire"))
            {
                return true;
            }
        }
        var targetCreatureType = GetCreatureType(target);

        if (Api.HasMacro("Hands") && !target.Auras.Contains("Sunfire") && mana >= 5 && targetCreatureType != CreatureType.Elemental)
        {
            if (hasSunfire)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Sunfire");
                Console.ResetColor();
                if (Api.UseMacro("Hands"))
                {

                    return true;
                }

            }
        }
        if (Api.HasMacro("Legs"))
        {
            if ((DateTime.Now - Starsurge) >= StarsurgeCD)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                if (hasStarsurge)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Legs rune");
                    Console.ResetColor();
                    if (Api.UseMacro("Legs"))
                    {
                        Starsurge = DateTime.Now;

                        return true;
                    }

                }
            }
        }
        if (Api.Spellbook.CanCast("Wrath") && me.Auras.Contains(408248))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Wrath with Eclipse");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Wrath"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Wrath") && targetCreatureType != CreatureType.Elemental)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Wrath");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Wrath"))
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
    public bool UsePotions()
    {
        // Check for health potions if health is low
        if (Api.Player.HealthPercent <= 70)
        {
            if (UsePotion("Major Healing Potion")) return true;
            if (UsePotion("Superior Healing Potion")) return true;
            if (UsePotion("Greater Healing Potion")) return true;
            if (UsePotion("Healing Potion")) return true;
            if (UsePotion("Lesser Healing Potion")) return true;
            if (UsePotion("Minor Healing Potion")) return true;
        }

        // Check for mana potions if mana is low
        if (Api.Player.ManaPercent < 30)
        {
            if (UsePotion("Major Mana Potion")) return true;
            if (UsePotion("Superior Mana Potion")) return true;
            if (UsePotion("Greater Mana Potion")) return true;
            if (UsePotion("Mana Potion")) return true;
            if (UsePotion("Lesser Mana Potion")) return true;
            if (UsePotion("Minor Mana Potion")) return true;
        }

        return false; // No potions were used
    }

    private bool UsePotion(string potionName)
    {
        int potionCount = Api.Inventory.ItemCount(potionName);

        // Check cooldown for potions
        bool isOnCooldown = potionCooldowns.ContainsKey("Potion") && (DateTime.Now - potionCooldowns["Potion"]).TotalSeconds < 130;

        if (potionCount > 0 && !isOnCooldown)
        {
            Console.ForegroundColor = potionName.Contains("Mana") ? ConsoleColor.Cyan : ConsoleColor.Green;
            Console.WriteLine($"Using {potionName}.");
            Console.ResetColor();

            if (Api.Inventory.Use(potionName))
            {
                potionCooldowns["Potion"] = DateTime.Now; // Update the cooldown
                return true; // Exit early after using the potion
            }
        }

        return false; // Potion was not used
    }
    private void LogPlayerStats()
    {
        var me = Api.Player;

        var mana = me.ManaPercent;
        var healthPercentage = me.HealthPercent;
        var target = Api.Target;
        var targethealth = target.HealthPercent;
        var reaction = me.GetReaction(target);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();
        Console.ResetColor();

        if (me.Auras.Contains("Fury of Stormrage"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Fury of Stormrage");
            Console.ResetColor();
        }

        if (target.IsValid() && !target.IsDead())
{
    Console.WriteLine("Target is valid and not dead");
    
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"{targethealth}% Target Health");
    Console.WriteLine($"Target Reaction: {reaction}");
    Console.ResetColor();
}
else
{
    Console.WriteLine("Target is not valid or is dead");
}


        bool hasSunfire = HasEnchantment(EquipmentSlot.Hands, "Sunfire");
        bool hasStarsurge = HasEnchantment(EquipmentSlot.Legs, "Starsurge");
        bool hasStormrage = HasEnchantment(EquipmentSlot.Chest, "Fury of Stormrage");

        if (me.Auras.Contains("Mark of the Wild"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Have Mark of the Wild");
            Console.ResetColor();
        }
        if (me.Auras.Contains("Rejuvenation"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Have Rejuvenation");
            Console.ResetColor();
        }
        if (me.Auras.Contains(5234))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Have 5234");
            Console.ResetColor();
        }
        if (Api.Spellbook.CanCast("Mark of the Wild"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Can cast Mark of the Wild");
            Console.ResetColor();
            
        }

        if (hasSunfire)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("HasSunfire");
            Console.ResetColor();

        }

        if (hasStormrage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasStormrage");
            Console.ResetColor();

        }
        if (hasStarsurge)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasStarsurge");
            Console.ResetColor();

        }
        // Log available health potions
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Available Health Potions:");
        LogPotionCount("Major Healing Potion");
        LogPotionCount("Superior Healing Potion");
        LogPotionCount("Greater Healing Potion");
        LogPotionCount("Healing Potion");
        LogPotionCount("Lesser Healing Potion");
        LogPotionCount("Minor Healing Potion");
        Console.ResetColor();

        // Log available mana potions
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Available Mana Potions:");
        LogPotionCount("Major Mana Potion");
        LogPotionCount("Superior Mana Potion");
        LogPotionCount("Greater Mana Potion");
        LogPotionCount("Mana Potion");
        LogPotionCount("Lesser Mana Potion");
        LogPotionCount("Minor Mana Potion");
        Console.ResetColor();

        // Log potion cooldown timer
        if (potionCooldowns.ContainsKey("Potion"))
        {
            var cooldownRemaining = 130 - (DateTime.Now - potionCooldowns["Potion"]).TotalSeconds;
            if (cooldownRemaining > 0)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Potion cooldown remaining: {Math.Ceiling(cooldownRemaining)} seconds");
                Console.ResetColor();
            }
        }
    }
    private void LogPotionCount(string potionName)
    {
        int count = Api.Inventory.ItemCount(potionName);
        Console.WriteLine($"Checking {potionName}: {count}");
        if (count > 0)
        {
            Console.WriteLine($"{potionName}: {count}");
        }
    }
}