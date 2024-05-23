using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class Priest : Rotation
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
    private DateTime lastHands = DateTime.MinValue;
    private TimeSpan Hands = TimeSpan.FromSeconds(14);
    private DateTime lastPants = DateTime.MinValue;
    private TimeSpan Pants = TimeSpan.FromSeconds(60);
    private DateTime lastChest = DateTime.MinValue;
    private TimeSpan Chest = TimeSpan.FromSeconds(8);
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
        var me = Api.Player;
        var mana = me.ManaPercent;
        var target = Api.Target;
        var targethealth = target.HealthPercent;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        // Health percentage of the player
        var healthPercentage = me.HealthPercent;

        // Power percentages for different resources

        // Target distance from the player

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if (Api.Spellbook.CanCast("Renew") && !me.Auras.Contains("Renew") && healthPercentage < 80)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Renew");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Renew"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Power Word: Fortitude") && !me.Auras.Contains("Power Word: Fortitude"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Fortitude");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Fortitude"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Inner Fire") && !me.Auras.Contains("Inner Fire"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Inner Fire");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Inner Fire"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Power Word: Shield") && !me.Auras.Contains("Power Word: Shield"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Shield");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Shield"))
            {
                return true;
            }
        }

        var reaction = me.GetReaction(target);

        if (target.IsDead())
        {
            if (reaction != UnitReaction.Friendly && reaction != UnitReaction.Honored && reaction != UnitReaction.Revered && reaction != UnitReaction.Exalted)
            {
                if (mana >= 5)
                {
                    if (!IsNPC(target))
                    {
                        if (Api.Spellbook.CanCast("Smite"))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Casting Smite");
                            Console.ResetColor();
                            if (Api.Spellbook.Cast("Smite"))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Smite is not ready to be cast.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Target is an NPC.");
                    }
                }
                else
                {
                    Console.WriteLine("Mana is not above 20%.");
                }
            }
            else
            {
                Console.WriteLine("Target is friendly, honored, revered, or exalted.");
            }
        }
        else
        {
            Console.WriteLine("Target is dead.");
        }

        return base.PassivePulse();
    }


    public override bool CombatPulse()
    {
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
        var targethealth = target.HealthPercent;
        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };


        //hands
        bool hasPenance = HasEnchantment(EquipmentSlot.Hands, "Penance");
        bool hasDeath = HasEnchantment(EquipmentSlot.Hands, "Shadow Word: Death");
        bool hasCircle = HasEnchantment(EquipmentSlot.Hands, "Circle of Healing");
        bool hasMindSear = HasEnchantment(EquipmentSlot.Hands, "Mind Sear");

        // Bracer
        bool hasVoidZone = HasEnchantment(EquipmentSlot.Wrist, "Void Zone");
        bool hasSurgeofLight = HasEnchantment(EquipmentSlot.Wrist, "Surge of Light");
        bool hasDespair = HasEnchantment(EquipmentSlot.Wrist, "Despair");

        //legs
        bool hasSharedPain = HasEnchantment(EquipmentSlot.Legs, "Shared Pain");
        bool hasHomunculi = HasEnchantment(EquipmentSlot.Legs, "Homunculi");
        bool hasMending = HasEnchantment(EquipmentSlot.Legs, "Prayer of Mending");
        bool hasBarrier = HasEnchantment(EquipmentSlot.Legs, "Power Word: Barrier");
        bool hasSerendipity = HasEnchantment(EquipmentSlot.Legs, "Serendipity");

        //waist
        bool hasRenew = HasEnchantment(EquipmentSlot.Waist, "Empowered Renew");
        bool hasIRenewedHope = HasEnchantment(EquipmentSlot.Waist, "Renewed Hope");
        bool hasMindSpike = HasEnchantment(EquipmentSlot.Waist, "Mind Spike");

        //feet
        bool hasSuppression = HasEnchantment(EquipmentSlot.Feet, "Pain Suppression");
        bool hasDispersion = HasEnchantment(EquipmentSlot.Feet, "Dispersion");
        bool hasSpirit = HasEnchantment(EquipmentSlot.Feet, "Spirit of the Redeemer");

        //chest
        bool hasVoidPlague = HasEnchantment(EquipmentSlot.Chest, "Void Plague");
        bool hasFaith = HasEnchantment(EquipmentSlot.Chest, "Twisted Faith");
        bool hasStrengthofSoul = HasEnchantment(EquipmentSlot.Chest, "Strength of Soul");

        //head
        bool hasSuffering = HasEnchantment(EquipmentSlot.Head, "Pain and Suffering");
        bool hasEye = HasEnchantment(EquipmentSlot.Head, "Eye of the Void");
        bool hasAegis = HasEnchantment(EquipmentSlot.Head, "Divine Aegis");


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

        // Target distance from the player
        var targetDistance = target.Position.Distance2D(me.Position);
        if (Api.Spellbook.CanCast("Power Word: Shield") && !me.Auras.Contains("Power Word: Shield") && mana > 15 && !me.Auras.Contains("Weakened Soul"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Power Word: Shield");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Power Word: Shield"))
            {
                return true;
            }
        }
        if (Api.HasMacro("Hands"))
        {
            bool hasEnchantmentOnHands = HasEnchantment(EquipmentSlot.Hands, "Penance") ||
                                         HasEnchantment(EquipmentSlot.Hands, "Shadow Word: Death") ||
                                         HasEnchantment(EquipmentSlot.Hands, "Circle of Healing") ||
                                         HasEnchantment(EquipmentSlot.Hands, "Mind Sear");

            if (!hasEnchantmentOnHands)
            {
                Console.WriteLine("No enchantment on Hands. Skipping cast.");
            }
            else if ((DateTime.Now - lastHands) >= Hands)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Hands rune");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    lastHands = DateTime.Now;
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Hands rune is on cooldown. Skipping cast.");
            }
        }

        if (Api.HasMacro("Chest"))
        {
            bool hasEnchantmentOnChest = HasEnchantment(EquipmentSlot.Chest, "Void Plague") ||
                                         HasEnchantment(EquipmentSlot.Chest, "Twisted Faith") ||
                                         HasEnchantment(EquipmentSlot.Chest, "Strength of Soul");

            if (!hasEnchantmentOnChest)
            {
                Console.WriteLine("No enchantment on Chest. Skipping cast.");
            }
            else if ((DateTime.Now - lastChest) >= Chest)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Chest rune");
                Console.ResetColor();

                if (Api.UseMacro("Chest"))
                {
                    lastChest = DateTime.Now;
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Chest rune is on cooldown. Skipping cast.");
            }
        }

        if (Api.HasMacro("Legs"))
        {
            bool hasEnchantmentOnLegs = HasEnchantment(EquipmentSlot.Legs, "Shared Pain") ||
                                        HasEnchantment(EquipmentSlot.Legs, "Homunculi") ||
                                        HasEnchantment(EquipmentSlot.Legs, "Prayer of Mending") ||
                                        HasEnchantment(EquipmentSlot.Legs, "Power Word: Barrier") ||
                                        HasEnchantment(EquipmentSlot.Legs, "Serendipity");

            if (!hasEnchantmentOnLegs)
            {
                Console.WriteLine("No enchantment on Legs. Skipping cast.");
            }
            else if ((DateTime.Now - lastPants) >= Pants)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Legs rune");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    lastPants = DateTime.Now;
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Legs rune is on cooldown. Skipping cast.");
            }
        }
        if (Api.Spellbook.CanCast("Shadow Word: Pain") && !target.Auras.Contains("Shadow Word: Pain") && targethealth >= 30 && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shadow Word: Pain");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Shadow Word: Pain"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Mind Blast") && targethealth >= 30 && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Mind Blast");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Mind Blast"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Smite") && mana > 10)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Smite");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Smite"))

                return true;
        }
        if (Api.Equipment.HasItem(EquipmentSlot.Extra))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ranged weapon is equipped. Attempting to cast Shoot.");
            Console.ResetColor();

            if (Api.HasMacro("Shoot") && Api.UseMacro("Shoot"))
            {
                return true;
            }
        }
        else
        {
            Console.WriteLine("No ranged weapon equipped. Skipping Shoot.");
        }
        if (Api.Spellbook.CanCast("Attack") && mana <= 11)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Attack");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Attack"))

                return true;
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

        var mana = me.ManaPercent;
        var healthPercentage = me.HealthPercent;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();

        // Log enchantments for each rune
        LogEnchantment(EquipmentSlot.Hands, "Penance");
        LogEnchantment(EquipmentSlot.Hands, "Shadow Word: Death");
        LogEnchantment(EquipmentSlot.Hands, "Circle of Healing");
        LogEnchantment(EquipmentSlot.Hands, "Mind Sear");
        LogEnchantment(EquipmentSlot.Wrist, "Void Zone");
        LogEnchantment(EquipmentSlot.Wrist, "Surge of Light");
        LogEnchantment(EquipmentSlot.Wrist, "Despair");
        LogEnchantment(EquipmentSlot.Legs, "Shared Pain");
        LogEnchantment(EquipmentSlot.Legs, "Homunculi");
        LogEnchantment(EquipmentSlot.Legs, "Prayer of Mending");
        LogEnchantment(EquipmentSlot.Legs, "Power Word: Barrier");
        LogEnchantment(EquipmentSlot.Legs, "Serendipity");
        LogEnchantment(EquipmentSlot.Waist, "Empowered Renew");
        LogEnchantment(EquipmentSlot.Waist, "Renewed Hope");
        LogEnchantment(EquipmentSlot.Waist, "Mind Spike");
        LogEnchantment(EquipmentSlot.Feet, "Pain Suppression");
        LogEnchantment(EquipmentSlot.Feet, "Dispersion");
        LogEnchantment(EquipmentSlot.Feet, "Spirit of the Redeemer");
        LogEnchantment(EquipmentSlot.Chest, "Void Plague");
        LogEnchantment(EquipmentSlot.Chest, "Twisted Faith");
        LogEnchantment(EquipmentSlot.Chest, "Strength of Soul");
        LogEnchantment(EquipmentSlot.Head, "Pain and Suffering");
        LogEnchantment(EquipmentSlot.Head, "Eye of the Void");
        LogEnchantment(EquipmentSlot.Head, "Divine Aegis");
    }

    private void LogEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        bool hasEnchantment = HasEnchantment(slot, enchantmentName);
        if (hasEnchantment)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Has {enchantmentName}");
            Console.ResetColor();
        }
    }


}