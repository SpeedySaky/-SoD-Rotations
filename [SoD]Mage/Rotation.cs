using System;
using System.Threading;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;



public class MageSoD : Rotation
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
var pet = me.Pet();

if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
// Health percentage of the player
var healthPercentage = me.HealthPercent;
var mana = me.ManaPercent;
var targetDistance = target.Position.Distance2D(me.Position);
ShadowApi shadowApi = new ShadowApi();

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
	
	
	string[] waterTypes = { "Conjured Fresh Water", "Conjured Water", "Conjured Purified Water" };
bool needsWater = true;

foreach (string waterType in waterTypes)
{
    if (shadowApi.Inventory.HasItem(waterType))
    {
        needsWater = false;
        break;
    }
}

// Now needsWater variable will indicate if the character needs water
if (needsWater)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Character needs water!");
    Console.ResetColor();

    // Add logic here to conjure water or perform any action needed to acquire water
    // Example: Cast "Conjure Water" spell
    // Assuming the API allows for conjuring water in a similar way to casting spells
    if (Api.Spellbook.CanCast("Conjure Water"))
    {
        if (Api.Spellbook.Cast("Conjure Water"))
        {
            Console.WriteLine("Conjured water.");
            // Add further actions if needed after conjuring water
        }
 }
}
string[] foodTypes = { "Conjured Muffin", "Conjured Bread", "Conjured Rye" };
bool needsFood = true;

foreach (string foodType in foodTypes)
{
    if (shadowApi.Inventory.HasItem(foodType))
    {
        needsFood = false;
        break;
    }
}

// Now needsWater variable will indicate if the character needs food
if (needsFood)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Character needs food!");
    Console.ResetColor();

    // Add logic here to conjure water or perform any action needed to acquire food
    // Example: Cast "Conjure food" spell
    // Assuming the API allows for conjuring food in a similar way to casting spells
    if (Api.Spellbook.CanCast("Conjure Food"))
    {
        if (Api.Spellbook.Cast("Conjure Food"))
        {
            Console.WriteLine("Conjured Food.");
            // Add further actions if needed after conjuring water
        }

}
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
		if (Api.Spellbook.CanCast("Fireblast") && mana>30 && targethealth<50  && !Api.Spellbook.OnCooldown("Fireblast"))
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
	
	if (Api.Spellbook.CanCast("Frostbolt")  && targethealth<20 && mana>20)
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Frostbolt");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Frostbolt"))
        return true;
	}
	
	if (Api.Spellbook.CanCast("Shoot")   )
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Shoot");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Shoot"))
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
		var mana = me.ManaPercent;
ShadowApi shadowApi = new ShadowApi();

// Health percentage of the player
var healthPercentage = me.HealthPercent;


// Target distance from the player
		var targetDistance = target.Position.Distance2D(me.Position);
		

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
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
	


// Define food and water types
string[] foodTypes = { "Conjured Muffin", "Conjured Bread", "Conjured Rye" };
string[] waterTypes = { "Conjured Fresh Water", "Conjured Water", "Conjured Purified Water" };

// Count food items in the inventory
int foodCount = 0;
foreach (string foodType in foodTypes)
{
    int count = shadowApi.Inventory.ItemCount(foodType);
    foodCount += count;
}

// Count water items in the inventory
int waterCount = 0;
foreach (string waterType in waterTypes)
{
    int count = shadowApi.Inventory.ItemCount(waterType);
    waterCount += count;
}

// Display the counts of food and water items
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Current Food Count: " + foodCount);
Console.WriteLine("Current Water Count: " + waterCount);
Console.ResetColor();



Console.ResetColor();
    }
	}