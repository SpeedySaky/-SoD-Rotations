using System;
using System.Threading;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.Warcraft.Structures.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Spell;



public class Druid : Rotation
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
			

		 var me = Api.Player;
		var healthPercentage = me.HealthPercent;
		var mana = me.Mana;

			if (me.IsDead() || me.IsGhost() || me.IsCasting()|| me.IsMoving() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }


if (Api.Spellbook.CanCast("Mark of the Wild") && !me.HasAura("Mark of the Wild")  )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Mark of the Wild");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Mark of the Wild"))
   
        return true;
    }
	if (Api.Spellbook.CanCast("Thorns") && !me.HasAura("Thorns")  )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Thorns");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Thorns"))
   
        return true;
    }

	if (Api.Spellbook.CanCast("Omen of Clarity") && !me.HasAura("Omen of Clarity")  )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Omen of Clarity");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Omen of Clarity"))
   
        return true;
    }
	if (!me.HasPermanent("Cat Form") && Api.Spellbook.CanCast("Cat Form") )	
		{	
		if (Api.Spellbook.CanCast("Cat Form") && !me.HasPermanent("Cat Form") )
				{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Cat Form");
			Console.ResetColor();
		if (Api.Spellbook.Cast("Cat Form"))
				{
					return true;
				}
				}
				
			}
			 var target = Api.Target;

			if (!target.IsDead())

if (Api.Spellbook.CanCast("Wrath") && healthPercentage > 50 && !me.HasPermanent("Cat Form"))
  
    {
        var reaction = me.GetReaction(target);
        
        if (reaction != UnitReaction.Friendly)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Wrath");
            Console.ResetColor();
            
            if (Api.Spellbook.Cast("Wrath"))
            {
                return true;
            }
        }
        else
        {
            // Handle if the target is friendly
            Console.WriteLine("Target is friendly. Skipping Wrath cast.");
        }
    }
    else
    {
        // Handle if the target is not valid
        Console.WriteLine("Invalid target. Skipping Wrath cast.");
    }

 return base.PassivePulse();
}				
		
	public override bool CombatPulse()
    {
				

        var me = Api.Player;
		var healthPercentage = me.HealthPercent;
		var mana = me.Mana;
		 var target = Api.Target;
		var targethealth = target.HealthPercent;
		var energy = me.Energy;
		var points = me.ComboPoints;

		if (Api.Spellbook.CanCast("Rejuvenation") &&!me.HasAura("Rejuvenation") && healthPercentage <= 70 && mana > 15)
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Rejuvenation");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Rejuvenation"))
			{
				return true;
			}
		}
		
		if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana > 25 && me.HasAura("Fury of Stormrage"))
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Healing Touch");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Healing Touch"))
			{
				return true;
			}
       }
	   if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 45 && mana > 25 )
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Healing Touch");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Healing Touch"))
			{
				return true;
			}
       }
		if (!target.HasAura("Sunfire") && targethealth>30 && Api.HasMacro("Sunfire") )
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Sunfire");
            Console.ResetColor();
            if (Api.UseMacro("Sunfire"))
            {
                return true;
            }
        }
		if (Api.Spellbook.CanCast("Moonfire") && !target.HasAura("Moonfire") && targethealth>30 )
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Moonfire");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Moonfire"))
			{
				return true;
			}
		}

		

       if (Api.Spellbook.CanCast("Wrath") )
	   {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Wrath");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Wrath"))
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
        Console.WriteLine($"{mana}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
		Console.ResetColor();
Console.ResetColor();

if (me.HasAura("Fury of Stormrage"))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Fury of Stormrage");
			Console.ResetColor();
    }
	
}
}