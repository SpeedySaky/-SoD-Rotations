using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class Hunter : Rotation
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
    private DateTime lastFeedTime = DateTime.MinValue;
    private DateTime lastChimeraShotTime = DateTime.MinValue;
    private TimeSpan chimeraShotCooldown = TimeSpan.FromSeconds(6.5);
    private DateTime lastCallPetTime = DateTime.MinValue;
    private TimeSpan callPetCooldown = TimeSpan.FromSeconds(10);
    private DateTime lastFlanking = DateTime.MinValue;
    private TimeSpan FlankingCooldown = TimeSpan.FromSeconds(30);
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
        var PetHealth = 0.0f;
        if (IsValid(pet))
        {
            PetHealth = pet.HealthPercent;
        }


        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources
        var mana = me.ManaPercent;


        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsChanneling()) return false;
        if (me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;





        // Add logic here for actions when pet's health is low, e.g., healing spells



        var reaction = me.GetReaction(target);

        if (!target.IsDead() &&
    (reaction != UnitReaction.Friendly &&
     reaction != UnitReaction.Honored &&
     reaction != UnitReaction.Revered &&
     reaction != UnitReaction.Exalted) &&
    mana > 20 && !IsNPC(target) && Api.Spellbook.CanCast("Hunter's Mark") && !target.Auras.Contains("Hunter's Mark") && healthPercentage > 50 && mana > 20)

        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Mark");
            Console.ResetColor();

            if (Api.UseMacro("Mark"))

                return true;

        }




        if (Api.Spellbook.CanCast("Serpent Sting") && target.Auras.Contains("Hunter's Mark") && healthPercentage > 50 && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Serpent Sting");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Serpent Sting"))
                return true;
        }




        return base.PassivePulse();

    }

    public override bool CombatPulse()
    {
        // Variables for player and target instances
        var me = Api.Player;
        var target = Api.Target;
        var targetDistance = target.Position.Distance2D(me.Position);
        var pet = me.Pet();
        var PetHealth = 0.0f;

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


        // Target distance from the player

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsChanneling()) return false;
        if (me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        var meTarget = me.Target;
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


        if (Api.Spellbook.CanCast("Aspect of the Hawk") && !me.Auras.Contains("Aspect of the Hawk", false))

        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Aspect of the Hawk");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Aspect of the Hawk"))

                return true;
        }
        else if (Api.Spellbook.CanCast("Aspect of the Monkey") && !me.Auras.Contains("Aspect of the Monkey", false) && !me.Auras.Contains("Aspect of the Hawk", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Aspect of the Monkey");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Aspect of the Monkey"))

                return true;

        }
        if (Api.Spellbook.CanCast("Hunter's Mark") && !target.Auras.Contains("Hunter's Mark") && Api.HasMacro("Mark"))

        {
            var reaction = me.GetReaction(target);

            if (reaction != UnitReaction.Friendly)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Mark");
                Console.ResetColor();

                if (Api.UseMacro("Mark"))

                    return true;
            }
        }



        // Single Target Abilities
        if (!target.IsDead() && targetDistance >= 8)
        {

            if (Api.Spellbook.CanCast("Serpent Sting") && mana > 15 && !target.Auras.Contains("Serpent Sting") && healthPercentage > 35)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Serpent Sting");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Serpent Sting"))
                    return true;
            }

            if (target.Auras.Contains("Serpent Sting") && Api.HasMacro("Hands"))
            {
                if ((DateTime.Now - lastChimeraShotTime) >= chimeraShotCooldown)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Hands rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                    {
                        lastChimeraShotTime = DateTime.Now;
                        return true;
                    }
                }
                else
                {
                    // If the cooldown period for Chimera Shot hasn't elapsed yet
                    Console.WriteLine("Chimera Shot is on cooldown. Skipping cast.");
                }
            }


            if (Api.Spellbook.CanCast("Auto Shot"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Auto Shot");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Auto Shot"))
                    return true;
            }
        }//end of ranged

        if (!target.IsDead() && targetDistance <= 7)
        {

            if (Api.HasMacro("Flanking") && (DateTime.Now - lastFlanking) >= FlankingCooldown)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Flanking Strike.");
                Console.ResetColor();

                if (Api.UseMacro("Flanking"))
                {
                    lastFlanking = DateTime.Now; // Update the lastCallPetTime after successful casting
                    return true;
                }
            }
            if (Api.Spellbook.CanCast("Wing Clip") && mana > 40 && !target.Auras.Contains("Wing Clip"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Wing Clip");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Wing Clip"))
                    return true;
            }

            if (Api.Spellbook.CanCast("Raptor Strike") && mana > 15)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Raptor Strike");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Raptor Strike"))
                    return true;
            }
        }


        // Check if in combat and if there are multiple targets nearby


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
        var pet = me.Pet();
        var PetHealth = 0.0f;



        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources
        var mana = me.Mana;

        var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();


    }
}