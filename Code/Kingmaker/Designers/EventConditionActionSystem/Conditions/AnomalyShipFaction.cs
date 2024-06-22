using System.Diagnostics.CodeAnalysis;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("a712e10a564f4b2585a10100aeb078d6")]
public class AnomalyShipFaction : Condition
{
	[NotNull]
	public FactionType Faction;

	protected override string GetConditionCaption()
	{
		return "Check faction of currently interacted anomaly";
	}

	protected override bool CheckCondition()
	{
		StarSystemObjectEntity starSystemObject = Game.Instance.Player.StarSystemsState.StarSystemContextData.StarSystemObject;
		AnomalyStarShip anomalyStarShip = starSystemObject?.Blueprint.GetComponent<AnomalyStarShip>();
		if (anomalyStarShip == null || !anomalyStarShip.HasFaction)
		{
			return false;
		}
		return starSystemObject?.Blueprint.GetComponent<AnomalyStarShip>()?.Faction == Faction;
	}
}
