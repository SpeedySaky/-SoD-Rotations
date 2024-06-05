using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class SoDHunter : Rotation
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
    private DateTime lastCallPetTime = DateTime.MinValue;
    private TimeSpan callPetCooldown = TimeSpan.FromSeconds(10);
    private DateTime lastFeedTime = DateTime.MinValue;
    private DateTime lastChimeraShotTime = DateTime.MinValue;
    private TimeSpan chimeraShotCooldown = TimeSpan.FromSeconds(6.5);
    private DateTime lastFlanking = DateTime.MinValue;
    private TimeSpan FlankingCooldown = TimeSpan.FromSeconds(30);
    private DateTime lastMarkLogTime = DateTime.MinValue;
    private TimeSpan markCooldown = TimeSpan.FromSeconds(10);
    private DateTime Steady = DateTime.MinValue;
    private TimeSpan SteadyCD = TimeSpan.FromSeconds(5);
    private DateTime Explosive = DateTime.MinValue;
    private TimeSpan ExplosiveCD = TimeSpan.FromSeconds(8);
    private DateTime Carve = DateTime.MinValue;
    private TimeSpan CarveCD = TimeSpan.FromSeconds(8);
    private DateTime Command = DateTime.MinValue;
    private TimeSpan CommandCD = TimeSpan.FromSeconds(60);


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
        // Variables for player and target instances
        var me = Api.Player;
        var target = Api.Target;
        var mana = me.ManaPercent;
        var pet = me.Pet();
        var PetHealth = 0.0f;
        if (IsValid(pet))
        {
            PetHealth = pet.HealthPercent;
        }
        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;


        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }

        // Power percentages for different resources


        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsLooting() || me.IsFlying() || me.Auras.Contains("Drink") || me.Auras.Contains("Food") || me.IsMounted()) return false;
        bool hasLion = HasEnchantment(EquipmentSlot.Chest, "Heart of the Lion");

        string[] Arrows = { "Thorium Headed Arrow", "Jagged Arrow", "Razor Arrow", "Sharp Arrow", "Rough Arrow", "Doomshot", "Ice Threaded Arrow", "Explosive Arrow" };
        string[] Bullets = { "Thorium Shells", "Ice Threaded Bullet", "Rockshard Pellets", "Mithril Gyro-Shot", "Accurate Slugs", "Hi-Impact Mithril Slugs", "Exploding Shot", "Crafted Solid Shot", "Solid Shot", "Crafted Heavy Shot", "Heavy Shot", "Crafted Light Shot" };

        if (me.IsValid())
        {
            if (Api.HasMacro("Chest") && !me.Auras.Contains(409583, false) && hasLion)

            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Chest Rune");
                Console.ResetColor();

                if (Api.UseMacro("Chest"))

                    return true;

            }


            // Add logic here for actions when pet's health is low, e.g., healing spells


            if (Api.Spellbook.CanCast("Aspect of the Cheetah") && !me.Auras.Contains("Aspect of the Cheetah", false) && !me.IsMounted())

            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Aspect of the Cheetah");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Aspect of the Cheetah"))

                    return true;

            }



            if ((!IsValid(pet) || PetHealth < 1) && Api.Spellbook.CanCast("Call Pet"))
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
            if ((!IsValid(pet) || pet.IsDead()) && Api.Spellbook.CanCast("Revive Pet"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Casting Revive Pet");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Revive Pet"))
                {
                    return true;
                }
            }

            if (IsValid(pet) && (DateTime.Now - lastFeedTime).TotalMinutes >= 10 && Api.HasMacro("Feed"))
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
            if (IsValid(pet) && PetHealth < 40 && Api.Spellbook.CanCast("Mend Pet") && !pet.Auras.Contains("Mend Pet") && mana > 10)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Pet health is low healing him");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Mend Pet"))

                    return true;
            }

            if (Api.Spellbook.CanCast("Aspect of the Hawk") && !me.Auras.Contains("Aspect of the Hawk", false) && !me.Auras.Contains("Aspect of the Cheetah", false))

            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Aspect of the Hawk");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Aspect of the Hawk"))

                    return true;
            }
        }
        var reaction = me.GetReaction(target);

        if (target.IsValid())
        {
            if (!target.IsDead() && (reaction != UnitReaction.Friendly && reaction != UnitReaction.Honored && reaction != UnitReaction.Revered && reaction != UnitReaction.Exalted) && mana > 20 && !IsNPC(target) && Api.Spellbook.CanCast("Hunter's Mark") && !target.Auras.Contains("Hunter's Mark") && healthPercentage > 50 && mana > 20 && PetHealth > 50)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Mark");
                Console.ResetColor();

                if (Api.UseMacro("Mark"))

                    // Update the lastMarkTime after successful casting
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
        var targetDistance = target.Position.Distance2D(me.Position);

        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;
        var mana = me.ManaPercent;
        var meTarget = me.Target;
        var pet = me.Pet();
        var targetDistanceToPet = target.Position.Distance2D(pet.Position);

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

        if (!me.IsValid() || !target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };

        //runes
        //legs
        bool hasFlanking = HasEnchantment(EquipmentSlot.Legs, "Flanking Strike");
        bool hasCommand = HasEnchantment(EquipmentSlot.Legs, "Kill Command");
        //hands
        bool hasChimera = HasEnchantment(EquipmentSlot.Hands, "Chimera Shot");
        bool hasExplosive = HasEnchantment(EquipmentSlot.Hands, "Explosive Shot");
        bool hasCarve = HasEnchantment(EquipmentSlot.Hands, "Carve");
        //chest
        bool hasLion = HasEnchantment(EquipmentSlot.Chest, "Heart of the Lion");
        //waist
        bool hasSteady = HasEnchantment(EquipmentSlot.Waist, "Steady Shot");
        bool hasSpecialist = HasEnchantment(EquipmentSlot.Waist, "Melee Specialist");
        //feet
        bool hasDW = HasEnchantment(EquipmentSlot.Feet, "Dual Wield Specialization");




        if ((!IsValid(pet) || PetHealth<1) && Api.Spellbook.CanCast("Call Pet"))
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
        if ((!IsValid(pet) || PetHealth <=1 ) && Api.Spellbook.CanCast("Revive Pet"))
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

        if (Api.Spellbook.CanCast("Intimidation") && Api.Spellbook.HasSpell("Intimidation") && !Api.Spellbook.OnCooldown("Intimidation") && IsValid(pet) && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Intimidation");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Intimidation"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Hunter's Mark") && !target.Auras.Contains("Hunter's Mark"))
        {

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Mark");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hunter's Mark"))
            {
                return true;

            }

        }
        if (Api.Spellbook.CanCast("Bestial Wrath") && Api.Spellbook.HasSpell("Bestial Wrath") && !Api.Spellbook.OnCooldown("Bestial Wrath"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Bestial Wrath");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Bestial Wrath"))
            {
                return true;
            }
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


        string[] Arrows = { "Thorium Headed Arrow", "Jagged Arrow", "Razor Arrow", "Sharp Arrow", "Rough Arrow", "Doomshot", "Ice Threaded Arrow", "Explosive Arrow" };
        string[] Bullets = { "Thorium Shells", "Ice Threaded Bullet", "Rockshard Pellets", "Mithril Gyro-Shot", "Accurate Slugs", "Hi-Impact Mithril Slugs", "Exploding Shot", "Crafted Solid Shot", "Solid Shot", "Crafted Heavy Shot", "Heavy Shot", "Crafted Light Shot" };

        bool hasArrows = true;
        bool hasBullets = true;



       

            if (Api.Spellbook.CanCast("Aspect of the Monkey") && !me.Auras.Contains("Aspect of the Monkey", false))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Aspect of the Monkey");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Aspect of the Monkey"))

                    return true;

            }
            if (Api.Spellbook.CanCast("Deterrence") && Api.UnfriendlyUnitsNearby(10, true) >= 2 && !Api.Spellbook.OnCooldown("Deterrence"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Deterrence");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Deterrence"))

                    return true;

            }
            if (Api.HasMacro("Legs") && hasFlanking && (DateTime.Now - lastFlanking) >= FlankingCooldown)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Flanking Strike.");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    lastFlanking = DateTime.Now; // Update the lastCallPetTime after successful casting
                    return true;
                }
            }
            if (Api.HasMacro("Hands") && hasCarve)
            {
                if ((DateTime.Now - Carve) >= CarveCD)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Hands rune");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                    {
                        Carve = DateTime.Now;
                        return true;
                    }
                }
                else
                {
                    // If the cooldown period for Chimera Shot hasn't elapsed yet
                    Console.WriteLine("Hands Rune is on cooldown. Skipping cast.");
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
            if (Api.Spellbook.CanCast("Mongoose Bite") && mana > 15)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Mongoose Bite");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Mongoose Bite"))
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
            if (Api.Spellbook.CanCast("Attack"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Attack");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Attack"))
                    return true;
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
        var mana = me.ManaPercent;

        var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();


        // Existing code...

        // Check the status of the pet and log accordingly

        if (IsValid(pet))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Pet is alive.");
            Console.ResetColor();
            // Additional actions for when the pet is dead
        }
        else
        if (!IsValid(pet))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Pet is dead.");
            Console.ResetColor();
            // Additional actions for when the pet's health is low
        }
        if (me.Auras.Contains(409583, false))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Has 409583");
            Console.ResetColor();
            // Additional actions for when the pet is dead
        }
        else
        if (me.Auras.Contains(409580, false))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Has 409583");
            Console.ResetColor();
            // Additional actions for when the pet is dead
        }

        // Log the pet's health only when its status changes

        else

                    if (me.Auras.Contains("Heart of the Lion"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Hass Heart of the Lion");
            Console.ResetColor();
            // Additional actions for when the pet's health is low
        }
        //runes
        //legs
        bool hasFlanking = HasEnchantment(EquipmentSlot.Legs, "Flanking Strike");
        if (hasFlanking)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasFlanking");
            Console.ResetColor();

        }
        bool hasCommand = HasEnchantment(EquipmentSlot.Legs, "Kill Command");
        if (hasCommand)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasCommand");
            Console.ResetColor();

        }
        //hands
        bool hasChimera = HasEnchantment(EquipmentSlot.Hands, "Chimera Shot");
        if (hasChimera)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasChimera");
            Console.ResetColor();

        }
        bool hasLion = HasEnchantment(EquipmentSlot.Chest, "Heart of the Lion");
        if (hasLion)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasLion");
            Console.ResetColor();

        }
        bool hasSteady = HasEnchantment(EquipmentSlot.Waist, "Steady Shot");
        if (hasSteady)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasSteady");
            Console.ResetColor();

        }
        bool hasSpecialist = HasEnchantment(EquipmentSlot.Waist, "Melee Specialist");
        if (hasSpecialist)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasSpecialist");
            Console.ResetColor();

        }
        bool hasDW = HasEnchantment(EquipmentSlot.Feet, "Dual Wield Specialization");
        if (hasDW)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasDW");
            Console.ResetColor();

        }

        bool hasExplosive = HasEnchantment(EquipmentSlot.Hands, "Explosive Shot");
        if (hasExplosive)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasExplosive");
            Console.ResetColor();

        }
        bool hasCarve = HasEnchantment(EquipmentSlot.Hands, "Carve");
        if (hasCarve)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hasCarve");
            Console.ResetColor();

        }


    }
}



