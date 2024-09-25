using System.Collections.Generic;

namespace Kingmaker.Enums;

public static class ModifierDescriptorHelper
{
	public static readonly HashSet<ModifierDescriptor> DefaultStackingDescriptors = new HashSet<ModifierDescriptor>
	{
		ModifierDescriptor.None,
		ModifierDescriptor.UntypedStackable,
		ModifierDescriptor.CareerAdvancement,
		ModifierDescriptor.OriginAdvancement,
		ModifierDescriptor.OtherAdvancement,
		ModifierDescriptor.ShipSystemComponent
	};

	public static bool IsStackable(this ModifierDescriptor descriptor)
	{
		return DefaultStackingDescriptors.Contains(descriptor);
	}

	public static bool IsAdvancement(this ModifierDescriptor descriptor)
	{
		if (descriptor != ModifierDescriptor.CareerAdvancement && descriptor != ModifierDescriptor.OriginAdvancement)
		{
			return descriptor == ModifierDescriptor.OtherAdvancement;
		}
		return true;
	}

	public static bool IsPermanentModifier(this ModifierDescriptor descriptor)
	{
		if (descriptor != ModifierDescriptor.CareerAdvancement && descriptor != ModifierDescriptor.OriginAdvancement && descriptor != ModifierDescriptor.OtherAdvancement && descriptor != ModifierDescriptor.Difficulty && descriptor != ModifierDescriptor.BaseValue && descriptor != ModifierDescriptor.BaseStatBonus && descriptor != ModifierDescriptor.ShipSystemComponent)
		{
			return descriptor == ModifierDescriptor.Racial;
		}
		return true;
	}
}
