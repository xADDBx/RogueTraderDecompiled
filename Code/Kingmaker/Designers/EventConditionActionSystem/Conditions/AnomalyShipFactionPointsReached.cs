using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("2ee0e9573a7643afbf57c7cd1cd9fa18")]
public class AnomalyShipFactionPointsReached : Condition
{
	[SerializeField]
	private int m_Reputation;

	protected override string GetConditionCaption()
	{
		return "Check faction of currently interacted anomaly";
	}

	protected override bool CheckCondition()
	{
		AnomalyStarShip anomalyStarShip = (ContextData<StarSystemContextData>.Current?.StarSystemObject)?.Blueprint.GetComponent<AnomalyStarShip>();
		if (anomalyStarShip == null || anomalyStarShip.HasFaction)
		{
			return false;
		}
		return ReputationHelper.GetCurrentReputationPoints(anomalyStarShip.Faction) >= m_Reputation;
	}
}
