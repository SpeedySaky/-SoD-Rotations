using System;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;



public class RetPala : Rotation
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
    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private DateTime Crusader = DateTime.MinValue;
    private TimeSpan CrusaderCd = TimeSpan.FromSeconds(7);

    private DateTime Recogning = DateTime.MinValue;
    private TimeSpan ReckoningCooldown = TimeSpan.FromSeconds(12);

    private DateTime lastChest = DateTime.MinValue;
    private TimeSpan ChestCd = TimeSpan.FromSeconds(12);



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
        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
        if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 50 && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
            {
                return true;
            }
        }




        if (Api.Spellbook.CanCast("Blessing of Wisdom") && !me.Auras.Contains("Blessing of Might") && !me.Auras.Contains("Blessing of Wisdom") && mana < 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Wisdom");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Blessing of Wisdom"))
            {
                return true;
            }
        }
        else if (!me.Auras.Contains("Blessing of Might") && mana > 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Might");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Blessing of Might"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Devotion Aura") && !me.Auras.Contains("Devotion Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Devotion Aura");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Devotion Aura"))
            {
                return true;
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
        var targethp = target.HealthPercent;
        if (me.IsDead() || me.IsGhost() || me.IsCasting()) return false;
        if (me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;
        bool hasCrusaderStrike = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");
        bool hasHandOfReckoning = HasEnchantment(EquipmentSlot.Hands, "Hand of Reckoning");
        bool hasBeaconOfLight = HasEnchantment(EquipmentSlot.Hands, "Beacon of Light");

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };

        bool hasCrusader = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");









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


        if (Api.Spellbook.CanCast("Devotion Aura") && !me.Auras.Contains("Devotion Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Devotion Aura");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Devotion Aura"))
            {
                return true;
            }
        }
        var hasPoisonDebuff = me.Auras.Contains("Poison") || me.Auras.Contains("Rabies") || me.Auras.Contains("Tetanus") || me.Auras.Contains("Infected Bite");
        if (Api.Spellbook.CanCast("Divine Protection") && Api.Player.HealthPercent < 45 && !me.IsCasting() && !Api.Player.Auras.Contains("Forbearance") && !Api.Spellbook.OnCooldown("Divine Protection"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Divine Protection");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Divine Protection"))
            {
                return true;
            }
        }

        if (me.Auras.Contains("Divine Protection") && healthPercentage <= 50 && Api.Spellbook.CanCast("Holy Light"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Blessing of Protection") && Api.Player.HealthPercent < 30 && !me.IsCasting() && !Api.Player.Auras.Contains("Forbearance") && !Api.Spellbook.OnCooldown("Blessing of Protection"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Protection");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Blessing of Protection"))
            {
                return true;
            }
        }

        if (Api.Player.Auras.Contains("Blessing of Protection") && healthPercentage <= 35 && Api.Spellbook.CanCast("Holy Light") && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
            {
                return true;
            }
        }
        if (Api.Player.Auras.Contains("Lay on Hands") && healthPercentage <= 10 && Api.Spellbook.CanCast("Lay on Hands") && !Api.Spellbook.OnCooldown("Lay on Hands"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Lay on Hands");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Lay on Hands"))
            {
                return true;
            }
        }


        if (hasPoisonDebuff && Api.Spellbook.CanCast("Purify") && mana > 32)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Have poison debuff casting Purify");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Purify"))

                return true;
        }

        if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 50 && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))

                return true;

        }

        if (Api.Spellbook.CanCast("Consecration") && !Api.Spellbook.OnCooldown("Consecration") && targethp >= 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Consecration");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Consecration"))

                return true;

        }

        if (!me.Auras.Contains("Seal of Command") && Api.Spellbook.CanCast("Seal of Command") && !Api.Spellbook.OnCooldown("Seal of Command"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Command");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Command"))

                return true;

        }
        else if (!me.Auras.Contains("Seal of Righteousness") && Api.Spellbook.CanCast("Seal of Righteousness") && !Api.Spellbook.OnCooldown("Seal of Righteousness") && !me.Auras.Contains("Seal of Command"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Righteousness");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Righteousness"))

                return true;

        }

        if (Api.Spellbook.CanCast("Hammer of Justice") && !Api.Spellbook.OnCooldown("Hammer of Justice") && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Hammer of Justice");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hammer of Justice"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Judgement") && !Api.Spellbook.OnCooldown("Judgement") && !Api.Spellbook.OnCooldown("Judgement") && (me.Auras.Contains("Seal of Righteousness") || me.Auras.Contains("Seal of Command")))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Judgement");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Judgement"))
            {
                return true;
            }
        }


        if (Api.HasMacro("Hands"))
        {
            if ((DateTime.Now - Crusader) >= CrusaderCd)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                if (hasCrusaderStrike)
                {
                    if (Api.UseMacro("Hands"))
                    {
                        Crusader = DateTime.Now;
                        return true;
                    }
                    Console.WriteLine("Casting Crusader Strike");
                    // Add logic to cast Crusader Strike using API method
                    // Example: if (Api.UseSpell("Crusader Strike"))
                    // Replace "Crusader Strike" with the correct API method for casting the spell
                }
                else if (hasHandOfReckoning && !me.Auras.Contains("Hand of Reckoning"))
                {
                    if (Api.UseMacro("Hands"))
                    {
                        Recogning = DateTime.Now;
                        return true;
                    }
                    Console.WriteLine("Casting Hand of Reckoning");
                }
                else if (hasBeaconOfLight)
                {
                    Console.WriteLine("Hands rune has Beacon of Light enchantment");
                    // No need to cast Beacon of Light, just log that it has the enchantment
                }

                return true;
            }
            else if (hasBeaconOfLight)
            {
                Console.WriteLine("Hands rune has Beacon of Light enchantment");
                // No need to cast Beacon of Light, just log that it has the enchantment
            }

            return true;
        }



        //DPS rotation



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

        // Assuming me is an instance of a player character
        var hasPoisonDebuff = me.Auras.Contains("Poison");

        if (hasPoisonDebuff)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Have poison debuff");
            Console.ResetColor();
        }
        if (Api.Spellbook.CanCast("Crusader Strike") || Api.Spellbook.CanCast(407676))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Can Cast Crusader Strike");
            Console.ResetColor();


        }
        bool hasCrusader = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");
        
        if (hasCrusader)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Crusade Strike");
            Console.ResetColor();

        }
        bool hasAegis = HasEnchantment(EquipmentSlot.Chest, "Aegis");

        if (hasAegis)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Aegis");
            Console.ResetColor();

        }
        bool hasRebuke = HasEnchantment(EquipmentSlot.Legs, "Rebuke");

        if (hasRebuke)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Rebuke");
            Console.ResetColor();

        }
        bool hasLordaeron = HasEnchantment(EquipmentSlot.Chest, "Horn of Lordaeron");

        if (hasLordaeron)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Horn of Lordaeron");
            Console.ResetColor();

        }
        bool hasReckoning = HasEnchantment(EquipmentSlot.Hands, "Hand of Reckoning");

        if (hasReckoning)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Hand of Reckoning");
            Console.ResetColor();

        }
        bool hasMartyrdom = HasEnchantment(EquipmentSlot.Chest, "Seal of Martyrdom");

        if (hasMartyrdom)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Seal of Martyrdom");
            Console.ResetColor();

        }
        bool hasStorm = HasEnchantment(EquipmentSlot.Chest, "Divine Storm");

        if (hasStorm)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Divine Storm");
            Console.ResetColor();

        }
        bool hasShield = HasEnchantment(EquipmentSlot.Legs, "Avenger’s Shield");

        if (hasShield)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Avenger’s Shield");
            Console.ResetColor();

        }
        bool hasExorcist = HasEnchantment(EquipmentSlot.Legs, "Exorcist");

        if (hasExorcist)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Exorcist");
            Console.ResetColor();

        }
        bool hasSacrifice = HasEnchantment(EquipmentSlot.Legs, "Divine Sacrifice");

        if (hasSacrifice)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Divine Sacrifice");
            Console.ResetColor();

        }
        bool hasBeacon = HasEnchantment(EquipmentSlot.Hands, "Beacon of Light");

        if (hasBeacon)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Beacon of Light");
            Console.ResetColor();

        }
        bool hasJudgements = HasEnchantment(EquipmentSlot.Waist, "Enlightened Judgements");

        if (hasJudgements)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Enlightened Judgements");
            Console.ResetColor();

        }
        bool hasGuarded = HasEnchantment(EquipmentSlot.Feet, "Guarded by the Light");

        if (hasGuarded)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Guarded by the Light");
            Console.ResetColor();

        }
        bool hasSacred = HasEnchantment(EquipmentSlot.Feet, "Sacred Shield");

        if (hasSacred)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Sacred Shield");
            Console.ResetColor();

        }
        bool hasArt = HasEnchantment(EquipmentSlot.Feet, "The Art of War");

        if (hasArt)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has The Art of War");
            Console.ResetColor();

        }
        bool hasInfusion = HasEnchantment(EquipmentSlot.Waist, "Infusion of Light");

        if (hasInfusion)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Infusion of Light");
            Console.ResetColor();

        }
        bool hasSheath = HasEnchantment(EquipmentSlot.Waist, "Sheath of Light");

        if (hasSheath)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Sheath of Light");
            Console.ResetColor();

        }
        bool hasExemplar = HasEnchantment(EquipmentSlot.Legs, "Inspiration Exemplar");

        if (hasSheath)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Inspiration Exemplar");
            Console.ResetColor();

        }








        Console.ResetColor();
    }
}
