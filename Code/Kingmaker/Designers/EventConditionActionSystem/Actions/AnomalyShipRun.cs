using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("e23ef463879e4908b8b6e43ff12f3379")]
public class AnomalyShipRun : GameAction
{
	public bool DisappearAfterRun;

	public override string GetCaption()
	{
		return "Interacting anomaly run to the edge of system";
	}

	public override void RunAction()
	{
		if (ContextData<StarSystemContextData>.Current?.StarSystemObject is AnomalyEntityData anomalyEntityData)
		{
			float num = FindLastOrbitRadius();
			Vector3 vector = Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity obj) => obj.Blueprint is BlueprintStar)?.Position ?? Vector3.zero;
			Vector3 normalized = (anomalyEntityData.Position - vector).normalized;
			Vector3 destination = vector + normalized * num;
			anomalyEntityData.Move(destination, DisappearAfterRun);
		}
	}

	public static float FindLastOrbitRadius()
	{
		IEnumerable<StarSystemObjectEntity> enumerable = Game.Instance.State.StarSystemObjects.Where((StarSystemObjectEntity obj) => obj is PlanetEntity);
		float num = 0f;
		Vector3 vector = Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity obj) => obj.Blueprint is BlueprintStar)?.Position ?? Vector3.zero;
		foreach (StarSystemObjectEntity item in enumerable)
		{
			if ((item.Position - vector).magnitude > num)
			{
				num = (item.Position - vector).magnitude;
			}
		}
		return num;
	}
}
