using System;
using System.Threading;
using wShadow.Templates;
using wShadow.Warcraft.Classes;
using wShadow.Warcraft.Defines;
using wShadow.Warcraft.Managers;
using wShadow.Warcraft.Structures.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Player;
using wShadow.Warcraft.Defines.Wow_Spell;



public class EnhaShaman : Rotation
{
private bool HasEnchantment(EquipmentSlot slot, string enchantmentName)
{
    return Api.Equipment.HasEnchantment(slot, enchantmentName);
}
    private int debugInterval = 5; // Set the debug interval in seconds
    private DateTime lastDebugTime = DateTime.MinValue;
	 private DateTime lastRockbiterTime = DateTime.MinValue;   
	private DateTime lastlash = DateTime.MinValue;
	private TimeSpan lashCooldown = TimeSpan.FromSeconds(6.5);

	
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

	bool hasRockbiterEnchantment1 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 1");
	bool hasRockbiterEnchantment2 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 2");
	bool hasRockbiterEnchantment3 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 3");

bool hasAnyRockbiterEnchantment = hasRockbiterEnchantment1 || hasRockbiterEnchantment2 || hasRockbiterEnchantment3;

if (Api.Spellbook.CanCast("Rockbiter Weapon") && !hasAnyRockbiterEnchantment)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Rockbiter Weapon");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Rockbiter Weapon"))
    {
        return true;
    }
}

	bool hasRockbiterEnchantment1off = HasEnchantment(EquipmentSlot.OffHand, "Rockbiter 1");
	bool hasRockbiterEnchantment2off = HasEnchantment(EquipmentSlot.OffHand, "Rockbiter 2");
	bool hasRockbiterEnchantment3off = HasEnchantment(EquipmentSlot.OffHand, "Rockbiter 3");

bool hasAnyRockbiterEnchantment2 = hasRockbiterEnchantment1off || hasRockbiterEnchantment2off || hasRockbiterEnchantment3off;

if (Api.Spellbook.CanCast("Rockbiter Weapon") && !hasAnyRockbiterEnchantment2)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Rockbiter Weapon");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Rockbiter Weapon"))
    {
        return true;
    }
}


if (Api.Spellbook.CanCast("Ghost Wolf") && !me.HasPermanent("Ghost Wolf"))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Ghost Wolf");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Ghost Wolf"))
    {
        return true;
    }
}

if (Api.Spellbook.CanCast("Lightning Shield") && !me.HasAura("Lightning Shield") && mana > 30 )
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
		if (Api.Spellbook.CanCast("Earth Shock") && !Api.Spellbook.OnCooldown("Earth Shock") && (target.IsCasting() || target.IsChanneling()))
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Earth Shock");
    Console.ResetColor();
    if (Api.Spellbook.Cast("Earth Shock"))
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

		if (Api.HasMacro("Lash") && mana >=30 && (DateTime.Now - lastlash) >= lashCooldown)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Casting Lash.");
    Console.ResetColor();

    if (Api.UseMacro("Lash"))
    {
        lastlash = DateTime.Now; // Update the lastCallPetTime after successful casting
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

bool hasRockbiterEnchantment1 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 1");
bool hasRockbiterEnchantment2 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 2");
bool hasRockbiterEnchantment3 = HasEnchantment(EquipmentSlot.MainHand, "Rockbiter 3");

if (hasRockbiterEnchantment1)
{
    Console.WriteLine("Rockbiter 1 enchantment found on weapon");
}
if (hasRockbiterEnchantment2)
{
    Console.WriteLine("Rockbiter 2 enchantment found on weapon");
}
if (hasRockbiterEnchantment3)
{
    Console.WriteLine("Rockbiter 3 enchantment found on weapon");
}



Console.ResetColor();

}

}