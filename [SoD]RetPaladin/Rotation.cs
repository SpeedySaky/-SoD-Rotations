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
    private TimeSpan CrusaderCd = TimeSpan.FromSeconds(6);

    private DateTime Hammer = DateTime.MinValue;
    private TimeSpan HammerCd = TimeSpan.FromSeconds(7);

    private DateTime Storm = DateTime.MinValue;
    private TimeSpan StormCd = TimeSpan.FromSeconds(10.5);

    private DateTime Shield = DateTime.MinValue;
    private TimeSpan ShieldCd = TimeSpan.FromSeconds(30);

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
        SlowTick = 1550;
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
        var me = Api.Player;
        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;
        var target = Api.Target;




        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }

        bool CanSelfBuff = false;

        if (target.Guid.Equals(me.Guid))
            CanSelfBuff = true;

        if (target.Guid.IsEmpty())
            CanSelfBuff = true;



        if (me.IsValid())
        {
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
            var hasDisease = me.Auras.Contains("Contagion of Rot") ||
                                  me.Auras.Contains("Bonechewer Rot") ||
                                  me.Auras.Contains("Ghoul Rot") ||
                                  me.Auras.Contains("Maggot Slime") ||
                                  me.Auras.Contains("Corrupted Strength") ||
                                  me.Auras.Contains("Corrupted Agility") ||
                                  me.Auras.Contains("Corrupted Intellect") ||
                                  me.Auras.Contains("Corrupted Stamina") ||
                                  me.Auras.Contains("Black Rot") ||
                                  me.Auras.Contains("Volatile Infection") ||
                                  me.Auras.Contains("Ghoul Plague") ||
                                  me.Auras.Contains("Corrupting Plague") ||
                                  me.Auras.Contains("Lacerating Bite") ||
                                  me.Auras.Contains("Sporeskin") ||
                                  me.Auras.Contains("Cadaver Worms") ||
                                  me.Auras.Contains("Rabies") ||
                                  me.Auras.Contains("Diseased Shot") ||
                                  me.Auras.Contains("Tetanus") ||
                                  me.Auras.Contains("Dredge Sickness") ||
                                  me.Auras.Contains("Noxious Catalyst") ||
                                  me.Auras.Contains("Spirit Decay") ||
                                  me.Auras.Contains("Withered Touch") ||
                                  me.Auras.Contains("Putrid Enzyme") ||
                                  me.Auras.Contains("Infected Wound") ||
                                  me.Auras.Contains("Infected Spine") ||
                                  me.Auras.Contains("Black Sludge") ||
                                  me.Auras.Contains("Silithid Pox") ||
                                  me.Auras.Contains("Festering Rash") ||
                                  me.Auras.Contains("Dark Sludge") ||
                                  me.Auras.Contains("Fevered Fatigue") ||
                                  me.Auras.Contains("Muculent Fever") ||
                                  me.Auras.Contains("Infected Bite") ||
                                  me.Auras.Contains("Fungal Decay") ||
                                  me.Auras.Contains("Diseased Spit") ||
                                  me.Auras.Contains("Choking Vines") ||
                                  me.Auras.Contains("Fevered Disease") ||
                                  me.Auras.Contains("Lingering Vines") ||
                                  me.Auras.Contains("Festering Wound") ||
                                  me.Auras.Contains("Creeping Vines") ||
                                  me.Auras.Contains("Parasite") ||
                                  me.Auras.Contains("Wandering Plague") ||
                                  me.Auras.Contains("Irradiated") ||
                                  me.Auras.Contains("Dark Plague") ||
                                  me.Auras.Contains("Plague Mind") ||
                                  me.Auras.Contains("Diseased Slime") ||
                                  me.Auras.Contains("Putrid Stench") ||
                                  me.Auras.Contains("Wither") ||
                                  me.Auras.Contains("Seething Plague") ||
                                  me.Auras.Contains("Death's Door") ||
                                  me.Auras.Contains("Plague Strike");

            if (hasDisease && Api.Spellbook.CanCast("Purify") && mana > 32)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Have poison debuff casting Purify");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Purify"))

                    return true;
            }


            if (Api.Spellbook.CanCast("Blessing of Might") && !me.Auras.Contains("Blessing of Might") && mana > 15)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Blessing of Might");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Blessing of Might"))
                {
                    return true;
                }
            }


            if (Api.Spellbook.CanCast("Sanctity Aura") && !me.Auras.Contains("Sanctity Aura", false))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Sanctity Aura");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Sanctity Aura"))
                {
                    return true;
                }
            }
            else
            if (Api.Spellbook.CanCast("Devotion Aura") && !me.Auras.Contains("Devotion Aura", false) && !me.Auras.Contains("Sanctity Aura", false))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Devotion Aura");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Devotion Aura"))
                {
                    return true;
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
        var targethp = target.HealthPercent;
        var targetHealth = Api.Target.HealthPercent;
        if (!me.IsValid() || !target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;

        //hands
        bool hasCrusaderStrike = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");
        bool hasHandOfReckoning = HasEnchantment(EquipmentSlot.Hands, "Hand of Reckoning");
        bool hasBeaconOfLight = HasEnchantment(EquipmentSlot.Hands, "Beacon of Light");

        // Bracer
        bool hasHammeroftheRighteous = HasEnchantment(EquipmentSlot.Wrist, "Hammer of the Righteous");

        //legs
        bool hasExemplar = HasEnchantment(EquipmentSlot.Legs, "Inspiration Exemplar");
        bool hasSacrifice = HasEnchantment(EquipmentSlot.Legs, "Divine Sacrifice");
        bool hasExorcist = HasEnchantment(EquipmentSlot.Legs, "Exorcist");
        bool hasShield = HasEnchantment(EquipmentSlot.Legs, "Avenger's Shield");
        bool hasRebuke = HasEnchantment(EquipmentSlot.Legs, "Rebuke");

        //waist
        bool hasSheath = HasEnchantment(EquipmentSlot.Waist, "Sheath of Light");
        bool hasInfusion = HasEnchantment(EquipmentSlot.Waist, "Infusion of Light");
        bool hasJudgements = HasEnchantment(EquipmentSlot.Waist, "Enlightened Judgements");

        //feet
        bool hasArt = HasEnchantment(EquipmentSlot.Feet, "The Art of War");
        bool hasSacred = HasEnchantment(EquipmentSlot.Feet, "Sacred Shield");
        bool hasGuarded = HasEnchantment(EquipmentSlot.Feet, "Guarded by the Light");

        //chest
        bool hasStorm = HasEnchantment(EquipmentSlot.Chest, "Divine Storm");
        bool hasMartyrdom = HasEnchantment(EquipmentSlot.Chest, "Seal of Martyrdom");
        bool hasLordaeron = HasEnchantment(EquipmentSlot.Chest, "Horn of Lordaeron");
        bool hasAegis = HasEnchantment(EquipmentSlot.Chest, "Aegis");

        string[] HP = { "Major Healing Potion", "Superior Healing Potion", "Greater Healing Potion", "Healing Potion", "Lesser Healing Potion", "Minor Healing Potion" };
        string[] MP = { "Major Mana Potion", "Superior Mana Potion", "Greater Mana Potion", "Mana Potion", "Lesser Mana Potion", "Minor Mana Potion" };

        foreach (string hpot in HP)
        {
            if (HasItem(hpot) && (!Api.Inventory.OnCooldown(HP) || !Api.Inventory.OnCooldown(MP)) && healthPercentage < 70)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Using {hpot}");
                Console.ResetColor();

                if (Api.Inventory.Use(hpot))
                {
                    return true;
                }
            }
        }

        foreach (string mpot in MP)
        {
            if (HasItem(mpot) && (!Api.Inventory.OnCooldown(MP) || !Api.Inventory.OnCooldown(HP)) && mana < 50)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Using {mpot}");
                Console.ResetColor();

                if (Api.Inventory.Use(mpot))
                {
                    return true;
                }
            }
        }





        if (Api.Spellbook.CanCast("Sanctity Aura") && !me.Auras.Contains("Sanctity Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Sanctity Aura");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Sanctity Aura"))
            {
                return true;
            }
        }
        else
         if (Api.Spellbook.CanCast("Devotion Aura") && !me.Auras.Contains("Devotion Aura", false) && !me.Auras.Contains("Sanctity Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Devotion Aura");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Devotion Aura"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Divine Protection") && Api.Player.HealthPercent < 45 && !me.IsCasting() && !Api.Player.Auras.Contains("Forbearance", false) && !Api.Spellbook.OnCooldown("Divine Protection"))
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

        if (Api.Spellbook.CanCast("Blessing of Protection") && Api.Player.HealthPercent < 30 && !me.IsCasting() && !Api.Player.Auras.Contains("Forbearance", false) && !Api.Spellbook.OnCooldown("Blessing of Protection"))
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
        if (healthPercentage <= 10 && Api.Spellbook.CanCast("Lay on Hands") && !Api.Spellbook.OnCooldown("Lay on Hands"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Lay on Hands");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Lay on Hands"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 50 && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))

                return true;

        }

        if (Api.Spellbook.CanCast("Hammer of Wrath") && targetHealth <= 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Hammer of Wrath");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hammer of Wrath"))
            {
                return true;
            }
        }

        if (Api.Spellbook.CanCast("Consecration") && !Api.Spellbook.OnCooldown("Consecration") && targethp >= 30 && mana > 30 && Api.UnfriendlyUnitsNearby(5, true) >= 2)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Consecration");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Consecration"))

                return true;

        }

        if (!me.Auras.Contains("Seal of Wisdom") && Api.Spellbook.CanCast("Seal of Wisdom") && !Api.Spellbook.OnCooldown("Seal of Wisdom") && mana > 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Wisdom");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Wisdom"))

                return true;

        }
        else if (!me.Auras.Contains("Seal of Righteousness") && Api.Spellbook.CanCast("Seal of Righteousness") && !Api.Spellbook.OnCooldown("Seal of Righteousness") && !me.Auras.Contains("Seal of Wisdom") && mana > 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Righteousness");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Righteousness"))

                return true;

        }

        if (Api.Spellbook.CanCast("Hammer of Justice") && mana > 10 && !Api.Spellbook.OnCooldown("Hammer of Justice") && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Hammer of Justice");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hammer of Justice"))
            {
                return true;
            }
        }
        if (Api.HasMacro("Legs") && hasRebuke && (target.IsCasting() || target.IsChanneling()) && Api.Spellbook.OnCooldown("Hammer of Justice"))
        {

            if (Api.UseMacro("Legs"))
                Console.WriteLine("Casting Rebuke rune");
            Console.ResetColor();


            {
                return true;
            }
        }
        else
        if (hasShield && (DateTime.Now - Shield) >= ShieldCd)
        {

            if (Api.UseMacro("Legs"))
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Casting Avenger's Shield rune");
            Console.ResetColor();


            {
                Shield = DateTime.Now;
                return true;
            }
        }

        if (hasExorcist && !Api.Spellbook.OnCooldown("Exorcism"))
        {
            if (Api.UseMacro("Legs"))
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Casting Exorcist rune");
            Console.ResetColor();


            {
                return true;
            }
        }



        if (Api.HasMacro("Chest") && hasAegis && !me.Auras.Contains("Aegis"))
        {

            if (Api.UseMacro("Chest"))
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Casting Aegis rune");
            Console.ResetColor();


            {
                return true;
            }
        }
        else
             if (hasStorm && (DateTime.Now - Storm) >= StormCd)
        {

            if (Api.UseMacro("Chest"))
                Console.ForegroundColor = ConsoleColor.Green;


            Console.WriteLine("Casting Divine Storm rune");
            Console.ResetColor();


            {
                Storm = DateTime.Now;
                return true;
            }

        }


        if (Api.HasMacro("Hands") && (DateTime.Now - Crusader) >= CrusaderCd && hasCrusaderStrike)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Casting Crusader Strike");
            Console.ResetColor();


            if (Api.UseMacro("Hands"))
            {
                Crusader = DateTime.Now;
                return true;
            }
            // Add logic to cast Crusader Strike using API method
            // Example: if (Api.UseSpell("Crusader Strike"))
            // Replace "Crusader Strike" with the correct API method for casting the spell

            else if (hasHandOfReckoning && !me.Auras.Contains("Hand of Reckoning"))
            {
                if (Api.UseMacro("Hands"))
                    Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("Casting Hand of Reckoning");
                Console.ResetColor();


                {
                    Recogning = DateTime.Now;
                    return true;
                }
            }
            else if (hasBeaconOfLight)
            {
                Console.WriteLine("Hands rune has Beacon of Light enchantment");
                Console.ResetColor();

                // No need to cast Beacon of Light, just log that it has the enchantment
            }

            return true;
        }
        else if (hasBeaconOfLight)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Hands rune has Beacon of Light enchantment");
            Console.ResetColor();

            // No need to cast Beacon of Light, just log that it has the enchantment
        }



        if (Api.Spellbook.CanCast("Judgement") && mana > 15 && !Api.Spellbook.OnCooldown("Judgement") && (me.Auras.Contains("Seal of Righteousness") || me.Auras.Contains("Seal of Wisdom")))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Judgement");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Judgement"))
            {
                return true;
            }
        }

        if (Api.HasMacro("Bracer"))
        {
            if ((DateTime.Now - Hammer) >= HammerCd)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                if (hasHammeroftheRighteous)
                {
                    Console.WriteLine("Casting Hammer of the Righteous");
                    Console.ResetColor();

                    if (Api.UseMacro("Bracer"))
                    {
                        Hammer = DateTime.Now;
                        return true;
                    }
                }
            }
            return true;
        }
        if (Api.Spellbook.CanCast("Attack"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Attack");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Attack"))
            {
                return true;
            }
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

        if (Api.Spellbook.CanCast("Hammer of the Righteous"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Can Cast Hammer of the Righteous");
            Console.ResetColor();


        }

        if (Api.Spellbook.CanCast("Hammer of the Righteous") || Api.Spellbook.CanCast(53595))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Can Cast Hammer of the Righteous");
            Console.ResetColor();
        }
        bool hasHammer = HasEnchantment(EquipmentSlot.Wrist, "Hammer of the Righteous");

        if (hasHammer)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Hammer of the Righteous");
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
        bool hasShield = HasEnchantment(EquipmentSlot.Legs, "Avenger's Shield");

        if (hasShield)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Avenger's Shield");
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

        if (hasExemplar)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Has Inspiration Exemplar");
            Console.ResetColor();

        }




    }
}
