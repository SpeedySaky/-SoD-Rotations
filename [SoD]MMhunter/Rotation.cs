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
        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;
        var target = Api.Target;
        var targetDistance = target.Position.Distance2D(me.Position);
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

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;


        if (Api.HasMacro("Chest") && !me.Auras.Contains("Heart of the Lion", false))

        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Heart of the Lion");
            Console.ResetColor();

            if (Api.UseMacro("Chest"))

                return true;

        }


        if (Api.Spellbook.CanCast("Aspect of the Cheetah") && !me.Auras.Contains("Aspect of the Cheetah", false))

        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Aspect of the Cheetah");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Aspect of the Cheetah"))

                return true;

        }
        if (IsValid(pet) && PetHealth < 40 && Api.Spellbook.CanCast("Mend Pet") && !pet.Auras.Contains("Mend Pet") && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Pet health is low healing him");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Mend Pet"))

                return true;
        }
        var reaction = me.GetReaction(target);

        if (Api.Spellbook.CanCast("Hunter's Mark") && !target.Auras.Contains("Hunter's Mark") && !target.IsDead() && (reaction != UnitReaction.Friendly && reaction != UnitReaction.Honored && reaction != UnitReaction.Revered && reaction != UnitReaction.Exalted) &&
mana > 20 && !IsNPC(target) && healthPercentage > 50 && mana > 20 && PetHealth > 50)

        {
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Mark");
                Console.ResetColor();

                if (Api.UseMacro("Mark"))
                {
                    return true;
                }


            }
        }


        if (!IsValid(pet) && Api.Spellbook.CanCast("Call Pet") && (DateTime.Now - lastCallPetTime) >= callPetCooldown)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Call Pet.");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Call Pet"))
            {
                lastCallPetTime = DateTime.Now; // Update the lastCallPetTime after successful casting
                return true;
            }
        }
        // Additional actions for when the pet is dead
        else if (!IsValid(pet) && Api.Spellbook.CanCast("Revive Pet"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Revive Pet");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Revive Pet"))
            {
                return true;
            }
        }
        if (IsValid(pet) && !pet.IsDead() && (DateTime.Now - lastFeedTime).TotalMinutes >= 10 && Api.HasMacro("Feed"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Feeding pet.");
            Console.ResetColor();

            if (Api.UseMacro("Feed"))
            {
                lastFeedTime = DateTime.Now; // Update lastFeedTime

                // Log the estimated time until the next feeding attempt
                var nextFeedTime = lastFeedTime.AddMinutes(10);
                var timeUntilNextFeed = nextFeedTime - DateTime.Now;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Next feed pet in: {timeUntilNextFeed.TotalMinutes} minutes.");
                Console.ResetColor();

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
        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;
        var targetDistance = target.Position.Distance2D(me.Position);
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

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        var meTarget = me.Target;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };

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
        if (!IsValid(pet) && Api.Spellbook.CanCast("Revive Pet"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Revive Pet");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Revive Pet"))
            {
                return true;
            }
        }
        if (meTarget == null || target.IsDead())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Assist Pet");
            Console.ResetColor();

            // Use the Target property to set the player's target to the pet's target
            if (Api.UseMacro("AssistPet"))
            {
                // Successfully assisted the pet, continue rotation
                // Don't return true here, continue with the rest of the combat logic
                // without triggering a premature exit
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Mark");
            Console.ResetColor();

            if (Api.UseMacro("Mark"))

                return true;
        }

        if (PetHealth <= 30 && Api.Spellbook.CanCast("Mend Pet") && !pet.Auras.Contains("Mend Pet") && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Pet health is low healing him");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Mend Pet"))

                return true;
            // Add logic here for actions when pet's health is low, e.g., healing spells
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
                    Console.WriteLine("Handds rune is on cooldown. Skipping cast.");
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

            if (Api.HasMacro("Legs") && (DateTime.Now - lastFlanking) >= FlankingCooldown)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Legs rune.");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
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
        if (IsValid(pet))
        {
            PetHealth = pet.HealthPercent;
        }


        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources
        var mana = me.Mana;

        var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();


        // Existing code...

        // Check the status of the pet and log accordingly

        if (!pet.IsDead())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Pet is alive.");
            Console.ResetColor();
            // Additional actions for when the pet is dead
        }
        else
        if (Api.HasPet())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Pet is not summoned.");
            Console.ResetColor();
            // Additional actions for when the pet is dead
        }

        // Log the pet's health only when its status changes

        else

                    if (pet.IsDead())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Pet is dead.");
            Console.ResetColor();
            // Additional actions for when the pet's health is low
        }

        // Remaining code...
    }
}
