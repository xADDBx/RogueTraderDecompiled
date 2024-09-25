using System;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

[Serializable]
public class HideFamiliarSettings
{
	[SerializeField]
	private bool m_HideInCapitalMode;

	[SerializeField]
	private bool m_HideInCombat;

	[SerializeField]
	private bool m_HideInStealth;

	[SerializeField]
	private bool m_HideIfLeaderUnconscious;

	public bool HideInCapitalMode => m_HideInCapitalMode;

	public bool HideInCombat => m_HideInCombat;

	public bool HideInStealth => m_HideInStealth;

	public bool HideIfLeaderUnconscious => m_HideIfLeaderUnconscious;
}
