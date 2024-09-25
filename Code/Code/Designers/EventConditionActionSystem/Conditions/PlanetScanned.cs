using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;
using UnityEngine;

namespace Code.Designers.EventConditionActionSystem.Conditions;

[TypeId("39b5c25438ca44a199b88f0592ab7252")]
[PlayerUpgraderAllowed(false)]
public class PlanetScanned : Condition
{
	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	private BlueprintPlanet Planet => m_Planet?.Get();

	protected override string GetConditionCaption()
	{
		return "Check planet " + Planet?.Name + " scanned";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.StarSystemsState.ScannedPlanets.FirstOrDefault((PlanetExplorationInfo data) => data.Planet == Planet) != null;
	}
}
