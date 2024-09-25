using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Colonization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("09eb8c915fdc4824a8f21957ca8f6152")]
public class ColonySecurityHigher : Condition
{
	public int Value;

	protected override string GetConditionCaption()
	{
		return "Check if current colony security is higher than value";
	}

	protected override bool CheckCondition()
	{
		Colony colony = Game.Instance.Player.ColoniesState.ColonyContextData.Colony;
		if (colony == null)
		{
			return false;
		}
		return colony.Security.Value > Value;
	}
}
