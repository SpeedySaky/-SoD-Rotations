using System;
using System.Threading;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.Warcraft.Structures.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Spell;
using wShadow.Warcraft.Structures.Wow_Auras;


public class RetPala : Rotation
{
	
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;

	
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
var target = Api.Target;
    var pet = me.Pet();

if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
// Health percentage of the player
var healthPercentage = me.HealthPercent;

// Power percentages for different resources
		var mana = me.Mana;

// Target distance from the player
	var targetDistance = target.Position.Distance2D(me.Position);

if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		if (Api.Spellbook.CanCast("Frost Armor")  && !me.HasAura("Frost Armor"))
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Frost Armor");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Frost Armor"))
        return true;
	}
	
	if (Api.Spellbook.CanCast("Arcane Intellect")  && !me.HasPermanent("Arcane Intellect"))
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Arcane Intellect");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Arcane Intellect"))
        return true;
	}
	if (Api.Spellbook.CanCast("Conjure Water"))
{
    string[] waterTypes = new string[] { "Conjured Fresh Water", "Conjured Water" }; // Define water types from better to worse

    foreach (string waterType in waterTypes)
    {
        if (me.ItemCount(waterType) < 1)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting " + waterType);
            Console.ResetColor();

            if (Api.Spellbook.Cast("Conjure Water"))
            {
                return true;
            }
        }
        else
        {
            // Handle if already have the specified water type
            Console.WriteLine("Already have " + waterType + ". Skipping Conjure Water.");
            break; // Stop conjuring if already have a higher quality water
        }
    }
}

	
	if (Api.Spellbook.CanCast("Conjure Food")  && me.ItemCount("Conjured Muffin")<1)
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Conjure Food");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Conjure Food"))
        return true;
	}
	
	if (!target.IsDead())

if (Api.Spellbook.CanCast("Frostbolt")  &&  mana > 20)
  
    {
        var reaction = me.GetReaction(target);
        
        if (reaction != UnitReaction.Friendly)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Frostbolt");
            Console.ResetColor();
            
            if (Api.Spellbook.Cast("Frostbolt"))
            {
                return true;
            }
        }
        else
        {
            // Handle if the target is friendly
            Console.WriteLine("Target is friendly. Skipping Frostbolt cast.");
        }
    }
    else
    {
        // Handle if the target is not valid
        Console.WriteLine("Invalid target. Skipping Frostbolt cast.");
    }
				return base.PassivePulse();

		}
		
public override bool CombatPulse()
    {
	// Variables for player and target instances
var me = Api.Player;
var target = Api.Target;
 if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
// Health percentage of the player
var healthPercentage = me.HealthPercent;
var targethealth = target.HealthPercent;

// Power percentages for different resources
		var mana = me.Mana;

// Target distance from the player
	var targetDistance = target.Position.Distance2D(me.Position);

		if (me.IsDead() || me.IsGhost() || me.IsCasting()  ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		
		
	if (Api.Player.InCombat() && Api.Target != null && Api.Target.IsValid())
		{

    // Single Target Abilities
    if (!target.IsDead())
    {
		if (Api.Spellbook.CanCast("Fireblast") && mana>30 && targethealth<30 && !target.IsDead() && !Api.Spellbook.OnCooldown("Fireblast"))
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Fireblast");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Fireblast"))
        return true;
	}
        if (Api.Spellbook.CanCast("Fireball") && mana>30 && targethealth>20)
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Fireball");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Fireball"))
        return true;
	}
	
	if (Api.Spellbook.CanCast("Frostbolt")  && targethealth<20)
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Frostbolt");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Frostbolt"))
        return true;
	}
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

return base.CombatPulse();
}
private void LogPlayerStats()
    {
        // Variables for player and target instances
var me = Api.Player;
var target = Api.Target;
		var mana = me.Mana;

// Health percentage of the player
var healthPercentage = me.HealthPercent;


// Target distance from the player
		var targetDistance = target.Position.Distance2D(me.Position);
		

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
		Console.ResetColor();


	if (me.HasAura("Frost Armor")) // Replace "Thorns" with the actual aura name
	{
		 Console.ForegroundColor = ConsoleColor.Blue;
Console.ResetColor();
    var remainingTimeSeconds = me.AuraRemains("Frost Armor");
    var remainingTimeMinutes = remainingTimeSeconds / 60; // Convert seconds to minutes
    var roundedMinutes = Math.Round(remainingTimeMinutes/ 1000,1); // Round to one decimal place

    Console.WriteLine($"Remaining time for Frost Armor: {roundedMinutes} minutes");
	Console.ResetColor();
	}
	
	if (me.ItemCount("Conjured Water") > 1)
{
    var itemCount = me.ItemCount("Conjured Water"); // Replace "Conjured Water" with the name of your item
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine($"Number of Conjured Water items: {itemCount}");
    Console.ResetColor();
}

if (me.ItemCount("Conjured Muffin") > 1)
{
    var itemCount = me.ItemCount("Conjured Muffin"); // Replace "Conjured Water" with the name of your item
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine($"Number of Conjured Muffin items: {itemCount}");
    Console.ResetColor();
}


	if (me.HasPermanent("Devotion Aura")) // Replace "Thorns" with the actual aura name
	{
		 Console.ForegroundColor = ConsoleColor.Yellow;
Console.ResetColor();
   
		Console.WriteLine($"Have  Devotion Aura");
	}
if (me.HasAura("Blessing of Might")) // Replace "Thorns" with the actual aura name
{
		 Console.ForegroundColor = ConsoleColor.Blue;

   var remainingTimeSeconds = me.AuraRemains("Blessing of Might");
    var remainingTimeMinutes = remainingTimeSeconds / 60; // Convert seconds to minutes
    var roundedMinutes = Math.Round(remainingTimeMinutes/ 1000,1); // Round to one decimal place

    Console.WriteLine($"Remaining time for Blessing of Might: {roundedMinutes} minutes");
}

Console.ResetColor();
    }
	}