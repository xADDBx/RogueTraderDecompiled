using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Colonization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("99e0c62f06ea0024199cbb2ba69a615e")]
public class ColonyEfficiencyHigher : Condition
{
	public int Value;

	protected override string GetConditionCaption()
	{
		return "Check if current colony efficiency is higher than value";
	}

	protected override bool CheckCondition()
	{
		Colony colony = ContextData<ColonyContextData>.Current?.Colony;
		if (colony == null)
		{
			return false;
		}
		return colony.Efficiency.Value > Value;
	}
}
