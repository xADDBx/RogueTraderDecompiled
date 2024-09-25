using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

[PlayerUpgraderFilter]
[TypeId("3f84b30d95ddfcf4eb8ddd26cbf70e82")]
public class BlueprintPlayerUpgrader : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintPlayerUpgrader>
	{
	}

	[SerializeField]
	private BlueprintAreaReference m_SpecificArea;

	[SerializeField]
	private BlueprintAreaMechanicsReference m_SpecificMechanic;

	[SerializeField]
	[InfoBox("Will not load user save if crashed during converting. Set true for blocker-level issues.")]
	private bool m_CriticalConvert;

	[SerializeField]
	private ActionList m_Actions;

	public BlueprintAreaMechanicsReference SpecificMechanic => m_SpecificMechanic;

	public BlueprintAreaReference SpecificArea => m_SpecificArea;

	public void Apply()
	{
		try
		{
			ApplyInternal();
		}
		catch (Exception ex)
		{
			PFLog.History.System.Error($"Exception occured while applying player upgrader: {this}");
			PFLog.System.Exception(ex);
			if (m_CriticalConvert)
			{
				throw;
			}
		}
	}

	public void Revert()
	{
		try
		{
			RevertInternal();
		}
		catch (Exception ex)
		{
			PFLog.History.System.Error($"Exception occured while revert player upgrader: {this}");
			PFLog.System.Exception(ex);
			if (m_CriticalConvert)
			{
				throw;
			}
		}
	}

	private void ApplyInternal()
	{
		List<BlueprintPlayerUpgrader> appliedPlayerUpgraders = Game.Instance.Player.AppliedPlayerUpgraders;
		if (appliedPlayerUpgraders.Contains(this))
		{
			return;
		}
		List<BlueprintPlayerUpgrader> ignoredAppliedPlayerUpgraders = Game.Instance.Player.IgnoredAppliedPlayerUpgraders;
		if (ignoredAppliedPlayerUpgraders.Contains(this))
		{
			PFLog.History.System.Log("Remove player upgrader from ignored applied list and add but skipped: " + name);
			ignoredAppliedPlayerUpgraders.Remove(this);
			appliedPlayerUpgraders.Add(this);
			return;
		}
		List<BlueprintPlayerUpgrader> ignoredNotAppliedPlayerUpgraders = Game.Instance.Player.IgnoredNotAppliedPlayerUpgraders;
		if (ignoredNotAppliedPlayerUpgraders.Contains(this))
		{
			PFLog.History.System.Log("Remove player upgrader from ignored not applied list. Wait apply action: " + name);
			ignoredNotAppliedPlayerUpgraders.Remove(this);
		}
		bool flag = false;
		if (!m_SpecificArea.IsEmpty())
		{
			if (m_SpecificArea.Is(Game.Instance.CurrentlyLoadedArea))
			{
				BlueprintAreaMechanics blueprintAreaMechanics = m_SpecificMechanic.Get();
				if (blueprintAreaMechanics == null || blueprintAreaMechanics.IsSceneLoadedNow())
				{
					PFLog.History.System.Log("Applying area-specific player upgrader from blueprint: " + name);
					ApplyOverride();
					flag = true;
				}
			}
			if (!flag && !AreaDataStash.HasData(m_SpecificArea.Guid.ToString(), m_SpecificMechanic.Get()?.Scene.SceneName ?? ""))
			{
				PFLog.History.System.Log("Skipping player upgrader from blueprint: " + name + " because the area is not in the save");
				flag = true;
			}
		}
		else
		{
			flag = true;
			PFLog.History.System.Log("Applying player upgrader from blueprint: " + name);
			ApplyOverride();
		}
		if (flag)
		{
			appliedPlayerUpgraders.Add(this);
		}
	}

	private void RevertInternal()
	{
		List<BlueprintPlayerUpgrader> appliedPlayerUpgraders = Game.Instance.Player.AppliedPlayerUpgraders;
		if (!appliedPlayerUpgraders.Contains(this))
		{
			List<BlueprintPlayerUpgrader> ignoredNotAppliedPlayerUpgraders = Game.Instance.Player.IgnoredNotAppliedPlayerUpgraders;
			if (!ignoredNotAppliedPlayerUpgraders.Contains(this))
			{
				PFLog.History.System.Log("Blueprint place to ignored list: " + name + ".");
				ignoredNotAppliedPlayerUpgraders.Add(this);
			}
		}
		else
		{
			PFLog.History.System.Log("Revert player upgrader from blueprint: " + name + ". It's now in 'ignored but applied' list");
			appliedPlayerUpgraders.Remove(this);
			Game.Instance.Player.IgnoredAppliedPlayerUpgraders.Add(this);
		}
	}

	protected virtual void ApplyOverride()
	{
		m_Actions.Run();
	}
}
