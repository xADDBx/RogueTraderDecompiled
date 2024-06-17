using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Credits;

[TypeId("f90a3da6d7e1e7b478c43e0abe1d7e79")]
public class BlueprintCreditsGroup : BlueprintScriptableObject
{
	public Sprite PageIcon;

	public LocalizedString HeaderText;

	public Sprite PageImage;

	public LocalizedString PageText;

	[SerializeField]
	private BlueprintCreditsRolesReference m_RolesData;

	public string CsvName;

	[SerializeField]
	private BlueprintCreditsTeamsReference m_TeamsData;

	[HideInInspector]
	public List<string> OrderTeams = new List<string>();

	[HideInInspector]
	public List<CreditPerson> Persones = new List<CreditPerson>();

	public bool IsBakers;

	public bool ShowInMainMenuCredits;

	public bool ShowInGameCredits;

	public bool ShowInFinalTitles;

	public BlueprintCreditsRoles RolesData => m_RolesData?.Get();

	public BlueprintCreditsTeams TeamsData => m_TeamsData?.Get();
}
