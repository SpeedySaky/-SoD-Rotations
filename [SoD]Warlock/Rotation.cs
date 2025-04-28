using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.WowBots;
using wShadow.WowBots.PartyInfo;
using System.Linq;



public class WarlockSoD : Rotation
{

    private bool assistedPet = false;
    private DateTime lastAssistPetLogTime = DateTime.MinValue; // Add this field
    private Dictionary<string, DateTime> healthstoneCooldowns = new Dictionary<string, DateTime>();

    private bool HasItem(object item)
        => Api.Inventory.HasItem(item);
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private List<string> npcConditions = new List<string>
    {
        "Innkeeper", "Auctioneer", "Banker", "FlightMaster", "GuildBanker",
        "PlayerVehicle", "StableMaster", "Repair", "Trainer", "TrainerClass",
        "TrainerProfession", "Vendor", "VendorAmmo", "VendorFood", "VendorPoison",
        "VendorReagent", "WildBattlePet", "GarrisonMissionNPC", "GarrisonTalentNPC",
        "QuestGiver"
    };
    private Dictionary<string, DateTime> potionCooldowns = new Dictionary<string, DateTime>();

    public bool IsValid(WowUnit unit)
    {
        if (unit == null || unit.Address == null)
        {
            return false;
        }
        return true;
    }
    private CreatureType GetCreatureType(WowUnit unit)
    {
        return unit.Info.GetCreatureType();
    }
    private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        return Api.Equipment.HasEnchantment(slot, enchantmentName);
    }

    public override void Initialize()
    {
        // Can set min/max levels required for this rotation.

        lastDebugTime = DateTime.Now;
        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

        // Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 750;
        FastTick = 350;

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
        var TargetHealth = 0.0f;
        if (IsValid(target))
        {
            TargetHealth = target.HealthPercent;
        }

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;
        var targethealth = target.HealthPercent;
        ShadowApi shadowApi = new ShadowApi();
        // Target distance from the player
        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if (me.IsValid())
        {
            if (Api.Spellbook.CanCast("Demon Armor") && !me.Auras.Contains("Demon Armor"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Demon Armor");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Demon Armor"))
                    return true;
            }
            if (Api.Spellbook.CanCast("Demon Skin") && !me.Auras.Contains("Demon Armor") && !me.Auras.Contains("Demon Skin"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Demon Skin");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Demon Skin"))
                    return true;
            }

            //if (Api.HasMacro("Hands") && reaction != UnitReaction.Friendly && targethealth>=1)

            var petFamily = Api.Pet.Info.GetCreatureFamily();
            var isImp = petFamily == CreatureFamily.Imp; //|| petFamily == CreatureFamily.Felimp;
            var isVoid = petFamily == CreatureFamily.Voidwalker; //|| petFamily == CreatureFamily.Voidlord;
            var isGuard = petFamily == CreatureFamily.Felguard;
            var isHunter = petFamily == CreatureFamily.Felhunter;
            var isSuccubus = petFamily == CreatureFamily.Succubus;
            //|| !isVoid

            if ((pet == null || PetHealth == 0) && Api.Spellbook.CanCast("Summon Voidwalker") && HasItem("Soul Shard") && mana > 25 && Api.Spellbook.HasSpell("Summon Voidwalker"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Summon Voidwalker.");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Summon Voidwalker"))
                {
                    return true;
                }
            }
            else if ((pet == null || PetHealth == 0) && Api.Spellbook.CanCast("Summon Imp") && mana > 30)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Summon Imp.");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Summon Imp"))
                {
                    return true;
                }
            }


            if (IsValid(pet) && PetHealth < 50 && healthPercentage > 50 && Api.Spellbook.CanCast("Health Funnel"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Healing Pet ");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Health Funnel"))
                    return true;
            }

            if (Api.Spellbook.CanCast("Life Tap") && healthPercentage > 80 && mana < 30)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Life Tap");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Life Tap"))
                {
                    return true;
                }
            }
        }
        if (CreateHealthstone())
        {
            return true; // Exit early if a Healthstone was created
        }


        if (target.IsValid())
        {
            var reaction = me.GetReaction(target);

            if (Api.HasMacro("Hands") && !target.IsDead() && (reaction != UnitReaction.Friendly && reaction != UnitReaction.Honored && reaction != UnitReaction.Revered && reaction != UnitReaction.Exalted) &&
        mana > 20 && !IsNPC(target))

            {
                if ((DateTime.Now - lastHaunt) >= HauntCooldown)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Haunt");
                    Console.ResetColor();

                    if (Api.UseMacro("Hands"))
                    {
                        return true;
                    }
                }
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
        var targethealth = target.HealthPercent;
        var healthPercentage = me.HealthPercent;
        var pet = me.Pet();
        var PetHealth = 0.0f;
        if (!Api.Player.PetGuid.IsEmpty())
        {
            pet = Api.Pet;
            if (IsValid(pet))
            {
                PetHealth = pet.HealthPercent;
            }
        }
        if (!me.IsValid() || !target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };

        if (UsePotions())
        {
            return true; // Exit early if a potion was used
        }
        if (UseHealthstone())
        {
            return true; // Exit early if a potion was used
        }
        if (CreateHealthstone())
        {
            return true; // Exit early if a Healthstone was created
        }

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        var meTarget = me.Target;
        if ((pet == null || PetHealth == 0) && Api.Spellbook.CanCast("Summon Voidwalker") && HasItem("Soul Shard") && mana > 25 && Api.Spellbook.HasSpell("Summon Voidwalker"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Summon Voidwalker.");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Summon Voidwalker"))
            {
                return true;
            }
        }
        else if ((pet == null || PetHealth == 0) && Api.Spellbook.CanCast("Summon Imp") && mana > 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Summon Imp.");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Summon Imp"))
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

        if (Api.Spellbook.CanCast("Drain Soul") && Api.Inventory.ItemCount("Soul Shard") <= 2 && targethealth <= 30 && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Drain Soul");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Drain Soul"))
            {
                return true;
            }
        }

        if ((DateTime.Now - lastLegs) >= LegsCD)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Legs rune");
            Console.ResetColor();

            if (Api.UseMacro("Legs"))
            {
                lastLegs = DateTime.Now;
                return true;
            }
        }
        else
        {
            // If the cooldown period for Chimera Shot hasn't elapsed yet
            Console.WriteLine("Legs rune is on cooldown. Skipping cast.");
        }



        CreatureType targetCreatureType = GetCreatureType(target);


        if (Api.Spellbook.CanCast("Drain Life") && healthPercentage <= 30 && mana >= 5 && targetCreatureType != CreatureType.Undead &&
     targetCreatureType != CreatureType.Elemental && targetCreatureType != CreatureType.Mechanical)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Drain Life");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Drain Life"))
                return true;
        }

        if (Api.Spellbook.CanCast("Curse of Agony") && !target.Auras.Contains("Curse of Agony") && mana >= 10 && targethealth >= 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Curse of Agony");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Curse of Agony"))
                return true;
        }

        if (Api.Spellbook.CanCast("Corruption") && !target.Auras.Contains("Corruption") && mana >= 10 && targethealth >= 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Corruption");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Corruption"))
                return true;
        }



        if (Api.Spellbook.CanCast("Shadow Bolt") && mana >= 10 && targethealth >= 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shadow Bolt");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Shadow Bolt"))
                return true;
        }
        if (Api.HasMacro("Hands") && targethealth >= 30 && mana >= 12)
        {
            if ((DateTime.Now - lastHaunt) >= HauntCooldown)
            {


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Haunt");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    lastHaunt = DateTime.Now;
                    return true;
                }
            }
        }
        if (Api.Spellbook.CanCast("Shadow Bolt") && mana >= 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shadow Bolt");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Shadow Bolt"))
                return true;
        }
        if (Api.Equipment.HasItem(EquipmentSlot.Extra) && Api.Spellbook.CanCast("Shoot") && !me.IsShooting())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ranged weapon is equipped. Attempting to cast Shoot.");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Shoot"))
            {
                return true;
            }
        }
        else if (Api.Spellbook.CanCast("Attack") && (!me.IsAutoAttacking() && !me.IsShooting()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Attack");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Attack"))
            {
                return true;
            }
        }






        return base.CombatPulse();
    }
    private bool CreateHealthstone()
    {
        string[] healthstoneTypes = { "Fel Healthstone", "Demonic Healthstone", "Master Healthstone", "Major Healthstone", "Greater Healthstone", "Healthstone", "Lesser Healthstone", "Minor Healthstone" };
        bool needsHealthstone = true;

        foreach (string healthstoneType in healthstoneTypes)
        {
            if (Api.Inventory.HasItem(healthstoneType))
            {
                needsHealthstone = false;
                break;
            }
        }

        if (needsHealthstone && HasItem("Soul Shard"))
        {
            if (Api.Spellbook.CanCast("Create Healthstone (Fel)") && !Api.Inventory.HasItem("Fel Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone (Fel)"))
                {
                    Console.WriteLine("Creating Fel Healthstone.");
                    return true;
                }
            }
            else if (Api.Spellbook.CanCast("Create Healthstone (Demonic)") && !Api.Inventory.HasItem("Demonic Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone (Demonic)"))
                {
                    Console.WriteLine("Creating Demonic Healthstone.");
                    return true;
                }
            }
            else if (Api.Spellbook.CanCast("Create Healthstone (Master)") && !Api.Inventory.HasItem("Master Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone (Master)"))
                {
                    Console.WriteLine("Creating Master Healthstone.");
                    return true;
                }
            }
            else if (Api.Spellbook.CanCast("Create Healthstone (Major)") && !Api.Inventory.HasItem("Major Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone (Major)"))
                {
                    Console.WriteLine("Creating Major Healthstone.");
                    return true;
                }
            }
            else if (Api.Spellbook.CanCast("Create Healthstone (Greater)") && !Api.Inventory.HasItem("Greater Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone (Greater)"))
                {
                    Console.WriteLine("Creating Greater Healthstone.");
                    return true;
                }
            }
            else if (Api.Spellbook.CanCast("Create Healthstone") && !Api.Inventory.HasItem("Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone"))
                {
                    Console.WriteLine("Creating Healthstone.");
                    return true;
                }
            }
            else if (Api.Spellbook.CanCast("Create Healthstone (Lesser)") && !Api.Inventory.HasItem("Lesser Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone (Lesser)"))
                {
                    Console.WriteLine("Creating Lesser Healthstone.");
                    return true;
                }
            }
            else if (Api.Spellbook.CanCast("Create Healthstone (Minor)") && !Api.Inventory.HasItem("Minor Healthstone"))
            {
                if (Api.Spellbook.Cast("Create Healthstone (Minor)"))
                {
                    Console.WriteLine("Creating Minor Healthstone.");
                    return true;
                }
            }
        }

        return false; // No Healthstone was created
    }


    public bool UseHealthstone()
    {
        // Check for healthstones if health is low
        if (Api.Player.HealthPercent <= 50)
        {
            if (UseHealthstone("Master Healthstone")) return true;
            if (UseHealthstone("Major Healthstone")) return true;
            if (UseHealthstone("Greater Healthstone")) return true;
            if (UseHealthstone("Healthstone")) return true;
            if (UseHealthstone("Lesser Healthstone")) return true;
            if (UseHealthstone("Minor Healthstone")) return true;
        }

        return false; // No healthstones were used
    }
    private bool UseHealthstone(string healthstoneName)
    {
        int healthstoneCount = Api.Inventory.ItemCount(healthstoneName);

        // Check cooldown for healthstones
        bool isOnCooldown = healthstoneCooldowns.ContainsKey("Healthstone") && (DateTime.Now - healthstoneCooldowns["Healthstone"]).TotalSeconds < 120;

        if (healthstoneCount > 0 && !isOnCooldown)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Using {healthstoneName}.");
            Console.ResetColor();

            if (Api.Inventory.Use(healthstoneName))
            {
                healthstoneCooldowns["Healthstone"] = DateTime.Now; // Update the cooldown
                return true; // Exit early after using the healthstone
            }
        }

        return false; // Healthstone was not used
    }
    public bool UsePotions()
    {
        // Check for health potions if health is low
        if (Api.Player.HealthPercent <= 30)
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
        var targethealth = target.HealthPercent;

        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);
        ////var petFamily = Api.Pet.Info.GetCreatureFamily();

        // Log the summoned pet family
        if (petFamily == CreatureFamily.Imp || petFamily == CreatureFamily.Felimp)
        {
            Console.WriteLine("Imp or Felimp is summoned");
        }
        else if (petFamily == CreatureFamily.Voidwalker)
        {
            Console.WriteLine("Voidwalker or Voidlord is summoned");
        }
        else if (petFamily == CreatureFamily.Felguard)
        {
            Console.WriteLine("Felguard is summoned");
        }
        else if (petFamily == CreatureFamily.Felhunter)
        {
            Console.WriteLine("Felhunter is summoned");
        }
        else if (petFamily == CreatureFamily.Succubus)
        {
            Console.WriteLine("Succubus is summoned");
        }
        else
        {
            Console.WriteLine("Unknown pet family is summoned");
        }


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();




    }
}