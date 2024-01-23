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

    private int debugInterval = 20; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;

    public override void Initialize()
    {
        lastDebugTime = DateTime.Now;
        LogPlayerStats();
        SlowTick = 800;
        FastTick = 200;
        PassiveActions.Add((true, () => false));
        CombatActions.Add((true, () => false));
    }

    public override bool PassivePulse()
    {
        var me = Api.Player;
        var target = Api.Target;
        var pet = me.Pet();

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now;
        }

        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling()) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;

        if (Api.Spellbook.CanCast("Frost Armor") && !me.HasAura("Frost Armor"))
        {
            Console.WriteLine("Casting Frost Armor");
            Api.Spellbook.Cast("Frost Armor");
            return true;
        }

        if (Api.Spellbook.CanCast("Arcane Intellect") && !me.HasPermanent("Arcane Intellect"))
        {
            Console.WriteLine("Casting Arcane Intellect");
            Api.Spellbook.Cast("Arcane Intellect");
            return true;
        }

        string[] waterTypes = { "Conjured Fresh Water", "Conjured Water", "Conjured Purified Water" };
        bool needsWater = true;
foreach (string waterType in waterTypes)
{
    if (HasItem(waterType))
    {
        needsWater = false;
        break;
    }
}
        if (needsWater && Api.Spellbook.CanCast("Conjure Water"))
        {
            Console.WriteLine("Conjured water.");
            Api.Spellbook.Cast("Conjure Water");
            // Add further actions if needed after conjuring water
        }

        string[] foodTypes = { "Conjured Muffin", "Conjured Bread", "Conjured Rye" };
        bool needsFood = true	;
foreach (string foodType in foodTypes)
{
    if (HasItem(foodType))
    {
        needsFood = false;
        break;
    }
}
        if (needsFood && Api.Spellbook.CanCast("Conjure Food"))
        {
            Console.WriteLine("Conjured Food.");
            Api.Spellbook.Cast("Conjure Food");
            // Add further actions if needed after conjuring food
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

        
        return base.PassivePulse();
    }

    // ... (CombatPulse and LogPlayerStats unchanged)

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

    if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
    {
        lastDebugTime = DateTime.Now; // Update lastDebugTime
        // Log stats here, don't call LogPlayerStats() again
        // ...

        // Example log output:
        Console.WriteLine($"Health: {healthPercentage}%, Mana: {mana}");
    }

    if (me.HasAura("Frost Armor"))
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

    Console.ResetColor();
}
}