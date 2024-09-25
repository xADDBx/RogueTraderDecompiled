using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c80ce870f420477dbb07910023c65b27")]
public class PartyUseAbility : GameAction
{
	public AbilitiesHelper.AbilityDescription Description;

	public bool AllowItems;

	public override string GetCaption()
	{
		return string.Format("Party use ability{0}: {1}", AllowItems ? " (items allowed)" : "", Description);
	}

	protected override void RunAction()
	{
		if (!AbilitiesHelper.PartyUseAbility(Description, AllowItems, spend: true))
		{
			Element.LogError("Party can't use ability {0}", Description);
		}
	}
}
