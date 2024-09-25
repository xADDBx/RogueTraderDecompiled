namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

public static class SpellDescriptorOperations
{
	public const SpellDescriptor SystemDescriptors = SpellDescriptor.Sickened | SpellDescriptor.Shaken | SpellDescriptor.Fatigue | SpellDescriptor.Staggered | SpellDescriptor.Frightened | SpellDescriptor.Exhausted | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Blindness | SpellDescriptor.StatDebuff | SpellDescriptor.RestoreHP | SpellDescriptor.TemporaryHP | SpellDescriptor.BreathWeapon | SpellDescriptor.Bleed | SpellDescriptor.VilderavnBleed | SpellDescriptor.Ground | SpellDescriptor.GazeAttack | SpellDescriptor.UndeadControl;

	public const SpellDescriptor NegativeEffects = SpellDescriptor.MindAffecting | SpellDescriptor.Fear | SpellDescriptor.Compulsion | SpellDescriptor.Emotion | SpellDescriptor.Poison | SpellDescriptor.Disease | SpellDescriptor.Charm | SpellDescriptor.Daze | SpellDescriptor.Sickened | SpellDescriptor.Shaken | SpellDescriptor.Fatigue | SpellDescriptor.Staggered | SpellDescriptor.Nauseated | SpellDescriptor.Exhausted | SpellDescriptor.Stun | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Blindness | SpellDescriptor.Curse | SpellDescriptor.Sleep | SpellDescriptor.StatDebuff | SpellDescriptor.Bleed | SpellDescriptor.VilderavnBleed | SpellDescriptor.Petrified | SpellDescriptor.NegativeEmotion | SpellDescriptor.UndeadControl;

	public const SpellDescriptor DebilitatingEffects = SpellDescriptor.Shaken | SpellDescriptor.Nauseated | SpellDescriptor.Stun | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Sleep | SpellDescriptor.Petrified | SpellDescriptor.UndeadControl;

	public static bool HasAnyFlag(this SpellDescriptor descriptor, SpellDescriptor flags)
	{
		return (descriptor & flags) != 0;
	}

	public static bool HasAllFlags(this SpellDescriptor descriptor, SpellDescriptor flags)
	{
		return (descriptor & flags) == flags;
	}

	public static bool IsSystemSpellDescriptor(this SpellDescriptor descriptor)
	{
		return (descriptor & (SpellDescriptor.Sickened | SpellDescriptor.Shaken | SpellDescriptor.Fatigue | SpellDescriptor.Staggered | SpellDescriptor.Frightened | SpellDescriptor.Exhausted | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Blindness | SpellDescriptor.StatDebuff | SpellDescriptor.RestoreHP | SpellDescriptor.TemporaryHP | SpellDescriptor.BreathWeapon | SpellDescriptor.Bleed | SpellDescriptor.VilderavnBleed | SpellDescriptor.Ground | SpellDescriptor.GazeAttack | SpellDescriptor.UndeadControl)) != 0;
	}

	public static bool IsNegativeEffect(this SpellDescriptor descriptor)
	{
		return (descriptor & (SpellDescriptor.MindAffecting | SpellDescriptor.Fear | SpellDescriptor.Compulsion | SpellDescriptor.Emotion | SpellDescriptor.Poison | SpellDescriptor.Disease | SpellDescriptor.Charm | SpellDescriptor.Daze | SpellDescriptor.Sickened | SpellDescriptor.Shaken | SpellDescriptor.Fatigue | SpellDescriptor.Staggered | SpellDescriptor.Nauseated | SpellDescriptor.Exhausted | SpellDescriptor.Stun | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Blindness | SpellDescriptor.Curse | SpellDescriptor.Sleep | SpellDescriptor.StatDebuff | SpellDescriptor.Bleed | SpellDescriptor.VilderavnBleed | SpellDescriptor.Petrified | SpellDescriptor.NegativeEmotion | SpellDescriptor.UndeadControl)) != 0;
	}

	public static SpellDescriptor GetNegativeEffects(this SpellDescriptor descriptor)
	{
		return descriptor & (SpellDescriptor.MindAffecting | SpellDescriptor.Fear | SpellDescriptor.Compulsion | SpellDescriptor.Emotion | SpellDescriptor.Poison | SpellDescriptor.Disease | SpellDescriptor.Charm | SpellDescriptor.Daze | SpellDescriptor.Sickened | SpellDescriptor.Shaken | SpellDescriptor.Fatigue | SpellDescriptor.Staggered | SpellDescriptor.Nauseated | SpellDescriptor.Exhausted | SpellDescriptor.Stun | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Blindness | SpellDescriptor.Curse | SpellDescriptor.Sleep | SpellDescriptor.StatDebuff | SpellDescriptor.Bleed | SpellDescriptor.VilderavnBleed | SpellDescriptor.Petrified | SpellDescriptor.NegativeEmotion | SpellDescriptor.UndeadControl);
	}

	public static bool IsDebilitatingEffect(this SpellDescriptor descriptor)
	{
		return (descriptor & (SpellDescriptor.Shaken | SpellDescriptor.Nauseated | SpellDescriptor.Stun | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Sleep | SpellDescriptor.Petrified | SpellDescriptor.UndeadControl)) != 0;
	}

	public static SpellDescriptor GetDebilitatingEffects(this SpellDescriptor descriptor)
	{
		return descriptor & (SpellDescriptor.Shaken | SpellDescriptor.Nauseated | SpellDescriptor.Stun | SpellDescriptor.Paralysis | SpellDescriptor.Confusion | SpellDescriptor.Sleep | SpellDescriptor.Petrified | SpellDescriptor.UndeadControl);
	}

	public static bool Intersects(this SpellDescriptor descriptor1, SpellDescriptor descriptor2)
	{
		return (descriptor1 & descriptor2) != 0;
	}
}
