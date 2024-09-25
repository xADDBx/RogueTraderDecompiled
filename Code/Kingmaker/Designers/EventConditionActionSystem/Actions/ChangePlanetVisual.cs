using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("7def417596434663b95228f37bc18af3")]
[PlayerUpgraderAllowed(false)]
public class ChangePlanetVisual : GameAction
{
	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	[SerializeField]
	private BlueprintPlanetPrefab.Reference m_NewVisualBlueprint;

	private BlueprintPlanet Planet => m_Planet?.Get();

	public override string GetCaption()
	{
		return "Change planet visual";
	}

	protected override void RunAction()
	{
		if (Planet != null)
		{
			Dictionary<BlueprintPlanet, BlueprintPlanetPrefab> planetChangedVisualPrefabs = Game.Instance.Player.StarSystemsState.PlanetChangedVisualPrefabs;
			if (planetChangedVisualPrefabs.ContainsKey(Planet))
			{
				planetChangedVisualPrefabs[Planet] = m_NewVisualBlueprint;
			}
			else
			{
				planetChangedVisualPrefabs.Add(Planet, m_NewVisualBlueprint);
			}
			(Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity sso) => sso.Blueprint == Planet) as PlanetEntity)?.View.SetNewVisual();
		}
	}
}
