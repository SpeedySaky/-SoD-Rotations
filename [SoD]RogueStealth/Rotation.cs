using System;
using System.Threading;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.Warcraft.Structures.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Spell;


public class Rogue : Rotation
{
	
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
    private int pickPocketDelay = 3000; // Delay in milliseconds (3 seconds)
	private DateTime lastQuickdraw = DateTime.MinValue;
    private TimeSpan QuickdrawCooldown = TimeSpan.FromSeconds(11);
	private DateTime lastBetween = DateTime.MinValue;
    private TimeSpan BetweenCooldown = TimeSpan.FromSeconds(10);
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
var energy = me.Energy; // Energy
		var points =  me.ComboPoints;

// Target distance from the player
	var targetDistance = target.Position.Distance2D(me.Position);

if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsLooting() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		
		if (Api.Spellbook.CanCast("Sprint") && !Api.Spellbook.OnCooldown("Sprint") )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Sprint");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Sprint"))
    {
        return true;
    }
}
		if (!me.HasPermanent("Stealth") && Api.Spellbook.CanCast("Stealth") && !Api.Spellbook.OnCooldown("Stealth") && Api.HasTarget())
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Stealth");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Stealth"))
        
            return true;
        
    }
 if (!target.IsDead() && targetDistance <= 5)
        {
            if (Api.Spellbook.CanCast("Pick Pocket"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Pick Pocket");
                Console.ResetColor();

                if (Api.Spellbook.Cast("Pick Pocket"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Pick Pocket successful! Waiting before Shadowstrike.");
                    Console.ResetColor();

                    // Introducing a delay after Pick Pocket
                    Thread.Sleep(pickPocketDelay);

                    // Attempting Backstab after delay
                    if (!target.IsDead() && targetDistance<=20 && Api.HasMacro("Hands"))
					{
      
					Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Casting Shadowstrike");
					Console.ResetColor();

					if (Api.UseMacro("Hands"))
					return true;
	
					}

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
 if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
// Health percentage of the player
var healthPercentage = me.HealthPercent;
var targethealth = target.HealthPercent;
var energy = me.Energy; // Energy
		var points =  me.ComboPoints;


// Target distance from the player
	var targetDistance = target.Position.Distance2D(me.Position);

		if (me.IsDead() || me.IsGhost() || me.IsCasting()  ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		if (Api.Spellbook.CanCast("Kick") && !Api.Spellbook.OnCooldown("Kick") && (target.IsCasting() || target.IsChanneling()))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Kick");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Kick"))
    {
        return true;
    }
}
if (Api.HasMacro("Chest") && energy>=20)
        {
            if ((DateTime.Now - lastQuickdraw) >= QuickdrawCooldown)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Chest Rune");
                Console.ResetColor();

                if (Api.UseMacro("Chest"))
                {
                    lastQuickdraw = DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Chest rune is on cooldown. Skipping cast.");
            }
        }
	
if (Api.HasMacro("Legs") && energy>=35)
        {
            if ((DateTime.Now - lastBetween) >= BetweenCooldown)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Legs Rune");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    lastBetween = DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Legs rune is on cooldown. Skipping cast.");
            }
        }	
		if (Api.Spellbook.CanCast("Eviscerate")  && points >= 5 )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Eviscerate ");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Eviscerate"))
        return true;
}
		if (Api.Spellbook.CanCast("Sinister Strike")  && energy >= 45 )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Sinister Strike ");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Sinister Strike"))
        return true;
}
		
	



return base.CombatPulse();
}
private void LogPlayerStats()
    {
        // Variables for player and target instances
var me = Api.Player;
var target = Api.Target;

// Health percentage of the player
var healthPercentage = me.HealthPercent;

var energy = me.Energy; // Energy
		var points =  me.ComboPoints;

// Target distance from the player
		var targetDistance = target.Position.Distance2D(me.Position);


        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{energy}% Energy available");
        Console.WriteLine($"{healthPercentage}% Health available");
		Console.WriteLine($"{points} points available");

		Console.ResetColor();


	
	

Console.ResetColor();
    }
	}