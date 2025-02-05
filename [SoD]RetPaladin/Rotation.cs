using System;
using System.Linq;
using System.Threading;
using wShadow.Templates;
using System.Collections.Generic;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.WowBots;
using wShadow.WowBots.PartyInfo;

public class EraRetPala : Rotation
{
    // ------------------------------------------------------------
    //  1) RUNE / ENCHANT COOLDOWN TRACKERS & GLOBAL CD
    // ------------------------------------------------------------
    private DateTime HammerOfTheRighteousCd = DateTime.MinValue;
    private TimeSpan HammerOfTheRighteousDuration = TimeSpan.FromSeconds(6);

    private DateTime DivineStormCd = DateTime.MinValue;
    private TimeSpan DivineStormDuration = TimeSpan.FromSeconds(10);

    private DateTime ShieldOfRighteousnessCd = DateTime.MinValue;
    private TimeSpan ShieldOfRighteousnessDuration = TimeSpan.FromSeconds(6);

    private DateTime CrusaderStrikeCd = DateTime.MinValue;
    private TimeSpan CrusaderStrikeDuration = TimeSpan.FromSeconds(6);

    private DateTime ConsecrationCd = DateTime.MinValue;
    private TimeSpan ConsecrationDuration = TimeSpan.FromSeconds(8);

    private DateTime DivineLightCd = DateTime.MinValue;
    private TimeSpan DivineLightDuration = TimeSpan.FromSeconds(8);

    private DateTime AvengerShieldCd = DateTime.MinValue;
    private TimeSpan AvengerShieldDuration = TimeSpan.FromSeconds(15);

    private DateTime RebukeCd = DateTime.MinValue;
    private TimeSpan RebukeDuration = TimeSpan.FromSeconds(15);

    private DateTime HandOfReckoningCd = DateTime.MinValue;
    private TimeSpan HandOfReckoningDuration = TimeSpan.FromSeconds(10);

    // Track Judgement cooldown so that Crusader Strike can “reset” it if needed
    private DateTime lastJudgementCast = DateTime.MinValue; 
    private readonly TimeSpan judgementCooldown = TimeSpan.FromSeconds(8);

    // Global Cooldown
    private DateTime lastGlobalCooldown = DateTime.MinValue;
    private readonly TimeSpan globalCooldownDuration = TimeSpan.FromSeconds(1.5); 

    // ------------------------------------------------------------
    //  2) OTHER IMPORTANT COOLDOWNS / VARIABLES
    // ------------------------------------------------------------
    private DateTime lastDivineProtectionCast = DateTime.MinValue;
    private readonly TimeSpan divineProtectionCooldown = TimeSpan.FromMinutes(5);

    private DateTime lastLayOnHandsCast = DateTime.MinValue;
    private readonly TimeSpan layOnHandsCooldown = TimeSpan.FromMinutes(10);

    private Dictionary<string, DateTime> potionCooldowns = new Dictionary<string, DateTime>();

    // For logging & debugging
    private int debugInterval = 5; // in seconds
    private DateTime lastDebugTime = DateTime.MinValue;

    // NPC detection strings
    private List<string> npcConditions = new List<string>
    {
        "Innkeeper", "Auctioneer", "Banker", "FlightMaster", "GuildBanker",
        "PlayerVehicle", "StableMaster", "Repair", "Trainer", "TrainerClass",
        "TrainerProfession", "Vendor", "VendorAmmo", "VendorFood", "VendorPoison",
        "VendorReagent", "WildBattlePet", "GarrisonMissionNPC", "GarrisonTalentNPC",
        "QuestGiver"
    };

    // ------------------------------------------------------------
    //  3) HELPER METHODS
    // ------------------------------------------------------------
    private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        return Api.Equipment.HasEnchantment(slot, enchantmentName);
    }

    private CreatureType GetCreatureType(WowUnit unit)
    {
        return unit.Info.GetCreatureType();
    }

    private int GetSafeManaPercent()
    {
        var me = Api.Player;
        if (me.IsDead() || me.IsGhost())
            return 0;

        int manaPercent = (int)me.ManaPercent;
        if (manaPercent == 0)
        {
            Console.WriteLine("Fallback enabled: Mana reading is 0 due to a known bug with wepw. Using 100% as fallback.");
            return 100;
        }
        
        return manaPercent;
    }

    private bool IsNPC(WowUnit unit)
    {
        if (!IsValid(unit))
            return false;
        foreach (var condition in npcConditions)
        {
            switch (condition)
            {
                case "Innkeeper"     when unit.IsInnkeeper():
                case "Auctioneer"    when unit.IsAuctioneer():
                case "Banker"        when unit.IsBanker():
                case "FlightMaster"  when unit.IsFlightMaster():
                case "GuildBanker"   when unit.IsGuildBanker():
                case "StableMaster"  when unit.IsStableMaster():
                case "Trainer"       when unit.IsTrainer():
                case "Vendor"        when unit.IsVendor():
                case "QuestGiver"    when unit.IsQuestGiver():
                    return true;
            }
        }
        return false;
    }

    public bool IsValid(WowUnit unit)
    {
        return (unit != null && unit.Address != null);
    }

    private bool UsePotion(string potionName)
    {
        int potionCount = Api.Inventory.ItemCount(potionName);

        // Classic/SoD potions ~2 min CD. We'll track it with "Potion" key.
        bool isOnCooldown = potionCooldowns.ContainsKey("Potion")
                            && (DateTime.Now - potionCooldowns["Potion"]).TotalSeconds < 120;

        if (potionCount > 0 && !isOnCooldown)
        {
            Console.ForegroundColor = potionName.Contains("Mana") ? ConsoleColor.Cyan : ConsoleColor.Green;
            Console.WriteLine($"Using {potionName}.");
            Console.ResetColor();

            if (Api.Inventory.Use(potionName))
            {
                potionCooldowns["Potion"] = DateTime.Now;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Uses a healing or mana potion if conditions are met. 
    /// Returns true if a potion was successfully used.
    /// </summary>
    public bool UsePotions()
    {
        var me = Api.Player;
        if (me.HealthPercent <= 70)
        {
            // Health potions
            if (UsePotion("Major Healing Potion"))    return true;
            if (UsePotion("Superior Healing Potion")) return true;
            if (UsePotion("Greater Healing Potion"))  return true;
            if (UsePotion("Healing Potion"))          return true;
            if (UsePotion("Lesser Healing Potion"))   return true;
            if (UsePotion("Minor Healing Potion"))    return true;
        }
        if (me.ManaPercent < 30)
        {
            // Mana potions
            if (UsePotion("Major Mana Potion"))       return true;
            if (UsePotion("Superior Mana Potion"))    return true;
            if (UsePotion("Greater Mana Potion"))     return true;
            if (UsePotion("Mana Potion"))             return true;
            if (UsePotion("Lesser Mana Potion"))      return true;
            if (UsePotion("Minor Mana Potion"))       return true;
        }
        return false; 
    }

    /// <summary>
    /// Logs some basic debug info at an interval.
    /// </summary>
    private void LogPlayerStats()
    {
        var me = Api.Player;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{me.Mana} Mana available");
        Console.WriteLine($"{me.HealthPercent}% Health available");
        Console.ResetColor();

        // Health Potions
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Available Health Potions:");
        LogPotionCount("Major Healing Potion");
        LogPotionCount("Superior Healing Potion");
        LogPotionCount("Greater Healing Potion");
        LogPotionCount("Healing Potion");
        LogPotionCount("Lesser Healing Potion");
        LogPotionCount("Minor Healing Potion");
        Console.ResetColor();

        // Mana Potions
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Available Mana Potions:");
        LogPotionCount("Major Mana Potion");
        LogPotionCount("Superior Mana Potion");
        LogPotionCount("Greater Mana Potion");
        LogPotionCount("Mana Potion");
        LogPotionCount("Lesser Mana Potion");
        LogPotionCount("Minor Mana Potion");
        Console.ResetColor();

        // Shared potion cooldown
        if (potionCooldowns.ContainsKey("Potion"))
        {
            double remaining = 120 - (DateTime.Now - potionCooldowns["Potion"]).TotalSeconds;
            if (remaining > 0)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Potion cooldown remaining: {Math.Ceiling(remaining)}s");
                Console.ResetColor();
            }
        }
    }

    private void LogPotionCount(string potionName)
    {
        int count = Api.Inventory.ItemCount(potionName);
        Console.WriteLine($"{potionName}: {count}");
    }

    // ------------------------------------------------------------
    //  4) INITIALIZE
    // ------------------------------------------------------------
    public override void Initialize()
    {
        lastDebugTime = DateTime.Now;
        LogPlayerStats();

        // Ticks
        SlowTick = 750;
        FastTick = 300;

        // Action placeholders - you can add more if needed
        PassiveActions.Add((true, () => false));
        CombatActions.Add((true, () => false));
    }

    // ------------------------------------------------------------
    //  5) PASSIVE PULSE
    // ------------------------------------------------------------
    public override bool PassivePulse()
    {
        var me = Api.Player;
        var healthPercentage = me.HealthPercent;
        var mana = GetSafeManaPercent();
        var target = Api.Target;

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() 
            || me.IsChanneling() || me.IsMounted() 
            || me.Auras.Contains("Drink") || me.Auras.Contains("Food"))
        {
            return false;
        }

        // Debug logging
        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now;
        }

        // Quick out-of-combat healing
        if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 50 && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light (OOC heal)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
                return true;
        }

        // Purify if diseased or poison
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


        // Keep a Blessing on ourselves
        if (Api.Spellbook.CanCast("Blessing of Wisdom") 
            && !me.Auras.Contains("Blessing of Wisdom", false)
            && mana < 30) 
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Wisdom (OOC)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Blessing of Wisdom"))
                return true;
        }
        else if (Api.Spellbook.CanCast("Blessing of Might") 
                 && !me.Auras.Contains("Blessing of Might", false)
                 && mana > 80)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Might (OOC)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Blessing of Might"))
                return true;
        }

        // Keep an Aura up
        if (Api.Spellbook.CanCast("Sanctity Aura") 
            && !me.Auras.Contains("Sanctity Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Sanctity Aura (OOC)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Sanctity Aura"))
                return true;
        }
        else if (Api.Spellbook.CanCast("Devotion Aura") 
                 && !me.Auras.Contains("Devotion Aura", false)
                 && !me.Auras.Contains("Sanctity Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Devotion Aura (OOC)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Devotion Aura"))
                return true;
        }

        // Keep a Seal up (if we have a valid target in range)
        if (target.IsValid() && !target.IsDead())
        {
            var dist = target.Position.Distance2D(me.Position);
            // Just example thresholds:
            if (!me.Auras.Contains("Seal of Command", false) 
                && Api.Spellbook.CanCast("Seal of Command") 
                && dist < 30 
                && mana > 30)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Seal of Command (OOC)");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Seal of Command"))
                    return true;
            }
            else if (!me.Auras.Contains("Seal of Wisdom", false) 
                     && Api.Spellbook.CanCast("Seal of Wisdom") 
                     && !me.Auras.Contains("Seal of Command", false)
                     && dist < 30
                     && mana < 20)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Seal of Wisdom (OOC)");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Seal of Wisdom"))
                    return true;
            }
        }

        // Optional: If you want to auto-attack a hostile target out of combat
        var reaction = me.GetReaction(target);
        if (target.IsValid() && !target.IsDead() 
            && reaction <= UnitReaction.Unfriendly 
            && mana > 20 && !IsNPC(target))
        {
            // Possibly cast Judgement if in range
            var dist = target.Position.Distance2D(me.Position);
            if (Api.Spellbook.CanCast("Judgement") && dist > 5 && dist < 10 && !Api.Spellbook.OnCooldown("Judgement"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Judgement (OOC pull)");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Judgement"))
                    return true;
            }
            else if (Api.Spellbook.CanCast("Attack") && !me.IsAutoAttacking())
            {
                Console.WriteLine("Starting Auto-Attack");
                Api.Spellbook.Cast("Attack");
                return true;
            }
        }

        // --------------------------------------------------
        // BLESSING BUFF FOR PARTY MEMBERS (from old code)
        // --------------------------------------------------
        var members = PartyBot.GetMemberUnits();
        if (members == null || members.Length == 0)
        {
            // No party members
            return base.PassivePulse();
        }

        foreach (var member in members)
        {
            if (member == null || !member.IsValid() || member.IsDead()) 
                continue;

            var memberDistance = member.Position.Distance2D(me.Position);
            // If > 30 yards, skip
            if (memberDistance > 30) 
            {
                Console.WriteLine($"{member.Name} is out of range ({memberDistance:F1} yards). Skipping Blessing.");
                continue;
            }

            // Check if the member already has a blessing
            if (member.Auras.Contains("Blessing of Might",    false) 
                || member.Auras.Contains("Blessing of Wisdom",false)
                || member.Auras.Contains("Blessing of Kings", false)
                || member.Auras.Contains("Blessing of Sanctuary", false))
            {
                // Already has a blessing
                continue;
            }

            // If we can cast Blessing of Might on them
            if (Api.Spellbook.CanCast("Blessing of Might"))
            {
                // You might need to target them via PartyBot 
                PartyBot.TargetMember((PartyMember)Array.IndexOf(members, member));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Casting Blessing of Might on {member.Name}.");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Blessing of Might"))
                {
                    Console.WriteLine($"Blessing of Might successfully cast on {member.Name}.");
                }
            }
        }

        return base.PassivePulse();
    }

    // ------------------------------------------------------------
    //  6) COMBAT PULSE
    // ------------------------------------------------------------
    public override bool CombatPulse()
    {
        var me = Api.Player;
        var target = Api.Target;
        if (!IsValid(me) || !IsValid(target) 
            || me.IsDead() || me.IsGhost()
            || me.IsCasting() || me.IsMoving()
            || me.IsChanneling() || me.IsMounted()
            || me.Auras.Contains("Drink") || me.Auras.Contains("Food"))
        {
            return false;
        }

        var healthPercentage = me.HealthPercent;
        var mana = GetSafeManaPercent();
        var targetHealth = target.HealthPercent;
        var dist = target.Position.Distance2D(me.Position);

        // Use potions if needed
        if (UsePotions())
            return true;

        // Keep aura in combat
        if (Api.Spellbook.CanCast("Sanctity Aura") 
            && !me.Auras.Contains("Sanctity Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Sanctity Aura (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Sanctity Aura"))
                return true;
        }
        else if (Api.Spellbook.CanCast("Devotion Aura")
                 && !me.Auras.Contains("Devotion Aura", false)
                 && !me.Auras.Contains("Sanctity Aura", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Devotion Aura (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Devotion Aura"))
                return true;
        }

        // Defensive cooldowns
        if (Api.Spellbook.CanCast("Divine Protection") 
            && !Api.Spellbook.OnCooldown("Divine Protection")
            && (DateTime.Now - lastDivineProtectionCast) >= divineProtectionCooldown
            && healthPercentage < 45 
            && !me.IsCasting()
            && !me.Auras.Contains("Forbearance", false))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Divine Protection (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Divine Protection"))
            {
                lastDivineProtectionCast = DateTime.Now;
                return true;
            }
        }
        if (me.Auras.Contains("Divine Protection", false) 
            && healthPercentage <= 50
            && Api.Spellbook.CanCast("Holy Light"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light under Divine Protection (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
                return true;
        }

        // Blessing of Protection
        if (Api.Spellbook.CanCast("Blessing of Protection") 
            && healthPercentage < 30
            && !me.IsCasting()
            && !me.Auras.Contains("Forbearance", false)
            && !Api.Spellbook.OnCooldown("Blessing of Protection"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Protection (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Blessing of Protection"))
                return true;
        }
        if (me.Auras.Contains("Blessing of Protection", false) 
            && healthPercentage <= 35
            && Api.Spellbook.CanCast("Holy Light")
            && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light under Blessing of Protection (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
                return true;
        }

        // Emergency Lay on Hands
        if (healthPercentage <= 10
            && Api.Spellbook.CanCast("Lay on Hands")
            && !Api.Spellbook.OnCooldown("Lay on Hands")
            && (DateTime.Now - lastLayOnHandsCast) >= layOnHandsCooldown)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Lay on Hands (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Lay on Hands"))
            {
                lastLayOnHandsCast = DateTime.Now;
                return true;
            }
        }

        // Small direct heal if needed
        if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 50 && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light (Combat Heal)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
                return true;
        }

        // Keep or switch Seals in combat
        if (!me.Auras.Contains("Seal of Command", false) 
            && Api.Spellbook.CanCast("Seal of Command")
            && mana > 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Command (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Command"))
                return true;
        }
        else if (!me.Auras.Contains("Seal of Wisdom", false)
                 && Api.Spellbook.CanCast("Seal of Wisdom")
                 && !Api.Spellbook.OnCooldown("Seal of Wisdom")
                 && !me.Auras.Contains("Seal of Command", false)
                 && mana < 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Wisdom (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Wisdom"))
                return true;
        }
        else if (!me.Auras.Contains("Seal of Righteousness", false)
                 && Api.Spellbook.CanCast("Seal of Righteousness")
                 && !Api.Spellbook.OnCooldown("Seal of Righteousness")
                 && !me.Auras.Contains("Seal of Wisdom", false)
                 && !me.Auras.Contains("Seal of Command", false)
                 && mana > 30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Righteousness (Combat)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Righteousness"))
                return true;
        }

        // Priority #1: Judgement (should be used first with a seal up)
        // Check GCD + Judgement cooldown
        if ((DateTime.Now - lastJudgementCast) >= judgementCooldown
            && Api.Spellbook.CanCast("Judgement")
            && mana > 15
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && (me.Auras.Contains("Seal of Righteousness", false) 
                || me.Auras.Contains("Seal of Wisdom", false)
                || me.Auras.Contains("Seal of Command", false)))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Judgement (Highest Priority).");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Judgement"))
            {
                lastJudgementCast = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // Priority #2: Crusader Strike (SoD Rune: Hands) – No mana cost, resets Judgement
        bool hasCrusaderStrike = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");
        if (hasCrusaderStrike
            && (DateTime.Now - CrusaderStrikeCd) >= CrusaderStrikeDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && dist <= 5
            && Api.HasMacro("Hands"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Crusader Strike (Hands Rune).");
            Console.ResetColor();

            // If Crusader Strike resets the Judgement cooldown, we can do that here:
            // lastJudgementCast = DateTime.MinValue; // forcibly reset so it can be cast again
            // or you might rely on in-game script to do it automatically if SoD says so.

            if (Api.UseMacro("Hands"))
            {
                CrusaderStrikeCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;

                // If your SoD enchant instantly resets Judgement, forcibly reset timer:
                lastJudgementCast = DateTime.MinValue;

                return true;
            }
        }

            // --- EXORCISM HANDLING ---
            var targetCreatureType = GetCreatureType(target);
            bool isUndeadOrDemon = (targetCreatureType == CreatureType.Undead || targetCreatureType == CreatureType.Demon);
            var targetDistanceForExorcism = target.Position.Distance2D(me.Position);

            if (Api.Spellbook.CanCast("Exorcism") &&
                !Api.Spellbook.OnCooldown("Exorcism") &&
                targetDistanceForExorcism <= 30 &&
                (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration)
            {
                bool hasArtOfWar = HasEnchantment(EquipmentSlot.Feet, "The Art of War");
                bool hasPurifyingPower = HasEnchantment(EquipmentSlot.Wrist, "Purifying Power");
                
                // Dynamic mana threshold based on enchantments
                int effectiveManaThreshold = hasArtOfWar ? 20 : 50;
                bool targetIsInterruptible = target.IsCasting() || target.IsChanneling();

                if (isUndeadOrDemon)
                {
                    // Priority casting against Undead/Demons
                    if (targetIsInterruptible || GetSafeManaPercent() >= effectiveManaThreshold)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Casting Exorcism on {targetCreatureType}" + 
                                        (hasArtOfWar ? " [Art of War]" : "") + 
                                        (hasPurifyingPower ? " [Purifying Power]" : ""));
                        Console.ResetColor();
                        
                        if (Api.Spellbook.Cast("Exorcism"))
                        {
                            lastGlobalCooldown = DateTime.Now;
                            return true;
                        }
                    }
                }
                else if (GetSafeManaPercent() >= effectiveManaThreshold) // Normal target
                {
                    Console.WriteLine("Casting Exorcism (Available Mana: {GetSafeManaPercent()}%)");
                    if (Api.Spellbook.Cast("Exorcism"))
                    {
                        lastGlobalCooldown = DateTime.Now;
                        return true;
                    }
                }
            }                

        // Hammer of Wrath in execute range
        if (Api.Spellbook.CanCast("Hammer of Wrath") && targetHealth <= 20 && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Hammer of Wrath (Execute Range)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hammer of Wrath"))
                return true;
        }

        // AoE: Consecration if multiple mobs
        if (Api.Spellbook.CanCast("Consecration")
            && !Api.Spellbook.OnCooldown("Consecration")
            && targetHealth >= 30
            && mana > 50
            && Api.UnfriendlyUnitsNearby(5, true) >= 2
            && (DateTime.Now - ConsecrationCd) >= ConsecrationDuration)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Consecration (AoE)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Consecration"))
            {
                ConsecrationCd = DateTime.Now;
                return true;
            }
        }

        // Interrupt w/ Hammer of Justice if no SoD Rebuke
        if (Api.Spellbook.CanCast("Hammer of Justice")
            && mana > 10
            && !Api.Spellbook.OnCooldown("Hammer of Justice")
            && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Casting Hammer of Justice (Interrupt)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hammer of Justice"))
                return true;
        }

        // ------------------------------------------------------------
        //  7) MACRO USAGE (SoD Runes) - VERY IMPORTANT
        // ------------------------------------------------------------
        // * Hammer of the Righteous (Bracer)
        bool hasHotR = HasEnchantment(EquipmentSlot.Wrist, "Hammer of the Righteous");
        if (hasHotR
            && (DateTime.Now - HammerOfTheRighteousCd) >= HammerOfTheRighteousDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && dist <= 5
            && Api.HasMacro("Wrist"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Hammer of the Righteous (Bracer Rune).");
            Console.ResetColor();
            if (Api.UseMacro("Wrist"))
            {
                HammerOfTheRighteousCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // * Divine Storm (Chest)
        bool hasDivineStorm = HasEnchantment(EquipmentSlot.Chest, "Divine Storm");
        if (hasDivineStorm
            && Api.HasMacro("Chest")
            && (DateTime.Now - DivineStormCd) >= DivineStormDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && dist <= 8)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Divine Storm (Chest Rune).");
            Console.ResetColor();
            if (Api.UseMacro("Chest"))
            {
                DivineStormCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // * Shield of Righteousness (Cloak)
        bool hasShieldOfRighteousness = HasEnchantment(EquipmentSlot.Back, "Shield of Righteousness");
        if (hasShieldOfRighteousness
            && Api.HasMacro("Back")
            && (DateTime.Now - ShieldOfRighteousnessCd) >= ShieldOfRighteousnessDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && dist <= 5)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Shield of Righteousness (Cloak Rune).");
            Console.ResetColor();
            if (Api.UseMacro("Back"))
            {
                ShieldOfRighteousnessCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // * Rebuke (Legs) - interrupt if you prefer this over HoJ
        bool hasRebuke = HasEnchantment(EquipmentSlot.Legs, "Rebuke");
        if (hasRebuke
            && Api.HasMacro("Legs")
            && (DateTime.Now - RebukeCd) >= RebukeDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Interrupting with Rebuke (Legs Rune).");
            Console.ResetColor();
            if (Api.UseMacro("Legs"))
            {
                RebukeCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // * Avenger's Shield (Legs) - if you have that enchant instead
        bool hasAvengerShield = HasEnchantment(EquipmentSlot.Legs, "Avenger's Shield");
        if (hasAvengerShield
            && Api.HasMacro("Legs")
            && (DateTime.Now - AvengerShieldCd) >= AvengerShieldDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && dist <= 30)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Avenger's Shield (Legs Rune).");
            Console.ResetColor();
            if (Api.UseMacro("Legs"))
            {
                AvengerShieldCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // * Divine Light (Chest) - big heal if you have that enchant
        bool hasDivineLight = HasEnchantment(EquipmentSlot.Chest, "Divine Light");
        if (hasDivineLight
            && Api.HasMacro("Chest")
            && (DateTime.Now - DivineLightCd) >= DivineLightDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && me.HealthPercent < 80)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Divine Light (Chest Rune).");
            Console.ResetColor();
            if (Api.UseMacro("Chest"))
            {
                DivineLightCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // Auto-attack if nothing else
        if (Api.Spellbook.CanCast("Attack") && !me.IsAutoAttacking())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ensuring we are Auto-Attacking.");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Attack"))
                return true;
        }

        return base.CombatPulse();
    }
}
