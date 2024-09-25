using System;
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

	public bool FollowWhileCutscene => m_FollowWhileCutscene;

	public bool AlwaysRun => m_AlwaysRun;

	public bool CanBeSlowerThanLeader => m_CanBeSlowerThanLeader;

	public bool FollowInCombat => m_FollowInCombat;
}
