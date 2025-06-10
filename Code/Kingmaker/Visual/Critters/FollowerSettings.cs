using System;
using Kingmaker.Formations;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

[Serializable]
public class FollowerSettings
{
	[SerializeField]
	private bool m_FollowWhileCutscene;

	[SerializeField]
	private bool m_AlwaysRun;

	[SerializeField]
	private bool m_CanBeSlowerThanLeader;

	[SerializeField]
	private bool m_FollowInCombat;

	[SerializeField]
	private bool m_UsePersonalSettings;

	[ShowIf("m_UsePersonalSettings")]
	[SerializeField]
	private FormationPersonalSettings m_FollowingSettingsOverride;

	public bool FollowWhileCutscene => m_FollowWhileCutscene;

	public bool AlwaysRun => m_AlwaysRun;

	public bool CanBeSlowerThanLeader => m_CanBeSlowerThanLeader;

	public bool FollowInCombat => m_FollowInCombat;

	public bool UsePersonalSettings => m_UsePersonalSettings;

	public FormationPersonalSettings FollowingSettingsOverride => m_FollowingSettingsOverride;

	public FollowerSettings(FormationPersonalSettings formationSettings = null)
	{
		m_FollowingSettingsOverride = formationSettings;
		m_UsePersonalSettings = formationSettings != null;
	}
}
