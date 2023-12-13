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


// Health percentage of the player
var healthPercentage = me.HealthPercent;

// Power percentages for different resources
var mana = me.GetPowerPercent(UnitPower.Mana); // Mana
var rage = me.GetPowerPercent(UnitPower.Rage); // Rage
var energy = me.GetPowerPercent(UnitPower.Energy); // Energy
var runicPower = me.GetPowerPercent(UnitPower.RunicPower); // Runic Power
// Runes count
var bloodRunes = me.GetRuneCount(RuneType.Blood);
var frostRunes = me.GetRuneCount(RuneType.Frost);
var unholyRunes = me.GetRuneCount(RuneType.Unholy);
var deathRunes = me.GetRuneCount(RuneType.Death);

// Target distance from the player
var targetDistance = me.Distance2D(target.Position); // Assuming Position returns a Vector3

if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
				return base.PassivePulse();

		}
		
public override bool CombatPulse()
    {
	// Variables for player and target instances
var me = Api.Player;
var target = Api.Target;

// Health percentage of the player
var healthPercentage = me.HealthPercent;

// Power percentages for different resources
var mana = me.GetPowerPercent(UnitPower.Mana); // Mana
var rage = me.GetPowerPercent(UnitPower.Rage); // Rage
var energy = me.GetPowerPercent(UnitPower.Energy); // Energy
var runicPower = me.GetPowerPercent(UnitPower.RunicPower); // Runic Power

// Runes count
var bloodRunes = me.GetRuneCount(RuneType.Blood);
var frostRunes = me.GetRuneCount(RuneType.Frost);
var unholyRunes = me.GetRuneCount(RuneType.Unholy);
var deathRunes = me.GetRuneCount(RuneType.Death);

// Target distance from the player
var targetDistance = me.Distance2D(target.Position); // Assuming Position returns a Vector3

		if (me.IsDead() || me.IsGhost() || me.IsCasting()  ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		
		
	if (Api.Player.InCombat() && Api.Target != null && Api.Target.IsValid())
{
    var me = Api.Player;
    var target = Api.Target;

    // Single Target Abilities
    if (target.IsAlive())
    {
        // Logic for single-target abilities, e.g. damage spells, buffs, etc.
        // Example: if (me.CanCast("Spell") && target.HealthPercent < 50)
        // {
        //     me.Cast("Spell");
        // }
    }
}


// Check if in combat and if there are multiple targets nearby
if (Api.Player.InCombat() && Api.Player.HostilesNearby(10, true, true) >= 2)
{
    var me = Api.Player;
    var targets = Api.Player.GetAllNearbyEnemies(10);

    // Multi-Target Abilities
    foreach (var target in targets)
    {
        if (target.IsAlive())
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
    }	return base.CombatPulse();
    }

private void LogPlayerStats()
    {
        // Variables for player and target instances
var me = Api.Player;
var target = Api.Target;

// Health percentage of the player
var healthPercentage = me.HealthPercent;

// Power percentages for different resources
var mana = me.GetPowerPercent(UnitPower.Mana); // Mana
var rage = me.GetPowerPercent(UnitPower.Rage); // Rage
var energy = me.GetPowerPercent(UnitPower.Energy); // Energy
var runicPower = me.GetPowerPercent(UnitPower.RunicPower); // Runic Power

// Runes count
var bloodRunes = me.GetRuneCount(RuneType.Blood);
var frostRunes = me.GetRuneCount(RuneType.Frost);
var unholyRunes = me.GetRuneCount(RuneType.Unholy);
var deathRunes = me.GetRuneCount(RuneType.Death);

// Target distance from the player
var targetDistance = me.Distance2D(target.Position); // Assuming Position returns a Vector3
		

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
		Console.ResetColor();


	if (me.HasPassive("Frost Armor")) // Replace "Thorns" with the actual aura name
	{
		 Console.ForegroundColor = ConsoleColor.Blue;
Console.ResetColor();
    var remainingTimeSeconds = me.AuraRemains("Mark of the Wild");
    var remainingTimeMinutes = remainingTimeSeconds / 60; // Convert seconds to minutes
    var roundedMinutes = Math.Round(remainingTimeMinutes/ 1000,1); // Round to one decimal place

    Console.WriteLine($"Remaining time for Frost Armor: {roundedMinutes} minutes");
	Console.ResetColor();
	}
	
	if (me.HasAura("Frost Armor")) // Replace "Thorns" with the actual aura name
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.ResetColor();
    Console.WriteLine($"Have aura Seal of Righteousness");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.ResetColor();
    Console.WriteLine($"No Seal of Righteousness aura");
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