using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("2ddcc4c227cf4a54a95d426804d7dd17")]
public class PartyCanUseAbility : Condition
{
	public AbilitiesHelper.AbilityDescription Description;

	public bool AllowItems;

	protected override string GetConditionCaption()
	{
		return string.Format("Can party use ability{0}: {1}", AllowItems ? " (items allowed)" : "", Description);
	}

	protected override bool CheckCondition()
	{
		return AbilitiesHelper.PartyUseAbility(Description, AllowItems, spend: false);
	}
}
