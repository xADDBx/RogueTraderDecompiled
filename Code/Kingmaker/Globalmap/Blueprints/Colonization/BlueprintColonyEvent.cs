using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("a7423956769d433f9a182d3e557ad427")]
public class BlueprintColonyEvent : BlueprintScriptableObject, IUIDataProvider
{
	[SerializeField]
	private LocalizedString m_Name;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private BlueprintDialogReference m_Event;

	[SerializeField]
	private BlueprintPlanet.Reference m_ExclusivePlanet;

	[SerializeField]
	private BlueprintPlanet.Reference[] m_IgnorePlanets;

	[SerializeField]
	private ConditionsChecker m_StartConditions;

	[SerializeField]
	public bool CanBeRepeated;

	public BlueprintDialog Event => m_Event.Get();

	public BlueprintPlanet ExclusivePlanet => m_ExclusivePlanet?.Get();

	public string Name => m_Name.Text;

	public string Description => m_Description.Text;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => null;

	public bool IsExclusive => ExclusivePlanet != null;

	public bool IsExclusiveForPlanet(BlueprintPlanet planet)
	{
		if (IsExclusive)
		{
			return planet == ExclusivePlanet;
		}
		return false;
	}

	public bool IsExclusiveForOtherPlanet(BlueprintPlanet planet)
	{
		if (IsExclusive)
		{
			return planet != ExclusivePlanet;
		}
		return false;
	}

	public bool IsPlanetInIgnoreList(BlueprintPlanet planet)
	{
		if (m_IgnorePlanets != null)
		{
			return m_IgnorePlanets.Any((BlueprintPlanet.Reference x) => x.Get() == planet);
		}
		return false;
	}

	public bool IsAllowedOnPlanet(BlueprintPlanet planet)
	{
		if ((bool)ContextData<IgnoreColonyEventRequirements>.Current)
		{
			return true;
		}
		if (!IsExclusiveForOtherPlanet(planet))
		{
			return !IsPlanetInIgnoreList(planet);
		}
		return false;
	}

	public bool CanStart()
	{
		if (m_StartConditions != null)
		{
			return m_StartConditions.Check();
		}
		return true;
	}
}
