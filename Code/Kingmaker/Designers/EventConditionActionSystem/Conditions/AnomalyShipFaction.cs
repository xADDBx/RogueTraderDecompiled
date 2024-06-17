using System.Diagnostics.CodeAnalysis;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
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
		StarSystemObjectEntity starSystemObjectEntity = ContextData<StarSystemContextData>.Current?.StarSystemObject;
		AnomalyStarShip anomalyStarShip = starSystemObjectEntity?.Blueprint.GetComponent<AnomalyStarShip>();
		if (anomalyStarShip == null || !anomalyStarShip.HasFaction)
		{
			return false;
		}
		return starSystemObjectEntity?.Blueprint.GetComponent<AnomalyStarShip>()?.Faction == Faction;
	}
}
