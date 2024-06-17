using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("006dc20d2ed94924a340a339b6e146ef")]
public class AnomalyShipFactionLevelReached : Condition
{
	[SerializeField]
	private int m_ReputationLvl;

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
		return ReputationHelper.FactionReputationLevelReached(anomalyStarShip.Faction, m_ReputationLvl);
	}
}
