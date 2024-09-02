using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.UnitLogic.Buffs.Components;

public static class DOTLogicUIExtensions
{
	public static DamageData CalculateDOTDamage(Buff buff)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			Buff buff2 = null;
			DOTLogic dOTLogic = null;
			DOTLogicVisual component = buff.Blueprint.GetComponent<DOTLogicVisual>();
			foreach (Buff item in buff.Owner.Buffs.Enumerable)
			{
				dOTLogic = item.Blueprint?.GetComponent<DOTLogic>();
				if (dOTLogic != null && dOTLogic.Type == component.Type)
				{
					buff2 = item;
					break;
				}
			}
			if (dOTLogic == null || buff2 == null)
			{
				return null;
			}
			return DOTLogic.GetDamageDataOfType(buff2.Owner, dOTLogic.Type);
		}
	}
}
