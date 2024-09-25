using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class PlanetEntity : StarSystemObjectEntity, IHashable
{
	[JsonProperty]
	public bool IsColonized;

	private Colony m_Colony;

	[JsonProperty]
	public List<BlueprintColonyTrait> Traits = new List<BlueprintColonyTrait>();

	public Colony Colony
	{
		get
		{
			return m_Colony ?? (m_Colony = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData colonyData) => colonyData.Planet == Blueprint && colonyData.Area == Game.Instance.CurrentlyLoadedArea)?.Colony);
		}
		set
		{
			m_Colony = value;
		}
	}

	public bool CanBeColonized
	{
		get
		{
			if (Colony == null && !Game.Instance.Player.ColoniesState.ForbidColonization)
			{
				return Blueprint.GetComponent<ColonyComponent>() != null;
			}
			return false;
		}
	}

	public new BlueprintPlanet Blueprint => base.Blueprint as BlueprintPlanet;

	public new PlanetView View => base.View as PlanetView;

	public PlanetEntity(PlanetView view, BlueprintPlanet blueprint)
		: base(view, blueprint)
	{
		InitializeResources(view, blueprint);
		if (IsScanned)
		{
			AddToScanned(reported: true);
		}
	}

	protected PlanetEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	private void InitializeResources(PlanetView view, BlueprintPlanet blueprint)
	{
		foreach (BasePointOfInterest item in PointOfInterests.Where((BasePointOfInterest poi) => poi is PointOfInterestColonyTrait))
		{
			Traits.Add((item as PointOfInterestColonyTrait)?.Blueprint.Trait);
		}
	}

	protected override void OnScan()
	{
		base.OnScan();
		ScanStarSystemObjectActions component = Blueprint.GetComponent<ScanStarSystemObjectActions>();
		BlueprintPlanetSettingsRoot blueprintPlanetSettingsRoot = BlueprintWarhammerRoot.Instance.BlueprintPlanetSettingsRoot;
		if (component?.Conditions == null || component.Conditions.Check())
		{
			using (ContextData<StarSystemObjectContextData>.Request().Setup(this))
			{
				blueprintPlanetSettingsRoot.ActionsOnScan.Run();
			}
		}
		GameHelper.GainExperience(ExperienceForScan());
		AddToScanned();
	}

	public int ExperienceForScan()
	{
		return BlueprintWarhammerRoot.Instance.BlueprintPlanetSettingsRoot.GainedExp;
	}

	private void AddToScanned(bool reported = false)
	{
		Game.Instance.Player.StarSystemsState.ScannedPlanets.Add(new PlanetExplorationInfo
		{
			StarSystemMap = (Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap),
			Planet = Blueprint,
			IsReportedToAdministratum = reported
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref IsColonized);
		List<BlueprintColonyTrait> traits = Traits;
		if (traits != null)
		{
			for (int i = 0; i < traits.Count; i++)
			{
				Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(traits[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
