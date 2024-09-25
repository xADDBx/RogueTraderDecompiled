using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Enums;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0c999ae67aa244d183f40f6aeff494e1")]
[PlayerUpgraderAllowed(false)]
public class GainFactionReputation : GameAction
{
	public int Reputation;

	public FactionType Faction;

	public override string GetCaption()
	{
		return $"Gain {Reputation} points for {Faction.ToString()}";
	}

	protected override void RunAction()
	{
		ReputationHelper.GainFactionReputation(Faction, Reputation);
	}
}
