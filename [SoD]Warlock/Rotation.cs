using System;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;

public class Warlock : Rotation
{
	
	private bool HasItem(object item)
        => Api.Inventory.HasItem(item);
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
	private TimeSpan HauntCooldown = TimeSpan.FromSeconds(14);
	private DateTime lastHaunt = DateTime.MinValue;
	public bool IsValid(WowUnit unit)
	{
		if (unit == null || unit.Address == null)
		{
			return false;
		}
		return true;
	}
	
    public override void Initialize()
    {  
	// Can set min/max levels required for this rotation.
        
		 lastDebugTime = DateTime.Now;
        LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

		// Assuming wShadow is an instance of some class containing UnitRatings property
        SlowTick = 800;
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
		var mana = me.ManaPercent;
var pet = me.Pet();
	  var PetHealth  = 0.0f;
	     if(IsValid(pet))
		 {
		   PetHealth = pet.HealthPercent;
		 }  
		 var TargetHealth  = 0.0f;
	     if(IsValid(target))
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

if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsChanneling() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		if (Api.Spellbook.CanCast("Demon Skin")  && !me.HasAura("Demon Skin"))
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Demon Skin");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Demon Skin"))
        return true;
	}
	
		//if (Api.HasMacro("Hands") && reaction != UnitReaction.Friendly && targethealth>=1)

var petFamily = Api.Pet.Info.UnitFamily;
var isImp = petFamily == CreatureFamily.Imp; //|| petFamily == CreatureFamily.Felimp;
var isVoid = petFamily == CreatureFamily.Voidwalker; //|| petFamily == CreatureFamily.Voidlord;
var isGuard = petFamily == CreatureFamily.Felguard;
var isHunter = petFamily == CreatureFamily.Felhunter;
var isSuccubus = petFamily == CreatureFamily.Succubus;
//|| !isVoid
    
	if (!IsValid(pet)  && Api.Spellbook.CanCast("Summon Voidwalker") && HasItem("Soul Shard") && mana > 25 && Api.Spellbook.HasSpell("Summon Voidwalker"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Summon Voidwalker.");
        Console.ResetColor();

        if (Api.Spellbook.Cast("Summon Voidwalker"))
        {
            return true;
        }
    }
    else if (!IsValid(pet) && Api.Spellbook.CanCast("Summon Imp")  && mana > 30)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Summon Imp.");
        Console.ResetColor();

        if (Api.Spellbook.Cast("Summon Imp"))
        {
            return true;
        }
    }
	
	
	if (PetHealth<50 && healthPercentage>50 && Api.Spellbook.CanCast("Health Funnel"))
		{
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Healing Pet ");
            Console.ResetColor();

            if (Api.Spellbook.Cast("Health Funnel"))
                return true;
        }

		
	if (Api.Spellbook.CanCast("Life Tap") && healthPercentage>80 && mana<30) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Life Tap");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Life Tap"))
    {
        return true;
    } 
	}
		
//string[] healthstoneTypes = { "Minor Healthstone", "Lesser Healthstone", "Healthstone", "Greater Healthstone", "Major Healthstone", "Master Healthstone", "Demonic Healthstone", "Fel Healthstone" };



	
		var reaction = me.GetReaction(target);
	if (Api.HasMacro("Hands") && reaction != UnitReaction.Friendly && targethealth>=1)
  
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
ShadowApi shadowApi = new ShadowApi();
 if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
}
// Target distance from the player
	var targetDistance = target.Position.Distance2D(me.Position);

		if (me.IsDead() || me.IsGhost() || me.IsCasting()  || me.IsChanneling()) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
	var pet = me.Pet();
	  var PetHealth  = 0.0f;
	     if(IsValid(pet))
		 {
		   PetHealth = pet.HealthPercent;
		 }        	
		var meTarget = me.Target;
		  
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
		
		if (Api.Spellbook.CanCast("Drain Soul") && shadowApi.Inventory.ItemCount("Soul Shard") <= 2 && targethealth <= 30 && mana>10)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Drain Soul");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Drain Soul"))
    {
        return true;
    }
}
    	if (Api.Player.InCombat() && Api.Target != null && Api.Target.IsValid())
		{

    // Single Target Abilities
    if (!target.IsDead())
    {
		if (!IsValid(pet)  && Api.Spellbook.CanCast("Summon Voidwalker") && shadowApi.Inventory.HasItem("Soul Shard") && mana >=30)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Summon VoidWalker.");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Summon Voidwalker"))
    {
        return true;
    }
}
else if (!IsValid(pet) && Api.Spellbook.CanCast("Summon Imp") && !Api.Spellbook.CanCast("Summon Voidwalker") && mana >=30)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Summon Imp.");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Summon Imp"))
    {
        return true;
    }
}




		
		if (Api.Spellbook.CanCast("Drain Life") && healthPercentage<=50 && mana>=5 )
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Drain Life");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Drain Life"))
        return true;
	}
		
if (Api.Spellbook.CanCast("Curse of Agony") && !target.HasAura("Curse of Agony") && mana>=10 && targethealth>=30)
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Curse of Agony");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Curse of Agony"))
        return true;
	}
	
	if (Api.Spellbook.CanCast("Corruption") && !target.HasAura("Corruption") && mana>=10&& targethealth>=30 )
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Corruption");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Corruption"))
        return true;
	}
	
	
	
	if (Api.Spellbook.CanCast("Shadow Bolt") && mana>=10 && targethealth>=15 )
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Shadow Bolt");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Shadow Bolt"))
        return true;
	}
	if (!target.HasAura("Haunt") && Api.HasMacro("Hands") && targethealth>=30 && mana>=12)
  {
            if ((DateTime.Now - lastHaunt) >=HauntCooldown )
    {
        var reaction = me.GetReaction(target);
        
        if (reaction != UnitReaction.Friendly)
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
	if (Api.Spellbook.CanCast("Shoot")  )
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Shoot");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Shoot"))
        return true;
	}
    }
	

// Check if in combat and if there are multiple targets nearby
if (me.InCombat() && Api.UnfriendlyUnitsNearby(10, true) >= 2)
{
    
    // Multi-Target Abilities
    
    if (!target.IsDead())
        {
            // Logic for multi-target abilities, e.g. AoE spells, debuffs, etc.
            // Example: if (me.CanCast("AoE_Spell") && target.Distance < 8)
            // {
            //     me.Cast("AoE_Spell");
            // }
        }
    
   }
}
return base.CombatPulse();
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
		var petFamily = Api.Pet.Info.UnitFamily;

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

	
	
ShadowApi shadowApi = new ShadowApi();
// Access the Bank property and use its methods
if (HasItem("Hearthstone"))
{
    Console.WriteLine("Hearthstone found in inventory");
    Console.ResetColor();
}

if (HasItem("Lesser Healthstone"))
{
    Console.WriteLine("Lesser Healthstone found in inventory");
    Console.ResetColor();
}

if (Api.Spellbook.HasSpell(6202))
{
    Console.WriteLine("Can Cast Create Healthstone (Lesser)");
    Console.ResetColor();
}

    }
	}