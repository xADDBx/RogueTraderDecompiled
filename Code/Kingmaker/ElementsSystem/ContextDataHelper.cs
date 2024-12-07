using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.ElementsSystem;

public static class ContextDataHelper
{
	[CanBeNull]
	public static MechanicEntityFact GetFact()
	{
		object obj = ContextData<Buff.Data>.Current?.Buff;
		if (obj == null)
		{
			obj = ContextData<Feature.Data>.Current?.Feature;
			if (obj == null)
			{
				ItemEnchantment.Data current = ContextData<ItemEnchantment.Data>.Current;
				if (current == null)
				{
					return null;
				}
				obj = current.ItemEnchantment;
			}
		}
		return (MechanicEntityFact)obj;
	}

	public static bool TryGetFactByContext(MechanicsContext context, out MechanicEntityFact fact)
	{
		bool result = true;
		fact = ContextData<Buff.Data>.Current?.Buff;
		MechanicsContext mechanicsContext = fact?.MaybeContext;
		if (mechanicsContext == context)
		{
			return true;
		}
		if (mechanicsContext != null)
		{
			result = false;
		}
		fact = ContextData<Feature.Data>.Current?.Feature;
		mechanicsContext = fact?.MaybeContext;
		if (mechanicsContext == context)
		{
			return true;
		}
		if (mechanicsContext != null)
		{
			result = false;
		}
		fact = ContextData<ItemEnchantment.Data>.Current?.ItemEnchantment;
		mechanicsContext = fact?.MaybeContext;
		if (mechanicsContext == context)
		{
			return true;
		}
		if (mechanicsContext != null)
		{
			result = false;
		}
		return result;
	}
}
