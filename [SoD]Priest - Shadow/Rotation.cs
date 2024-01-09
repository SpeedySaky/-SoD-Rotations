using System;
using System.Threading;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;


public class Warrior : Rotation
{
	
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
	private DateTime lastHands = DateTime.MinValue;
    private TimeSpan Hands = TimeSpan.FromSeconds(14);
		private DateTime lastPants = DateTime.MinValue;
    private TimeSpan Pants = TimeSpan.FromSeconds(60);



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
var mana = me.ManaPercent;

if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
// Health percentage of the player
var healthPercentage = me.HealthPercent;

// Power percentages for different resources

// Target distance from the player

if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() || me.IsLooting() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;

		if (Api.Spellbook.CanCast("Renew") && !me.HasAura("Renew") && healthPercentage<80) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Renew");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Renew"))
    {
        return true;
    } 
	}
	if (Api.Spellbook.CanCast("Power Word: Fortitude") && !me.HasAura("Power Word: Fortitude") ) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Power Word: Fortitude");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Power Word: Fortitude"))
    {
        return true;
    } 
	}
	if (Api.Spellbook.CanCast("Inner Fire") && !me.HasAura("Inner Fire") ) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Inner Fire");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Inner Fire"))
    {
        return true;
    } 
	} 
	if (Api.Spellbook.CanCast("Power Word: Shield") && !me.HasAura("Power Word: Shield") ) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Power Word: Shield");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Power Word: Shield"))
    {
        return true;
    } 
	} 	
		if (Api.Spellbook.CanCast("Mind Blast")   )
{
	 {
        var reaction = me.GetReaction(target);
        
        if (reaction != UnitReaction.Friendly)
			{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Mind Blast");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Mind Blast"))
   
        return true;
    }
 }
	 }	return base.PassivePulse();
		}
		
		
	public override bool CombatPulse()
    {
		if ( me.IsCasting() || me.IsChanneling() || me.IsLooting() ) return false;

        var me = Api.Player;
var target = Api.Target;
var mana = me.ManaPercent;

 if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
// Health percentage of the player
var healthPercentage = me.HealthPercent;
var targethealth = target.HealthPercent;


// Target distance from the player
	var targetDistance = target.Position.Distance2D(me.Position);
	if (Api.Spellbook.CanCast("Power Word: Shield") && !me.HasAura("Power Word: Shield") && mana>15) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Power Word: Shield");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Power Word: Shield"))
    {
        return true;
    } 
	} 
if ( Api.HasMacro("Hands"))
        {
            if ((DateTime.Now - lastHands) >= Hands)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Hands rune");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    lastHands= DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Hands rune is on cooldown. Skipping cast.");
            }
        }
		
		if ( Api.HasMacro("Legs"))
        {
            if ((DateTime.Now - lastPants) >= Pants)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Legs rune");
                Console.ResetColor();

                if (Api.UseMacro("Legs"))
                {
                    lastHands= DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Hands rune is on cooldown. Skipping cast.");
            }
        }
if (Api.Spellbook.CanCast("Shadow Word: Pain") && !target.HasAura("Shadow Word: Pain") && targethealth>=30&& mana>10) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Shadow Word: Pain");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Shadow Word: Pain"))
    {
        return true;
    } 
	}
if (Api.Spellbook.CanCast("Mind Blast") && targethealth>=30&& mana>10) 
			{
              Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Mind Blast");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Mind Blast"))
    {
        return true;
    } 
	}

	
	if (Api.HasMacro("Shoot"))
  
    
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Shoot");
            Console.ResetColor();
            
            if (Api.UseMacro("Shoot"))
            {
                return true;
            }
        }
		
		
			
		return base.CombatPulse();
    }
	
	
	
	private void LogPlayerStats()
    {
        var me = Api.Player;

		var mana = me.Mana;
        var healthPercentage = me.HealthPercent;
		

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
		Console.ResetColor();



    }
	
}