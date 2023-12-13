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


public class Shaman : Rotation
{
	
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
	 private DateTime lastRockbiterTime = DateTime.MinValue;    public override void Initialize()
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
		var mana = me.Mana;


         if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
// Health percentage of the player
var healthPercentage = me.HealthPercent;

// Target distance from the player
	var targetDistance = target.Position.Distance2D(me.Position);

if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMoving() || me.IsChanneling() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		 if ((DateTime.Now - lastRockbiterTime).TotalMinutes >= 5)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Rockbiter Weapon");
            Console.ResetColor();

            if (Api.Spellbook.CanCast("Rockbiter Weapon"))
            {
                if (Api.Spellbook.Cast("Rockbiter Weapon"))
                {
                    lastRockbiterTime = DateTime.Now; // Update lastRockbiterTime
                    return true;
                }
            }
        }

		if (Api.Spellbook.CanCast("Healing Wave") && healthPercentage<=50  && mana>45)
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Healing Wave");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Healing Wave"))
        return true;
	}
	

				return base.PassivePulse();

		}
		
public override bool CombatPulse()
    {
	// Variables for player and target instances
var me = Api.Player;
var target = Api.Target;
		var mana = me.Mana;

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

		if (me.IsDead() || me.IsGhost() || me.IsCasting()  ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		
		
	if (Api.Player.InCombat() && Api.Target != null && Api.Target.IsValid())
		{

    // Single Target Abilities
    if (!target.IsDead())
    {
		if (Api.Spellbook.CanCast("Healing Wave") && healthPercentage<=30 && mana>=45  )
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Healing Wave");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Healing Wave"))
        return true;
	}
	if (Api.Spellbook.CanCast("Earth Shock") && !Api.Spellbook.OnCooldown("Earth Shock") && (target.IsCasting() || target.IsChanneling()))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Earth Shock");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Earth Shock"))
    
        return true;
    
}
	if (Api.Spellbook.CanCast("Lightning Bolt") && mana>20 )
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Lightning Bolt");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Lightning Bolt"))
        return true;
	}
	
	
    }
	}

// Check if in combat and if there are multiple targets nearby
if (me.InCombat() && Api.EnemiesNearby(10, true, true) >= 2)
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

// Power percentages for different resources
// Target distance from the player
		var targetDistance = target.Position.Distance2D(me.Position);
		

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{mana} Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
		Console.ResetColor();
		
		
if (me.HasItem("Minor Healing Potion"))
{
    if (Api.Inventory.OnCooldown("Minor Healing Potion"))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Have Minor Healing Potion but it's on cooldown");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Have Minor Healing Potion and it's available");
        Console.ResetColor();
    }
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Don't have Minor Healing Potion");
    Console.ResetColor();
}
	
	


    }
	}