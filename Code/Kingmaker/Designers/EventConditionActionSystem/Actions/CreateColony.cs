using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("94692eddaeccedc4b8ad9b0bc6304303")]
[PlayerUpgraderAllowed(false)]
public class CreateColony : GameAction
{
	[SerializeField]
	[NotNull]
	public BlueprintPlanet.Reference Planet;

	[CanBeNull]
	public BlueprintColonyTrait.Reference[] ApplyTraits;

	[SerializeField]
	public bool ChangeInitialContentment;

	[ShowIf("ChangeInitialContentment")]
	[SerializeField]
	public int InitialContentmentValue;

	[SerializeField]
	public bool ChangeInitialSecurity;

	[ShowIf("ChangeInitialSecurity")]
	[SerializeField]
	public int InitialSecurityValue;

	[SerializeField]
	public bool ChangeInitialEfficiency;

	[ShowIf("ChangeInitialEfficiency")]
	[SerializeField]
	public int InitialEfficiencyValue;

	public override string GetCaption()
	{
		return "Create colony from component";
	}

	protected override void RunAction()
	{
		if (Planet.Get().GetComponent<ColonyComponent>() == null || !(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap) || !(Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity entity) => entity.Blueprint == Planet.Get()) is PlanetEntity planetEntity))
		{
			return;
		}
		Game.Instance.ColonizationController.Colonize(planetEntity, isPlayerCommand: false);
		if (ApplyTraits != null && planetEntity.Colony != null)
		{
			BlueprintColonyTrait.Reference[] applyTraits = ApplyTraits;
			foreach (BlueprintColonyTrait.Reference reference in applyTraits)
			{
				planetEntity.Colony.AddTrait(reference.Get());
			}
			if (ChangeInitialContentment)
			{
				planetEntity.Colony.Contentment.InitialValue = InitialContentmentValue;
			}
			if (ChangeInitialSecurity)
			{
				planetEntity.Colony.Security.InitialValue = InitialSecurityValue;
			}
			if (ChangeInitialEfficiency)
			{
				planetEntity.Colony.Efficiency.InitialValue = InitialEfficiencyValue;
			}
		}
	}
}
