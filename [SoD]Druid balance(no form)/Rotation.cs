using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;


public class Druid : Rotation
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
    private TimeSpan starsurgeCooldown = TimeSpan.FromSeconds(1.1);
    private DateTime laststarsurgeTime = DateTime.MinValue;

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
        var mana = me.Mana;

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() ||me.IsChanneling() ||me.IsMounted() ||me.HasAura("Drink") || me.HasAura("Food")) return false;
        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }


        if (Api.Spellbook.CanCast("Mark of the Wild") && !me.HasAura("Mark of the Wild"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Mark of the Wild");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Mark of the Wild"))

                return true;
        }
        if (Api.Spellbook.CanCast("Thorns") && !me.HasAura("Thorns"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Thorns");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Thorns"))

                return true;
        }

        if (Api.Spellbook.CanCast("Omen of Clarity") && !me.HasAura("Omen of Clarity"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Omen of Clarity");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Omen of Clarity"))

                return true;
        }

        var target = Api.Target;

       var reaction = me.GetReaction(target);

if (!target.IsDead() && 
    (reaction != UnitReaction.Friendly &&
     reaction != UnitReaction.Honored &&
     reaction != UnitReaction.Revered &&
     reaction != UnitReaction.Exalted) &&
    mana > 20 && !IsNPC(target))
        {

            if (Api.Spellbook.CanCast("Moonfire") && !target.HasAura("Moonfire"))
            {

                
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Moonfire");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Moonfire"))
                                           return true; // Successful cast of Wrath
                                                    // If unable to cast Moonfire, proceed to the next spell
            }
            else
            if (Api.Spellbook.CanCast("Wrath"))
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

        if (Api.Spellbook.CanCast("Rejuvenation") && !me.HasAura("Rejuvenation") && healthPercentage <= 70 && mana >= 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Rejuvenation");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Rejuvenation"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana >= 20 && me.HasAura("Fury of Stormrage"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Touch");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Healing Touch"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana >= 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Touch");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Healing Touch"))
            {
                return true;
            }
        }
        if (!target.HasAura("Sunfire") && targethealth > 30 && Api.HasMacro("Sunfire"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Sunfire");
            Console.ResetColor();
            if (Api.UseMacro("Sunfire"))
            {
                return true;
            }
        }

        if (Api.HasMacro("Starsurge"))
        {
            if ((DateTime.Now - laststarsurgeTime) >= starsurgeCooldown)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Starsurge");
                Console.ResetColor();

                if (Api.UseMacro("Starsurge"))
                {
                    laststarsurgeTime = DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Starsurge is on cooldown. Skipping cast.");
            }
        }
        if (Api.Spellbook.CanCast("Moonfire") && !target.HasAura("Moonfire") && targethealth > 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Moonfire");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Moonfire"))
            {
                return true;
            }
        }



        if (Api.Spellbook.CanCast("Wrath"))
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
    private void LogPlayerStats()
    {
        var me = Api.Player;

        var mana = me.Mana;
        var healthPercentage = me.HealthPercent;


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();
        Console.ResetColor();

        if (me.HasAura("Fury of Stormrage"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Fury of Stormrage");
            Console.ResetColor();
        }

    }
}