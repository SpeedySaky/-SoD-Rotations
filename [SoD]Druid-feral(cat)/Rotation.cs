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


if (Api.Spellbook.CanCast("Mark of the Wild") && !me.HasAura("Mark of the Wild") && (!me.HasPermanent("Cat Form")|| me.HasPermanent("Cat Form")) )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Mark of the Wild");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Mark of the Wild"))
   
        return true;
    }
	
	if (Api.Spellbook.CanCast("Thorns") && !me.HasAura("Thorns") && (!me.HasPermanent("Cat Form")|| me.HasPermanent("Cat Form")) )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Thorns");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Thorns"))
   
        return true;
    }

	if (Api.Spellbook.CanCast("Omen of Clarity") && !me.HasAura("Omen of Clarity") && (!me.HasPermanent("Cat Form")|| me.HasPermanent("Cat Form")) )
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
		
		 if (Api.Spellbook.CanCast("War Stomp") &&   !Api.Spellbook.OnCooldown("War Stomp") && healthPercentage<=30 )
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting War Stomp");
			Console.ResetColor();
			if (Api.Spellbook.Cast("War Stomp"))
			{
				return true;
			}
       }
		if (Api.Spellbook.CanCast("Rejuvenation") &&!me.HasAura("Rejuvenation") && healthPercentage <= 30 && mana > 15)
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Rejuvenation");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Rejuvenation"))
			{
				return true;
			}
		}
		
		if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <= 30 && mana > 25 && me.HasAura("Fury of Stormrage"))
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Healing Touch");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Healing Touch"))
			{
				return true;
			}
       }
	   if (Api.Spellbook.CanCast("Healing Touch") && healthPercentage <=30 && mana > 25 )
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Healing Touch");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Healing Touch"))
			{
				return true;
			}
       }
	   if (Api.Spellbook.CanCast("War Stomp") &&   !Api.Spellbook.OnCooldown("War Stomp") )
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting War Stomp");
			Console.ResetColor();
			if (Api.Spellbook.Cast("War Stomp"))
			{
				return true;
			}
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
			
			if (Api.Spellbook.CanCast("Tiger's Fury") &&!me.HasAura("Tiger's Fury") &&   !Api.Spellbook.OnCooldown("Tiger's Fury"))
        {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Casting Tiger's Fury");
			Console.ResetColor();
			if (Api.Spellbook.Cast("Tiger's Fury"))
			{
				return true;
			}
		}
		if (Api.HasMacro("Roar") && points >=1 && energy >= 25 && !me.HasAura("Savage Roar") && me.HasPermanent("Cat Form"))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Casting Savage Roar");
			Console.ResetColor();

		if (Api.UseMacro("Roar"))
				return true;
		}
		if (Api.HasMacro("Mangle") && points < 3 && energy >= 45 && me.HasPermanent("Cat Form"))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Casting Mangle (Cat) with {energy} Energy");
			Console.ResetColor();

		if (Api.UseMacro("Mangle"))
				return true;
		}

		if (Api.Spellbook.CanCast("Claw") && points < 3 && energy >= 45 && me.HasPermanent("Cat Form"))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Casting Claw (Cat) with {energy} Energy");
			Console.ResetColor();

		if (Api.Spellbook.Cast("Claw"))
			return true;
		}

		if (Api.Spellbook.CanCast("Rip") && !target.HasAura("Rip") && target.HealthPercent >= 20 && energy > 30 && points >= 3 && me.HasPermanent("Cat Form"))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Casting Rip with {points} Points and {energy} Energy");
			Console.ResetColor();

		if (Api.Spellbook.Cast("Rip"))
				return true;
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