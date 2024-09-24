using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("d294e8bd03f44d75a58752bc32bc1351")]
public class ResetPreparedRoundsForParty : GameAction
{
	public override string GetCaption()
	{
		return "Reset WasPreparedForRound for party";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			item.Initiative.WasPreparedForRound = 0;
		}
	}
}
