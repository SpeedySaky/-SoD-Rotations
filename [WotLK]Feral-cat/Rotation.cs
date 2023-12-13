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



public class CatDruid : Rotation
{
	
    private int debugInterval = 30; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;

	
    public override void Initialize()
    {
        // Can set min/max levels required for this rotation.
        
		lastDebugTime = DateTime.Now;
		 LogPlayerStats();
        // Use this method to set your tick speeds.
        // The simplest calculation for optimal ticks (to avoid key spam and false attempts)

		// Assuming wShadow is an instance of some class containing UnitRatings property
		var haste = UnitRating.HasteRanged;
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
		var health = me.HealthPercent;
        if (me.HasPermanent("Swift Flight Form") | me.HasPermanent("Flight Form") | me.HasPermanent("Travel Form")) return false;

		if (me.IsDead() || me.IsGhost() || me.IsCasting() || me.IsMounted()) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
		if (Api.Spellbook.CanCast("Thorns"))
			{
				if (!me.HasAura("Thorns"))
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Casting Thorns");
						Console.ResetColor();

					if (Api.Spellbook.Cast("Thorns"))
						{
					return true;
						}
					}
			}

		if (Api.Spellbook.CanCast("Gift of the Wild") && me.HasItem("Wild Spineleaf"))
		{
			if (!me.HasAura("Gift of the Wild"))
		{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Casting Gift of the Wild");
				Console.ResetColor();
			if (Api.Spellbook.Cast("Gift of the Wild"))
			{
            return true;
			}
		}
	}
		else if (Api.Spellbook.CanCast("Mark of the Wild") && !me.HasItem("Wild Spineleaf"))
		{
			if (!me.HasAura("Mark of the Wild"))
		{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Casting Mark of the Wild");
				Console.ResetColor();

			if (Api.Spellbook.Cast("Mark of the Wild"))
			{
            return true;
			}
		}
	}
	if (Api.Spellbook.CanCast("Rejuvenation") && health <= 60 && !me.HasAura("Rejuvenation"))
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Rejuvenation");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Rejuvenation"))
        return true;
	}
	
	if (Api.Spellbook.CanCast("Regrowth") && health <= 40 && !me.HasAura("Regrowth"))
	{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Regrowth");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Regrowth"))
        return true;
	}
	if (Api.Spellbook.CanCast("Healing Touch") && health <= 30)
        {
			Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Healing Touch" );
			Console.ResetColor();

            if (Api.Spellbook.Cast("Healing Touch"))
                return true;
        }
		
	
if (Api.Spellbook.CanCast("Cat Form") && !me.HasPermanent("Cat Form"))	
{
			Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Cat Form" );
			Console.ResetColor();

            if (Api.Spellbook.Cast("Cat Form"))
                return true;
        }
				
			

        return base.PassivePulse();
    }
	
	public override bool CombatPulse()
    {
		 var me = Api.Player;
        if (!me.IsValid() || me.IsDeadOrGhost()) return false;
		 var target = Api.Target;
		 if (!target.IsValid() || target.IsDeadOrGhost()) return false;
		 var Energy =  me.GetPowerPercent(UnitPower.Energy);
		var Points =  me.GetPowerPercent(UnitPower.ComboPoints)/20;
		var manaPercentage = me.GetPowerPercent(UnitPower.Mana);
        var healthPercentage = me.HealthPercent;

if (!me.HasAura("Innervate") && Api.Spellbook.CanCast("Innervate") && manaPercentage <= 30)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Innervate");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Innervate"))
    {
        return true;
    }
}
if (healthPercentage<=30 && !Api.Spellbook.OnCooldown("Survival Instincts"))
	{
			Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Survival Instincts as we geting low HP" );
			Console.ResetColor();

            if (Api.Spellbook.Cast("Survival Instincts"))
                return true;
        }
if (healthPercentage<=30 && !Api.Spellbook.OnCooldown("Barkskin"))
	{
			Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Barkskin as we geting low HP" );
			Console.ResetColor();

            if (Api.Spellbook.Cast("Barkskin"))
                return true;
        }
		
		if (Api.Spellbook.CanCast("Regrowth") && !me.HasAura("Regrowth") && me.HasAura("Barkskin"))
		{
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Regrowth");
        Console.ResetColor();

			if (Api.Spellbook.Cast("Regrowth"))
			{
            return true;
			}
		}

    if (Api.Spellbook.CanCast("Rejuvenation") && !me.HasAura("Rejuvenation") && me.HasAura("Barkskin"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Rejuvenation");
        Console.ResetColor();

        if (Api.Spellbook.Cast("Rejuvenation"))
        {
            return true;
        }
    }
	if (Api.Spellbook.CanCast("Cat Form") && !me.HasPermanent("Cat Form"))	
{
			Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Cat Form" );
			Console.ResetColor();

            if (Api.Spellbook.Cast("Cat Form"))
                return true;
        }
if (Api.Spellbook.CanCast("Faerie Fire (Feral)"))
{
    var hasFaerieFireAura = target.HasAura("Faerie Fire (Feral)", AuraFlags.None);

    if (!hasFaerieFireAura || target.AuraRemains("Faerie Fire (Feral)") <= 1000)
    {
        if (Api.Spellbook.Cast("Faerie Fire (Feral)"))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Casting Faerie Fire (Feral)");
            Console.ResetColor();
            return true;
        }
    }
}

if (Api.Spellbook.CanCast("Maim") && (target.IsCasting() || target.IsChanneling()))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Maim");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Maim"))
        return true;
}


if (Api.EnemiesNearby(5,false,false) >= 2 && Api.Spellbook.CanCast("Berserk") && !me.HasAura("Berserk"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Berserk");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Berserk"))
    {
        return true;
    }
}
else if (me.HasAura("Berserk") && Api.Spellbook.CanCast("Mangle (Cat)"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Mangle (Cat)");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Mangle (Cat)"))
    {
        return true;
    }
}


if (Api.Spellbook.CanCast("Savage Roar") && !me.HasAura("Savage Roar") && Points >= 2 && target.HealthPercent >= 40 && Energy >= 25 && me.HasPermanent("Cat Form"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Savage Roar with {Points} Points and {Energy} Energy");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Savage Roar"))
        return true;
}

if (Api.Spellbook.CanCast("Tiger's Fury") && !me.HasAura("Tiger's Fury") && !me.HasAura("Berserk") && target.HealthPercent >= 50 && me.HasPermanent("Cat Form"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Tiger's Fury");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Tiger's Fury"))
        return true;
}


if (Api.Spellbook.CanCast("Rake") && !target.HasAura("Rake") && target.HealthPercent >= 30 && Energy > 40 && me.HasPermanent("Cat Form"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Rake with {Energy} Energy");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Rake"))
        return true;
}
if (Api.Spellbook.CanCast("Rip") && !target.HasAura("Rip") && target.HealthPercent >= 20 && Energy > 30 && Points >= 2 && me.HasPermanent("Cat Form"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Rip with {Points} Points and {Energy} Energy");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Rip"))
        return true;
}

if (Api.Spellbook.CanCast("Ferocious Bite") && Energy > 35 && Points >= 5 && me.HasPermanent("Cat Form") )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Ferocious Bite with {Points} Points and {Energy} Energy");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Ferocious Bite"))
        return true;
}
if (Api.Spellbook.CanCast("Mangle (Cat)") && Points < 5 && Energy >= 45 && !target.HasAura("Mangle (Cat)") && me.HasPermanent("Cat Form"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Mangle (Cat) with {Energy} Energy");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Mangle (Cat)"))
        return true;
}

if (Api.Spellbook.CanCast("Claw")  && Energy >= 45 && me.HasPermanent("Cat Form"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Casting Claw with {me.GetPower(UnitPower.Energy)} Energy");
    Console.ResetColor();

    if (Api.Spellbook.Cast("Claw"))
        return true;
}
		
		
		
		return base.CombatPulse();
    }
	
	
	
	private void LogPlayerStats()
    {
        var me = Api.Player;

        var manaPercentage = me.GetPowerPercent(UnitPower.Mana);
        var healthPercentage = me.HealthPercent;
		var Rage =  me.GetPowerPercent(UnitPower.Rage);
		var Energy =  me.GetPowerPercent(UnitPower.Energy);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{manaPercentage}% Mana available");
        Console.WriteLine($"{healthPercentage}% Health available");
        Console.WriteLine($"{Rage}% Rage available");
        Console.WriteLine($"{Energy}% Energy available");
		Console.ResetColor();

// Check if the player has the Cat Form aura by iterating through the player's auras
// Retrieve the Strength value for a WowPlayer instance
int strengthValue = me.GetStat(UnitStat.Strength);

// Display the Strength value in the console
Console.WriteLine($"Player's Strength: {strengthValue}");

// Retrieve the melee haste value from UnitRatings
double meleeHaste = me.Ratings.MeleeHaste;

// Display the melee haste value in the console
Console.WriteLine($"Player's Melee Haste: {meleeHaste}");


if (me.HasAura("Thorns")) // Replace "Thorns" with the actual aura name
{
		 Console.ForegroundColor = ConsoleColor.Blue;

    var remainingTimeSeconds = me.AuraRemains("Thorns");
    var remainingTimeMinutes = remainingTimeSeconds / 60; // Convert seconds to minutes
    var roundedMinutes = Math.Round(remainingTimeMinutes/ 1000,1); // Round to one decimal place

    Console.WriteLine($"Remaining time for Thorns: {roundedMinutes} minutes");
}

if (me.HasAura("Mark of the Wild")) // Replace "Thorns" with the actual aura name
{
    var remainingTimeSeconds = me.AuraRemains("Mark of the Wild");
    var remainingTimeMinutes = remainingTimeSeconds / 60; // Convert seconds to minutes
    var roundedMinutes = Math.Round(remainingTimeMinutes/ 1000,1); // Round to one decimal place

    Console.WriteLine($"Remaining time for Mark of the Wild: {roundedMinutes} minutes");
	Console.ResetColor();
}

if (me.HasPermanent("Cat Form"))
{
    Console.WriteLine($"We are in Cat Form");
}
else
{
    Console.WriteLine($"We are not in Cat Form");
}
Console.ResetColor();


Console.ResetColor();

    }
	
}