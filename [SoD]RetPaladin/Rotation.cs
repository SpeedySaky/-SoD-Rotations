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
	    private CreatureType GetCreatureType(WowUnit unit)
    {
        return unit.Info.GetCreatureType();
    }												  
    private bool HasItem(object item) => Api.Inventory.HasItem(item);
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;

// Cooldown Tracking for Runes/Enchants
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

private DateTime lastGlobalCooldown = DateTime.MinValue;
private TimeSpan globalCooldownDuration = TimeSpan.FromSeconds(1.5);  // Normal GCD


    public override void Initialize()
    {
        // Can set min/max levels required for this rotation.

        lastDebugTime = DateTime.Now;
        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

        // Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 750;
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
		    
			if (!PartyBot.IsEnabled())
            PartyBot.Enable();



    }
    public override bool PassivePulse()
    {
        var me = Api.Player;
		var mana = me.Mana;
        var healthPercentage = me.HealthPercent;
        var manaProc = me.ManaPercent;
        var target = Api.Target;
		var targetDistance = target.Position.Distance2D(me.Position);	
		var members = PartyBot.GetMemberUnits();		

        if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food")) return false;


        if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }

        if (me.IsValid())
        {
            if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 50 && manaProc > 20)
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

            if (hasDisease && Api.Spellbook.CanCast("Purify") && manaProc > 32)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Have poison debuff casting Purify");
                Console.ResetColor();
                if (Api.Spellbook.Cast("Purify"))

                    return true;
            }


            if (Api.Spellbook.CanCast("Blessing of Might") && !me.Auras.Contains("Blessing of Might") && manaProc > 15)
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
		        if (!me.Auras.Contains("Seal of Command") && Api.Spellbook.CanCast("Seal of Command"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Command");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Command"))
                return true;
        }
        if (!me.Auras.Contains("Seal of Wisdom") && Api.Spellbook.CanCast("Seal of Wisdom") && !Api.Spellbook.OnCooldown("Seal of Wisdom") && !me.Auras.Contains("Seal of Command") && mana > 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Wisdom");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Wisdom"))

                return true;

        }
        else if (!me.Auras.Contains("Seal of Righteousness") && Api.Spellbook.CanCast("Seal of Righteousness") && !Api.Spellbook.OnCooldown("Seal of Righteousness") && !me.Auras.Contains("Seal of Wisdom") && !me.Auras.Contains("Seal of Command") && mana > 15)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Seal of Righteousness");
            Console.ResetColor();
            if (Api.Spellbook.Cast("Seal of Righteousness"))

                return true;
        }
        var reaction = me.GetReaction(target);
        if (target.IsValid())
        {
            if (!target.IsDead() &&
            (reaction != UnitReaction.Friendly &&
             reaction != UnitReaction.Honored &&
             reaction != UnitReaction.Revered &&
             reaction != UnitReaction.Exalted) &&
            mana > 20 && !IsNPC(target))
                if (Api.Spellbook.CanCast("Judgement") && targetDistance > 5 && targetDistance <10 && targetDistance < 10 && !Api.Spellbook.OnCooldown("Judgement"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Casting Judgement");
                    Console.ResetColor();

                    if (Api.Spellbook.Cast("Judgement"))
                        return true;
                }
            else  if (Api.Spellbook.CanCast("Attack") && !me.IsAutoAttacking())
                {
                    Api.Spellbook.Cast("Attack");
                    Console.WriteLine("Attacking");
                    return true;
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



public override bool CombatPulse()
{
    var me = Api.Player;
    var target = Api.Target;
    var mana = me.Mana;
    var healthPercentage = me.HealthPercent;
    var targetHealthPercentage = target.HealthPercent;
    var targetDistance = target.Position.Distance2D(me.Position);
    var targetCreatureType = GetCreatureType(target);
    var manaProc = me.ManaPercent;

    const int EXORCISM_BASE_MANA = 80;
    const int JUDGEMENT_MANA = 42;
    const int DIVINE_STORM_MANA = 87;
    const int CONSECRATION_MANA = 135;
    const int CRUSADER_STRIKE_MANA = 30;
    const int DANGER_HEALTH_LEVEL = 30;
    const int LOW_MANA_THRESHOLD = 20;
    const int CRITICAL_HEALTH_THRESHOLD = 10;  // Lay on Hands threshold
    const int LOW_HEALTH_THRESHOLD = 30;       // Divine Protection threshold

    // Early Exit - Check if Player/Target is Invalid
    if (!me.IsValid() || !target.IsValid() || me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsChanneling() || me.IsMounted() || me.Auras.Contains("Drink") || me.Auras.Contains("Food"))
        return false;

    // --- Log Player Stats ---
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"{mana} Mana available");
    Console.WriteLine($"{healthPercentage}% Health available");
    Console.ResetColor();

    // --- Dynamic Runes/Enchants ---
    // Hands
    bool hasCrusaderStrike = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");
    bool hasHandOfReckoning = HasEnchantment(EquipmentSlot.Hands, "Hand of Reckoning");
    bool hasBeaconOfLight = HasEnchantment(EquipmentSlot.Hands, "Beacon of Light");

    // Wrist
    bool hasHammeroftheRighteous = HasEnchantment(EquipmentSlot.Wrist, "Hammer of the Righteous");
    bool hasImprovedHammerOfWrath = HasEnchantment(EquipmentSlot.Wrist, "Improved Hammer of Wrath");
    bool hasPurifyingPower = HasEnchantment(EquipmentSlot.Wrist, "Purifying Power");
    bool hasLightsGrace = HasEnchantment(EquipmentSlot.Wrist, "Light's Grace");

    // Legs
    bool hasExemplar = HasEnchantment(EquipmentSlot.Legs, "Inspiration Exemplar");
    bool hasSacrifice = HasEnchantment(EquipmentSlot.Legs, "Divine Sacrifice");
    bool hasShield = HasEnchantment(EquipmentSlot.Legs, "Avenger's Shield");
    bool hasRebuke = HasEnchantment(EquipmentSlot.Legs, "Rebuke");
    bool hasAuraMastery = HasEnchantment(EquipmentSlot.Legs, "Aura Mastery");

    // Waist
    bool hasSheath = HasEnchantment(EquipmentSlot.Waist, "Sheath of Light");
    bool hasInfusion = HasEnchantment(EquipmentSlot.Waist, "Infusion of Light");
    bool hasMalleableProtection = HasEnchantment(EquipmentSlot.Waist, "Malleable Protection");

    // Feet
    bool hasArt = HasEnchantment(EquipmentSlot.Feet, "The Art of War");
    bool hasSacred = HasEnchantment(EquipmentSlot.Feet, "Sacred Shield");
    bool hasGuarded = HasEnchantment(EquipmentSlot.Feet, "Guarded by the Light");

    // Chest
    bool hasStorm = HasEnchantment(EquipmentSlot.Chest, "Divine Storm");
    bool hasAegis = HasEnchantment(EquipmentSlot.Chest, "Aegis");
    bool hasHallowedGround = HasEnchantment(EquipmentSlot.Chest, "Hallowed Ground");
    bool hasDivineLight = HasEnchantment(EquipmentSlot.Chest, "Divine Light");

    // Back
    bool hasShieldOfRighteousness = HasEnchantment(EquipmentSlot.Back, "Shield of Righteousness");
    bool hasShockAndAwe = HasEnchantment(EquipmentSlot.Back, "Shock and Awe");
    bool hasRighteousVengeance = HasEnchantment(EquipmentSlot.Back, "Righteous Vengeance");

    // Head
    bool hasFanaticism = HasEnchantment(EquipmentSlot.Head, "Fanaticism");
    bool hasImprovedSanctuary = HasEnchantment(EquipmentSlot.Head, "Improved Sanctuary");
    bool hasWrath = HasEnchantment(EquipmentSlot.Head, "Wrath");

    // --- Potion Handling (Health and Mana) ---
    if (healthPercentage <= DANGER_HEALTH_LEVEL || manaProc <= LOW_MANA_THRESHOLD)
    {
        Console.WriteLine("Entering Potion Handling Phase.");

        // Health Potion Usage
        if (healthPercentage <= DANGER_HEALTH_LEVEL)
        {
            var healthPotions = Api.Inventory.ItemNames?
                .Where(x => x.Contains("Healing Potion"))
                .ToArray() ?? Array.Empty<string>();

            foreach (var potion in healthPotions)
            {
                if (!Api.Inventory.OnCooldown(potion) && Api.Inventory.CanUse(potion))
                {
                    Console.WriteLine($"Using {potion} to restore health.");
                    if (Api.Inventory.Use(potion))
                    {
                        lastGlobalCooldown = DateTime.Now;
                        return true;
                    }
                }
            }
            Console.WriteLine("No healing potions found or all are on cooldown.");
        }

        // Mana Potion Usage
        if (manaProc <= LOW_MANA_THRESHOLD)
        {
            var manaPotions = Api.Inventory.ItemNames?
                .Where(x => x.Contains("Mana Potion"))
                .ToArray() ?? Array.Empty<string>();

            foreach (var potion in manaPotions)
            {
                if (!Api.Inventory.OnCooldown(potion) && Api.Inventory.CanUse(potion))
                {
                    Console.WriteLine($"Using {potion} to restore mana.");
                    if (Api.Inventory.Use(potion))
                    {
                        lastGlobalCooldown = DateTime.Now;
                        return true;
                    }
                }
            }
            Console.WriteLine("No mana potions found or all are on cooldown.");
        }

        // If no potions work, fall back to defensive cooldowns
            if (healthPercentage <= LOW_HEALTH_THRESHOLD && 
        Api.Spellbook.CanCast("Divine Protection") && 
        !Api.Spellbook.OnCooldown("Divine Protection"))
    {
        Console.WriteLine("Activating Divine Protection.");
        if (Api.Spellbook.Cast("Divine Protection"))
        {
            lastGlobalCooldown = DateTime.Now;
            return true;
        }
    }

    if (healthPercentage <= CRITICAL_HEALTH_THRESHOLD && 
        Api.Spellbook.CanCast("Lay on Hands") && 
        !Api.Spellbook.OnCooldown("Lay on Hands"))
    {
        Console.WriteLine("Casting Lay on Hands.");
        if (Api.Spellbook.Cast("Lay on Hands"))
        {
            lastGlobalCooldown = DateTime.Now;
            return true;
        }
    }
    }


    // --- SEAL MANAGEMENT ---
    if (manaProc < 25 && !me.Auras.Contains("Seal of Wisdom") && Api.Spellbook.CanCast("Seal of Wisdom"))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Applying Seal of Wisdom for Mana Regeneration.");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Seal of Wisdom"))
        {
            lastGlobalCooldown = DateTime.Now;
            return true;
        }
    }

    if (!me.Auras.Contains("Seal of Righteousness") && Api.Spellbook.CanCast("Seal of Righteousness") && manaProc > 30)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Reapplying Seal of Righteousness.");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Seal of Righteousness"))
        {
            lastGlobalCooldown = DateTime.Now;
            return true;
        }
    }



// --- Judgement - 10 Yards ---
if (Api.Spellbook.CanCast("Judgement") && 
    mana >= JUDGEMENT_MANA && 
    !Api.Spellbook.OnCooldown("Judgement") &&
    targetDistance <= 10)
{
    if (me.Auras.Contains("Seal of Righteousness") || me.Auras.Contains("Seal of Wisdom"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Judgement (10 yards).");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Judgement"))
            return true;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Judgement skipped – No active seal detected.");
        Console.ResetColor();
    }
}
else
{
    Console.WriteLine($"Judgement skipped. Mana: {mana}/{JUDGEMENT_MANA}, Cooldown: {Api.Spellbook.OnCooldown("Judgement")}");
}

// --- Crusader Strike (Rune) - 5 Yards ---
if (hasCrusaderStrike && 
    (DateTime.Now - CrusaderStrikeCd) >= CrusaderStrikeDuration &&
    (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration &&
    targetDistance <= 5)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Casting Crusader Strike (5 yards).");
    Console.ResetColor();
    if (Api.UseMacro("Hands"))  // Crusader Strike is a hands rune
    {
        CrusaderStrikeCd = DateTime.Now;
        lastGlobalCooldown = DateTime.Now;
        return true;
    }
}

// --- Divine Storm (AoE) - 8 Yards ---
if (hasStorm && 
    (DateTime.Now - DivineStormCd) >= DivineStormDuration &&
    (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration &&
    targetDistance <= 8)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Casting Divine Storm (8 yards).");
    Console.ResetColor();
    if (Api.UseMacro("Chest"))  // Divine Storm is a chest rune
    {
        DivineStormCd = DateTime.Now;
        lastGlobalCooldown = DateTime.Now;
        return true;
    }
}

// --- Consecration (AoE) ---
if (Api.Spellbook.CanCast("Consecration") && mana >= CONSECRATION_MANA &&
    (DateTime.Now - ConsecrationCd) >= ConsecrationDuration && Api.UnfriendlyUnitsNearby(8, true) >= 2)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Casting Consecration (AoE).");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Consecration"))
    {
        ConsecrationCd = DateTime.Now;
        return true;
    }
}

    // --- Exorcism (Undead/Demon Priority) ---
    if (Api.Spellbook.CanCast("Exorcism") && !Api.Spellbook.OnCooldown("Exorcism") && 
        targetDistance <= 30 && mana >= EXORCISM_BASE_MANA && 
        (targetCreatureType == CreatureType.Undead || targetCreatureType == CreatureType.Demon))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Casting Exorcism: High Priority on Undead/Demon.");
        Console.ResetColor();
        
        if (Api.Spellbook.Cast("Exorcism"))
        {
            Console.WriteLine("Exorcism successfully cast.");
            return true;
        }
        else
        {
            Console.WriteLine("Exorcism cast failed.");
        }
    }

    // --- General Exorcism Use (If Demon/Undead is Not Target) ---
    if (Api.Spellbook.CanCast("Exorcism") && !Api.Spellbook.OnCooldown("Exorcism") && targetDistance <= 30)
    {
        Console.WriteLine("Casting Exorcism as part of the normal rotation.");
        if (Api.Spellbook.Cast("Exorcism")) return true;
    }

// --- INTERRUPTS MANAGEMENT ---
if (Api.HasMacro("Legs") && hasRebuke && 
    (DateTime.Now - RebukeCd) >= RebukeDuration &&
    (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration &&
    (target.IsCasting() || target.IsChanneling()))
{
    Console.WriteLine("Interrupting with Rebuke.");
    if (Api.UseMacro("Legs"))
    {
        RebukeCd = DateTime.Now;
        lastGlobalCooldown = DateTime.Now;
        return true;
    }
}

if (Api.Spellbook.CanCast("Hammer of Justice") && 
    (target.IsCasting() || target.IsChanneling()) &&
    (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration)
{
    Console.WriteLine("Interrupting with Hammer of Justice.");
    if (Api.Spellbook.Cast("Hammer of Justice"))
    {
        lastGlobalCooldown = DateTime.Now;
        return true;
    }
}

// --- Avenger’s Shield (Legs Rune) - 30 Yards ---
if (Api.HasMacro("Legs") && hasShield && 
    (DateTime.Now - AvengerShieldCd) >= AvengerShieldDuration &&
    (DateTime.Now - lastGlobalCooldown) >= globalCooldownDuration &&
    targetDistance <= 30)
{
    Console.WriteLine("Casting Avenger's Shield (30 yards).");
    if (Api.UseMacro("Legs"))
    {
        AvengerShieldCd = DateTime.Now;
        lastGlobalCooldown = DateTime.Now;
        return true;
    }
}


// --- DEFENSIVE / HEALING ---
// SACRED SHIELD (Feet Rune)
if (Api.HasMacro("Feet") && hasSacred && !me.Auras.Contains("Sacred Shield") && healthPercentage < 50)
{
    Console.WriteLine("Applying Sacred Shield.");
    if (Api.UseMacro("Feet"))
        return true;
}

// DIVINE PROTECTION
if (Api.Spellbook.CanCast("Divine Protection") && healthPercentage < 45)
{
    Console.WriteLine("Activating Divine Protection.");
    if (Api.Spellbook.Cast("Divine Protection"))
        return true;
}

// LAY ON HANDS
if (healthPercentage <= 10 && Api.Spellbook.CanCast("Lay on Hands") && !Api.Spellbook.OnCooldown("Lay on Hands"))
{
    Console.WriteLine("Casting Lay on Hands.");
    if (Api.Spellbook.Cast("Lay on Hands"))
        return true;
}

// --- COOLDOWN MANAGEMENT ---
// AURA MASTERY (Legs Rune)
if (Api.HasMacro("Legs") && hasAuraMastery && !Api.Spellbook.OnCooldown("Aura Mastery"))
{
    Console.WriteLine("Casting Aura Mastery.");
    if (Api.UseMacro("Legs"))
        return true;
}

// DIVINE SACRIFICE (Legs Rune)
if (Api.HasMacro("Legs") && hasSacrifice && healthPercentage > 50)
{
    Console.WriteLine("Casting Divine Sacrifice.");
    if (Api.UseMacro("Legs"))
        return true;
}

// --- UTILITY & BUFFS ---
// BEACON OF LIGHT (Hands Rune)
if (hasBeaconOfLight && Api.HasMacro("Hands"))
{
    Console.WriteLine("Beacon of Light is active - Passive Healing.");
}

// GUARDED BY THE LIGHT (Feet Rune)
if (hasGuarded)
{
    Console.WriteLine("Guarded by the Light - Passive Mana Regen.");
}

// --- DEFAULT ATTACK ---
if (Api.Spellbook.CanCast("Attack") && !me.IsAutoAttacking())
{
    Console.WriteLine("Starting Auto Attack.");
    if (Api.Spellbook.Cast("Attack"))
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

    var hasPoisonDebuff = me.Auras.Contains("Poison");

    if (hasPoisonDebuff)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Have poison debuff");
        Console.ResetColor();
    }

    // Hands
    bool hasCrusaderStrike = HasEnchantment(EquipmentSlot.Hands, "Crusader Strike");
    if (hasCrusaderStrike)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Crusader Strike");
        Console.ResetColor();
    }

    bool hasHandOfReckoning = HasEnchantment(EquipmentSlot.Hands, "Hand of Reckoning");
    if (hasHandOfReckoning)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Hand of Reckoning");
        Console.ResetColor();
    }

    bool hasBeaconOfLight = HasEnchantment(EquipmentSlot.Hands, "Beacon of Light");
    if (hasBeaconOfLight)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Beacon of Light");
        Console.ResetColor();
    }

    // Wrist
    bool hasHammeroftheRighteous = HasEnchantment(EquipmentSlot.Wrist, "Hammer of the Righteous");
    if (hasHammeroftheRighteous)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Hammer of the Righteous");
        Console.ResetColor();
    }

    bool hasImprovedHammerOfWrath = HasEnchantment(EquipmentSlot.Wrist, "Improved Hammer of Wrath");
    if (hasImprovedHammerOfWrath)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Improved Hammer of Wrath");
        Console.ResetColor();
    }

    bool hasPurifyingPower = HasEnchantment(EquipmentSlot.Wrist, "Purifying Power");
    if (hasPurifyingPower)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Purifying Power");
        Console.ResetColor();
    }

    bool hasLightsGrace = HasEnchantment(EquipmentSlot.Wrist, "Light's Grace");
    if (hasLightsGrace)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Light's Grace");
        Console.ResetColor();
    }

    // Legs
    bool hasExemplar = HasEnchantment(EquipmentSlot.Legs, "Inspiration Exemplar");
    if (hasExemplar)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Inspiration Exemplar");
        Console.ResetColor();
    }

    bool hasSacrifice = HasEnchantment(EquipmentSlot.Legs, "Divine Sacrifice");
    if (hasSacrifice)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Divine Sacrifice");
        Console.ResetColor();
    }

    bool hasShield = HasEnchantment(EquipmentSlot.Legs, "Avenger's Shield");
    if (hasShield)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Avenger's Shield");
        Console.ResetColor();
    }

    bool hasRebuke = HasEnchantment(EquipmentSlot.Legs, "Rebuke");
    if (hasRebuke)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Rebuke");
        Console.ResetColor();
    }

    bool hasAuraMastery = HasEnchantment(EquipmentSlot.Legs, "Aura Mastery");
    if (hasAuraMastery)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Aura Mastery");
        Console.ResetColor();
    }

    // Waist
    bool hasSheath = HasEnchantment(EquipmentSlot.Waist, "Sheath of Light");
    if (hasSheath)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Sheath of Light");
        Console.ResetColor();
    }

    bool hasInfusion = HasEnchantment(EquipmentSlot.Waist, "Infusion of Light");
    if (hasInfusion)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Infusion of Light");
        Console.ResetColor();
    }

    bool hasMalleableProtection = HasEnchantment(EquipmentSlot.Waist, "Malleable Protection");
    if (hasMalleableProtection)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Malleable Protection");
        Console.ResetColor();
    }

    // Feet
    bool hasArt = HasEnchantment(EquipmentSlot.Feet, "The Art of War");
    if (hasArt)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has The Art of War");
        Console.ResetColor();
    }

    bool hasSacred = HasEnchantment(EquipmentSlot.Feet, "Sacred Shield");
    if (hasSacred)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Sacred Shield");
        Console.ResetColor();
    }

    bool hasGuarded = HasEnchantment(EquipmentSlot.Feet, "Guarded by the Light");
    if (hasGuarded)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Guarded by the Light");
        Console.ResetColor();
    }

    // Chest
    bool hasStorm = HasEnchantment(EquipmentSlot.Chest, "Divine Storm");
    if (hasStorm)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Divine Storm");
        Console.ResetColor();
    }

    bool hasAegis = HasEnchantment(EquipmentSlot.Chest, "Aegis");
    if (hasAegis)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Aegis");
        Console.ResetColor();
    }

    bool hasHallowedGround = HasEnchantment(EquipmentSlot.Chest, "Hallowed Ground");
    if (hasHallowedGround)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Hallowed Ground");
        Console.ResetColor();
    }

    bool hasDivineLight = HasEnchantment(EquipmentSlot.Chest, "Divine Light");
    if (hasDivineLight)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Divine Light");
        Console.ResetColor();
    }

    // Back
    bool hasShieldOfRighteousness = HasEnchantment(EquipmentSlot.Back, "Shield of Righteousness");
    if (hasShieldOfRighteousness)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Shield of Righteousness");
        Console.ResetColor();
    }

    bool hasShockAndAwe = HasEnchantment(EquipmentSlot.Back, "Shock and Awe");
    if (hasShockAndAwe)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Shock and Awe");
        Console.ResetColor();
    }

    bool hasRighteousVengeance = HasEnchantment(EquipmentSlot.Back, "Righteous Vengeance");
    if (hasRighteousVengeance)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Righteous Vengeance");
        Console.ResetColor();
    }

    // Head
    bool hasFanaticism = HasEnchantment(EquipmentSlot.Head, "Fanaticism");
    if (hasFanaticism)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Fanaticism");
        Console.ResetColor();
    }

    bool hasImprovedSanctuary = HasEnchantment(EquipmentSlot.Head, "Improved Sanctuary");
    if (hasImprovedSanctuary)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Improved Sanctuary");
        Console.ResetColor();
    }

    bool hasWrath = HasEnchantment(EquipmentSlot.Head, "Wrath");
    if (hasWrath)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Has Wrath");
        Console.ResetColor();
    }
}
}
