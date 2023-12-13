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



public class EnhaShaman : Rotation
{
	
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
	 private DateTime lastRockbiterTime = DateTime.MinValue;   

	
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
		var healthPercentage = me.HealthPercent;
		var mana = me.Mana;

		
		if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }



 if ((DateTime.Now - lastRockbiterTime).TotalMinutes >= 2)
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



if (!me.HasAura("Lightning Shield") && mana > 30 )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Lighting Shield");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Lightning Shield"))
    {
        return true;
    }
}
 return base.PassivePulse();
}
		
		
		
	public override bool CombatPulse()
    {
        var me = Api.Player;
		var healthPercentage = me.HealthPercent;
		var mana = me.Mana;
		 var target = Api.Target;

		
		Console.WriteLine("combat");
		
		if (Api.Spellbook.CanCast("Healing Wave") && healthPercentage <= 50 && mana > 20)
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Healing Wave");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Healing Wave"))
			{
				return true;
			}
       }
	
	
		if (Api.Spellbook.CanCast("Flame Shock") && !Api.Spellbook.OnCooldown("Flame Shock") && !target.HasAura("Flame Shock"))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Flame Shock");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Flame Shock"))
			{
				return true;
			}
		}

       if ( mana > 30)
	   {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Lava Lash`");
			Console.ResetColor();
			if (Api.UseMacro("Lash"))
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
Console.ResetColor();
    }
}