using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("e3e5cb605167439d917581a5bd82dd85")]
public class SetAnomalyToNonInteractable : GameAction
{
	[SerializeField]
	private BlueprintAnomaly.Reference m_Anomaly;

	private BlueprintAnomaly Anomaly => m_Anomaly?.Get();

	public override string GetCaption()
	{
		return "Set " + Anomaly.Name + " to non interactable";
	}

	protected override void RunAction()
	{
		if (Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity data) => data is AnomalyEntityData anomalyEntityData2 && anomalyEntityData2.Blueprint == Anomaly) is AnomalyEntityData anomalyEntityData)
		{
			anomalyEntityData.SetNonInteractable();
		}
		else
		{
			Game.Instance.Player.StarSystemsState.AnomalySetToNonInteractable.Add(Anomaly);
		}
	}
}
