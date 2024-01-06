using System;
using System.Threading;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.Warcraft.Structures.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Spell;



public class RetPala : Rotation
{
	
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
	private DateTime lastcrusaderShotTime = DateTime.MinValue;
    private TimeSpan crusader = TimeSpan.FromSeconds(6.5);

	
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

		if (me.IsDead() || me.IsGhost() || me.IsCasting() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		if ((DateTime.Now - lastDebugTime).TotalSeconds >= debugInterval)
        {
            LogPlayerStats();
            lastDebugTime = DateTime.Now; // Update lastDebugTime
        }
		if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 50 && mana >20)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Holy Light");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Holy Light"))
    {
        return true;
    }
}


  

if (Api.Spellbook.CanCast("Blessing of Wisdom") && !me.HasAura("Blessing of Might") && mana < 30)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Blessing of Wisdom");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Blessing of Wisdom"))
    {
        return true;
    }
}
else if (!me.HasAura("Blessing of Might") && mana > 30)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Blessing of Might");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Blessing of Might"))
    {
        return true;
    }
}
if (Api.Spellbook.CanCast("Devotion Aura") && !me.HasPermanent("Devotion Aura")  )
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Casting Devotion Aura");
						Console.ResetColor();

					if (Api.Spellbook.Cast("Devotion Aura"))
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
		var targethp = target.HealthPercent;
		if (me.IsDead() || me.IsGhost() || me.IsCasting() ) return false;
        if (me.HasAura("Drink") || me.HasAura("Food")) return false;
		
		
		if (Api.Spellbook.CanCast("Devotion Aura") && !me.HasPermanent("Devotion Aura")  )
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Casting Devotion Aura");
						Console.ResetColor();

					if (Api.Spellbook.Cast("Devotion Aura"))
						{
					return true;
						}
					}
		var hasPoisonDebuff = me.HasDebuff("Poison") || me.HasDebuff("Rabies")|| me.HasDebuff("Tetanus");

if (hasPoisonDebuff && Api.Spellbook.CanCast("Purify") && mana >32)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Have poison debuff casting Purify");
    Console.ResetColor();
	if (Api.Spellbook.Cast("Purify"))
        
            return true;
}
if ( Api.HasMacro("Hands"))
        {
            if ((DateTime.Now - lastcrusaderShotTime) >= crusader)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Casting Crusader Strike");
                Console.ResetColor();

                if (Api.UseMacro("Hands"))
                {
                    lastcrusaderShotTime = DateTime.Now;
                    return true;
                }
            }
            else
            {
                // If the cooldown period for Chimera Shot hasn't elapsed yet
                Console.WriteLine("Crusader Strike is on cooldown. Skipping cast.");
            }
        }
		
		if (Api.Spellbook.CanCast("Divine Protection") && healthPercentage <= 45 && !me.HasAura("Forbearance"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Divine Protection");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Divine Protection"))
        
            return true;
        
    }
		if (Api.Spellbook.CanCast("Lay on Hands") && healthPercentage <= 10 && !me.HasAura("Forbearance"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Lay on Hands");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Lay on Hands"))
        
            return true;
        
    }
		if (Api.Spellbook.CanCast("Holy Light") && healthPercentage <= 45 && mana > 20)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Holy Light");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Holy Light"))
        
            return true;
        
    }
	if (Api.Spellbook.CanCast("Consecration") && !Api.Spellbook.OnCooldown("Consecration") && targethp>=30 )
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Consecration");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Consecration"))
        
            return true;
        
    }
	if (Api.Spellbook.CanCast("Crusader Strike") )
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Crusader Strike");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Crusader Strike"))
        
            return true;
        
    }
	if (!me.HasAura("Seal of Command") && Api.Spellbook.CanCast("Seal of Command") && !Api.Spellbook.OnCooldown("Seal of Command"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Seal of Command");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Seal of Command"))
        
            return true;
        
    }
	else if (!me.HasAura("Seal of Righteousness") && Api.Spellbook.CanCast("Seal of Righteousness") && !Api.Spellbook.OnCooldown("Seal of Righteousness") && !me.HasAura("Seal of Command"))
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Casting Seal of Righteousness");
        Console.ResetColor();
        if (Api.Spellbook.Cast("Seal of Righteousness"))
        
            return true;
        
    }
	
	if (Api.Spellbook.CanCast("Hammer of Justice") && !Api.Spellbook.OnCooldown("Hammer of Justice") && (target.IsCasting() || target.IsChanneling()))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Hammer of Justice");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Hammer of Justice"))
    {
        return true;
    }
}

if (Api.Spellbook.CanCast("Judgement") && !Api.Spellbook.OnCooldown("Judgement")&& !Api.Spellbook.OnCooldown("Judgement") && (me.HasAura("Seal of Righteousness") ||me.HasAura("Seal of Command")))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Judgement");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Judgement"))
    {
        return true;
    }
}
//DPS rotation


	
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

// Assuming me is an instance of a player character
var hasPoisonDebuff = me.HasDebuff("Poison");

if (hasPoisonDebuff)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Have poison debuff");
    Console.ResetColor();
}

Console.ResetColor();
    }
	}
