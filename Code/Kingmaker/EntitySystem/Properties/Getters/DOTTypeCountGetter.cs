using System;
using Code.Enums;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs.Components;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d236685d675b36d4d854743418a46716")]
public class DOTTypeCountGetter : MechanicEntityPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Number of active DOT types on " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		int num = 0;
		foreach (DOT value in Enum.GetValues(typeof(DOT)))
		{
			if (DOTLogic.GetDamageOfTypeInstancesCount(base.CurrentEntity, value) > 0)
			{
				num++;
			}
		}
		return num;
	}
}
