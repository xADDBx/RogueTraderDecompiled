using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("6501a827f2cf7ba43a57fec1832f6fbb")]
public class PlayerUpgradeActionsRoot : BlueprintScriptableObject, IBlueprintScanner, IScanOnBuild
{
	[Serializable]
	public class Reference : BlueprintReference<PlayerUpgradeActionsRoot>
	{
	}

	[NotNull]
	[SerializeField]
	[HideInInspector]
	private List<BlueprintPlayerUpgrader.Reference> m_Upgraders = new List<BlueprintPlayerUpgrader.Reference>();

	[NotNull]
	[SerializeField]
	[HideInInspector]
	private List<BlueprintPlayerUpgrader.Reference> m_IgnoreUpgraders = new List<BlueprintPlayerUpgrader.Reference>();

	public IEnumerable<BlueprintPlayerUpgrader> Upgraders => m_Upgraders.Dereference();

	public IEnumerable<BlueprintPlayerUpgrader> IgnoreUpgraders => m_IgnoreUpgraders.Dereference();

	public void ApplyUpgrades()
	{
		int num = Game.Instance.Player.IgnoredAppliedPlayerUpgraders.Count + Game.Instance.Player.IgnoredNotAppliedPlayerUpgraders.Count;
		if (Game.Instance.Player.AppliedPlayerUpgraders.Count == m_Upgraders.Count && num == m_IgnoreUpgraders.Count)
		{
			return;
		}
		foreach (BlueprintPlayerUpgrader upgrader in Upgraders)
		{
			upgrader?.Apply();
		}
		foreach (BlueprintPlayerUpgrader ignoreUpgrader in IgnoreUpgraders)
		{
			ignoreUpgrader?.Revert();
		}
	}

	public void Scan()
	{
	}
}
