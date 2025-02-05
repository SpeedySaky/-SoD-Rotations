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
    //  1) RUNE / ENCHANT COOLDOWN TRACKERS
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

    private DateTime lastExorcismCast = DateTime.MinValue; // Tracks last Exorcism cast
    private TimeSpan baseExorcismCooldown = TimeSpan.FromSeconds(15); // Default cooldown
    private TimeSpan purifyingPowerCooldown = TimeSpan.FromSeconds(7.5); // Purifying Power effect
    private int artOfWarReduction = 2; // The Art of War reduction per crit

private DateTime lastJudgementCast = DateTime.MinValue; // Track last Judgement cast
private readonly TimeSpan judgementCooldown = TimeSpan.FromSeconds(8); // Typical Judgement cooldown

    // ------------------------------------------------------------
    //  2) TRACKING LAST GLOBAL COOLDOWN
    // ------------------------------------------------------------
    private DateTime lastGlobalCooldown = DateTime.MinValue;
    private readonly TimeSpan globalCooldownDuration = TimeSpan.FromSeconds(1.5); // Example GCD

    private List<string> npcConditions = new List<string>
    {
        "Innkeeper", "Auctioneer", "Banker", "FlightMaster", "GuildBanker",
        "PlayerVehicle", "StableMaster", "Repair", "Trainer", "TrainerClass",
        "TrainerProfession", "Vendor", "VendorAmmo", "VendorFood", "VendorPoison",
        "VendorReagent", "WildBattlePet", "GarrisonMissionNPC", "GarrisonTalentNPC",
        "QuestGiver"
    };

    private Dictionary<string, DateTime> potionCooldowns = new Dictionary<string, DateTime>();

    private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
    {
        return Api.Equipment.HasEnchantment(slot, enchantmentName);
    }

    private CreatureType GetCreatureType(WowUnit unit)
    {
        return unit.Info.GetCreatureType();
    }

    private bool HasItem(object item) => Api.Inventory.HasItem(item);

    private int debugInterval = 5; // seconds
    private DateTime lastDebugTime = DateTime.MinValue;

    public bool IsValid(WowUnit unit)
    {
        if (unit == null || unit.Address == null)
            return false;
        return true;
    }

    public override void Initialize()
    {
        lastDebugTime = DateTime.Now;
        LogPlayerStats();

        SlowTick = 750;
        FastTick = 300;

        PassiveActions.Add((true, () => false));
        CombatActions.Add((true, () => false));
    }

    // ------------------------------------------------------------
    //  3) PASSIVE PULSE
    // ------------------------------------------------------------
    public override bool PassivePulse()
    {
        var me = Api.Player;
        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;
        var target = Api.Target;
        var targetDistance = target.Position.Distance2D(me.Position);
        var members = PartyBot.GetMemberUnits();

        if (me.IsDead() || me.IsGhost() 
            || me.IsCasting() || me.IsMoving() 
            || me.IsChanneling() || me.IsMounted()
            || me.Auras.Contains("Drink", false)
            || me.Auras.Contains("Food", false))
        {
            return false;
        }

        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now;
        }

        // Simple self-heal if we're out of combat or not moving

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

        if (Api.Spellbook.CanCast("Blessing of Wisdom") && !me.Auras.Contains("Blessing of Wisdom") && mana <30)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Might");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Blessing of Wisdom"))
            {
                return true;
            }
        }
        if (Api.Spellbook.CanCast("Blessing of Might") && !me.Auras.Contains("Blessing of Might") && mana > 80)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Might");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Blessing of Might"))
            {
                return true;
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


        if (!me.Auras.Contains("Seal of Command") && Api.Spellbook.CanCast("Seal of Command") && mana>50)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Command");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Command"))
                return true;
        }
        else
            if (!me.Auras.Contains("Seal of Wisdom") && Api.Spellbook.CanCast("Seal of Wisdom") && !Api.Spellbook.OnCooldown("Seal of Wisdom") && !me.Auras.Contains("Seal of Command") && mana <20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Wisdom");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Wisdom"))

                return true;

        }
        else
            if (!me.Auras.Contains("Seal of Righteousness") && Api.Spellbook.CanCast("Seal of Righteousness") && !Api.Spellbook.OnCooldown("Seal of Righteousness") && !me.Auras.Contains("Seal of Wisdom") && !me.Auras.Contains("Seal of Command") && mana > 50)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Righteousness");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Righteousness"))

                return true;

        }

        var reaction = me.GetReaction(target);
        if (target.IsValid() && !target.IsDead() && (reaction != UnitReaction.Friendly && reaction != UnitReaction.Honored && reaction != UnitReaction.Revered && reaction != UnitReaction.Exalted) && mana > 20 && !IsNPC(target))
        {
            if (Api.Spellbook.CanCast("Judgement") && targetDistance > 5 && targetDistance < 10 && !Api.Spellbook.OnCooldown("Judgement"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Judgement");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Judgement"))
                    return true;
            }
            else if (Api.Spellbook.CanCast("Attack") && !me.IsAutoAttacking())
            {
                Api.Spellbook.Cast("Attack");
                Console.WriteLine("Attacking");
                return true;
            }
        }
        }
        // --- blessing buff party members //				
       if (members == null || members.Length == 0)
        {
            Console.WriteLine("No party members detected.");
            return base.PassivePulse();
        }

        foreach (var member in members)
{
    if (member == null || !member.IsValid() || member.IsDead())
        continue;

    // Rename to avoid conflict
    var memberDistance = member.Position.Distance2D(me.Position);

    // Skip if party member is out of Blessing of Might range (30 yards)
    if (memberDistance > 30)
    {
        Console.WriteLine($"{member.Name} is out of range ({memberDistance:F1} yards). Skipping.");
        continue;
    }

    // Check if the member already has any Blessing
    if (member.Auras.Contains("Blessing of Might") || 
        member.Auras.Contains("Blessing of Wisdom") || 
        member.Auras.Contains("Blessing of Kings") || 
        member.Auras.Contains("Blessing of Sanctuary"))
    {
        Console.WriteLine($"{member.Name} already has a blessing. Skipping.");
        continue;
    }

    // Apply Blessing of Might if missing and in range
    if (Api.Spellbook.CanCast("Blessing of Might"))
    {
        var partyMember = (PartyMember)Array.IndexOf(members, member);
        PartyBot.TargetMember(partyMember);

        Console.WriteLine($"Casting Blessing of Might on {member.Name} ({memberDistance:F1} yards).");

        if (Api.Spellbook.Cast("Blessing of Might"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Blessing of Might successfully cast on {member.Name}.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to cast Blessing of Might on {member.Name}.");
            Console.ResetColor();
        }
    }
}

        return base.PassivePulse();
    }

    // ------------------------------------------------------------
    //  4) COMBAT PULSE
    // ------------------------------------------------------------
    public override bool CombatPulse()
    {
        var me = Api.Player;
        var healthPercentage = me.HealthPercent;
        var mana = me.ManaPercent;
        var target = Api.Target;
        var targetHealth = target.HealthPercent;
        var targetDistance = target.Position.Distance2D(me.Position);

        if (!me.IsValid() || !target.IsValid() 
            || me.IsDead() || me.IsGhost() 
            || me.IsCasting() || me.IsMoving() 
            || me.IsChanneling() || me.IsMounted()
            || me.Auras.Contains("Drink", false)
            || me.Auras.Contains("Food", false))
        {
            return false;
        }

        if (UsePotions())
            return true;

        // Keep Auras up in combat
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
            && healthPercentage < 45
            && !me.IsCasting() 
            && !Api.Player.Auras.Contains("Forbearance", false)
            && !Api.Spellbook.OnCooldown("Divine Protection"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Divine Protection");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Divine Protection"))
                return true;
        }

        if (me.Auras.Contains("Divine Protection", false) 
            && healthPercentage <= 50 
            && Api.Spellbook.CanCast("Holy Light"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light under Divine Protection");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
                return true;
        }

        if (Api.Spellbook.CanCast("Blessing of Protection")
            && healthPercentage < 30
            && !me.IsCasting()
            && !Api.Player.Auras.Contains("Forbearance", false)
            && !Api.Spellbook.OnCooldown("Blessing of Protection"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Blessing of Protection");
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
            Console.WriteLine("Casting Holy Light under Blessing of Protection");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
                return true;
        }

        // Emergency LoH
        if (healthPercentage <= 10 
            && Api.Spellbook.CanCast("Lay on Hands")
            && !Api.Spellbook.OnCooldown("Lay on Hands"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Lay on Hands");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Lay on Hands"))
                return true;
        }

        // Small direct heal if needed
        if (Api.Spellbook.CanCast("Holy Light") 
            && healthPercentage <= 50 
            && mana > 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Holy Light (Combat Heal)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Holy Light"))
                return true;
        }

        // Exorcism on Undead/Demon
        var hasArtOfWar = HasEnchantment(EquipmentSlot.Wrist, "The Art of War");
        var hasPurifyingPower = HasEnchantment(EquipmentSlot.Wrist, "Purifying Power");
        var targetCreatureType = GetCreatureType(target);
TimeSpan exorcismCooldown = baseExorcismCooldown;
if (hasPurifyingPower)
{
    exorcismCooldown = purifyingPowerCooldown; // Reduce cooldown by 50%
}

// Apply The Art of War reduction dynamically if crits are tracked
if (hasArtOfWar)
{
    exorcismCooldown -= TimeSpan.FromSeconds(artOfWarReduction);
    if (exorcismCooldown < TimeSpan.FromSeconds(1.5)) // Minimum cooldown (GCD-like behavior)
    {
        exorcismCooldown = TimeSpan.FromSeconds(1.5);
    }
}

// Check if Exorcism is ready to cast
if ((DateTime.Now - lastExorcismCast) >= exorcismCooldown)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Exorcism with adjusted cooldown.");
    Console.ResetColor();

    if (Api.Spellbook.CanCast("Exorcism") && Api.Spellbook.Cast("Exorcism"))
    {
        lastExorcismCast = DateTime.Now; // Update cast time
        return true;
    }
}
        // Hammer of Wrath as execute
        if (Api.Spellbook.CanCast("Hammer of Wrath") && targetHealth <= 20)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Hammer of Wrath (Execute Range)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hammer of Wrath"))
                return true;
        }

        // AoE: Consecration
        if (Api.Spellbook.CanCast("Consecration")
            && !Api.Spellbook.OnCooldown("Consecration")
            && targetHealth >= 30
            && mana > 30
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

        // Maintain or switch seals if necessary
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

        // Interrupts
        if (Api.Spellbook.CanCast("Hammer of Justice")
            && mana > 10
            && !Api.Spellbook.OnCooldown("Hammer of Justice")
            && (target.IsCasting() || target.IsChanneling()))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Hammer of Justice (Interrupt)");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Hammer of Justice"))
                return true;
        }

        // Check only when Judgement is off cooldown
        if ((DateTime.Now - lastJudgementCast) >= judgementCooldown
            && Api.Spellbook.CanCast("Judgement")
            && mana > 15
            && (me.Auras.Contains("Seal of Righteousness", false)
                || me.Auras.Contains("Seal of Wisdom", false)
                || me.Auras.Contains("Seal of Command", false)))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Judgement.");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Judgement"))
            {
                lastJudgementCast = DateTime.Now; // Update last cast time only on success
                return true;
            }
        }

        // ------------------------------------------------------------
        //  RUNE / ENCHANTED SPELL USAGE (Macros) in Combat
        // ------------------------------------------------------------
        // 1) Crusader Strike (Hands) [Already in code, example below]
        bool hasCrusaderStrike = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");
        if (hasCrusaderStrike
            && (DateTime.Now - CrusaderStrikeCd) >= CrusaderStrikeDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && targetDistance <= 5)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Crusader Strike (Hands Rune, 5 yards).");
            Console.ResetColor();
            if (Api.UseMacro("Hands"))
            {
                CrusaderStrikeCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // 2) Hand of Reckoning (Hands)
        bool hasHandOfReckoning = HasEnchantment(EquipmentSlot.Hands, "Hand of Reckoning");
        if (hasHandOfReckoning
            && (DateTime.Now - HandOfReckoningCd) >= HandOfReckoningDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && targetDistance <= 30 
            && !Api.Spellbook.OnCooldown("Hand of Reckoning"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Hand of Reckoning (Gloves Rune).");
            Console.ResetColor();
            if (Api.UseMacro("Hands"))
            {
                HandOfReckoningCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // 3) Hammer of the Righteous (Bracer)
        bool hasHammeroftheRighteous = HasEnchantment(EquipmentSlot.Wrist, "Hammer of the Righteous");
        if (hasHammeroftheRighteous
            && (DateTime.Now - HammerOfTheRighteousCd) >= HammerOfTheRighteousDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && targetDistance <= 5
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

        // 4) Divine Storm (Chest)
        bool hasStorm = HasEnchantment(EquipmentSlot.Chest, "Divine Storm");
        if (hasStorm
            && Api.HasMacro("Chest")
            && (DateTime.Now - DivineStormCd) >= DivineStormDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && targetDistance <= 8)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Divine Storm (Chest Rune, 8 yards).");
            Console.ResetColor();
            if (Api.UseMacro("Chest"))
            {
                DivineStormCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // 5) Shield of Righteousness (Cloak)
        bool hasShieldOfRighteousness = HasEnchantment(EquipmentSlot.Back, "Shield of Righteousness");
        if (hasShieldOfRighteousness
            && Api.HasMacro("Back")
            && (DateTime.Now - ShieldOfRighteousnessCd) >= ShieldOfRighteousnessDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && targetDistance <= 5)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Casting Shield of Righteousness (Cloak Rune, 5 yards).");
            Console.ResetColor();
            if (Api.UseMacro("Back"))
            {
                ShieldOfRighteousnessCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // 6) Rebuke (Legs) - Interrupt
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

        // 7) Avenger’s Shield (Legs)
        bool hasShield = HasEnchantment(EquipmentSlot.Legs, "Avenger's Shield");
        if (hasShield
            && Api.HasMacro("Legs")
            && (DateTime.Now - AvengerShieldCd) >= AvengerShieldDuration
            && (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration
            && targetDistance <= 30)
        {
            Console.WriteLine("Casting Avenger's Shield (Legs Rune, 30 yards).");
            if (Api.UseMacro("Legs"))
            {
                AvengerShieldCd = DateTime.Now;
                lastGlobalCooldown = DateTime.Now;
                return true;
            }
        }

        // 8) Divine Light (Chest) - Big heal if we have that rune
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

        // If none of our higher-priority actions fired, ensure we are auto-attacking
        if (Api.Spellbook.CanCast("Attack") && !me.IsAutoAttacking())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Attack (ensuring we are swinging).");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Attack"))
                return true;
        }

        return base.CombatPulse();
    }

    // ------------------------------------------------------------
    //  5) HELPER: USE POTIONS
    // ------------------------------------------------------------
    public bool UsePotions()
    {
        // Use health pot at 70% HP or below
        if (Api.Player.HealthPercent <= 70)
        {
            if (UsePotion("Major Healing Potion")) return true;
            if (UsePotion("Superior Healing Potion")) return true;
            if (UsePotion("Greater Healing Potion")) return true;
            if (UsePotion("Healing Potion")) return true;
            if (UsePotion("Lesser Healing Potion")) return true;
            if (UsePotion("Minor Healing Potion")) return true;
        }

        // Use mana pot at 30% mana or below
        if (Api.Player.ManaPercent < 30)
        {
            if (UsePotion("Major Mana Potion")) return true;
            if (UsePotion("Superior Mana Potion")) return true;
            if (UsePotion("Greater Mana Potion")) return true;
            if (UsePotion("Mana Potion")) return true;
            if (UsePotion("Lesser Mana Potion")) return true;
            if (UsePotion("Minor Mana Potion")) return true;
        }
        return false;
    }

    private bool UsePotion(string potionName)
    {
        int potionCount = Api.Inventory.ItemCount(potionName);
        // Check shared potion cooldown (~2 mins in Classic/SoD)
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

    private bool IsNPC(WowUnit unit)
    {
        if (!IsValid(unit))
            return false;

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

    // ------------------------------------------------------------
    //  6) DEBUGGING / LOGGING
    // ------------------------------------------------------------
    private void LogPlayerStats()
    {
        var me = Api.Player;
        var mana = me.Mana;
        var healthPercentage = me.HealthPercent;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.ResetColor();

        // Log available Health Potions
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Available Health Potions:");
        LogPotionCount("Major Healing Potion");
        LogPotionCount("Superior Healing Potion");
        LogPotionCount("Greater Healing Potion");
        LogPotionCount("Healing Potion");
        LogPotionCount("Lesser Healing Potion");
        LogPotionCount("Minor Healing Potion");
        Console.ResetColor();

        // Log available Mana Potions
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Available Mana Potions:");
        LogPotionCount("Major Mana Potion");
        LogPotionCount("Superior Mana Potion");
        LogPotionCount("Greater Mana Potion");
        LogPotionCount("Mana Potion");
        LogPotionCount("Lesser Mana Potion");
        LogPotionCount("Minor Mana Potion");
        Console.ResetColor();

        // Log shared potion cooldown
        if (potionCooldowns.ContainsKey("Potion"))
        {
            var cooldownRemaining = 120 - (DateTime.Now - potionCooldowns["Potion"]).TotalSeconds;
            if (cooldownRemaining > 0)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Potion cooldown remaining: {Math.Ceiling(cooldownRemaining)}s");
                Console.ResetColor();
            }
        }
    }

    private void LogPotionCount(string potionName)
    {
        int count = Api.Inventory.ItemCount(potionName);
        if (count > 0)
            Console.WriteLine($"{potionName}: {count}");
        else
            Console.WriteLine($"{potionName}: 0");
    }
}
