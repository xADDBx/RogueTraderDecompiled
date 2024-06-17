using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("b5b2aafedd1685e4e995c48491dafb3c")]
public class WarhammerRestoreResourcesSet : ContextAction
{
	public enum RestoreMode
	{
		Spread_Equal,
		Most_Complete,
		Least_Complete,
		Choose_One_Random
	}

	[SerializeField]
	private ContextValue CommonValue;

	[SerializeField]
	private ContextValue ConcurrentValue = 0;

	[SerializeField]
	[ShowIf("HasConcurrentValue")]
	private RestoreMode restoreMode = RestoreMode.Most_Complete;

	private bool HasConcurrentValue => ConcurrentValue.Value > 0;

	public override string GetCaption()
	{
		string text = $"Restore {CommonValue} to all ultimate resources";
		if (HasConcurrentValue)
		{
			text += $" and {ConcurrentValue} to ultimate resources in {restoreMode} mode";
		}
		return text;
	}

	public static void GrantUltimateResources(MechanicEntity entity, int commonVal, int concVal, RestoreMode mode)
	{
		PartAbilityResourceCollection resources = entity.GetAbilityResourcesOptional();
		if (resources == null)
		{
			return;
		}
		List<BlueprintScriptableObject> list = resources.GetResources.Select((AbilityResource res) => res.Blueprint).ToList();
		int num = 0;
		if (mode == RestoreMode.Spread_Equal)
		{
			commonVal += concVal / list.Count;
			num = concVal % list.Count;
			concVal = 0;
		}
		list.Shuffle(PFStatefulRandom.Mechanics);
		for (int i = 0; i < list.Count; i++)
		{
			int amount = commonVal + ((i < num) ? 1 : 0);
			resources.Restore(list[i], amount);
		}
		switch (mode)
		{
		case RestoreMode.Most_Complete:
			list.Sort(delegate(BlueprintScriptableObject a, BlueprintScriptableObject b)
			{
				int resourceAmount2 = resources.GetResourceAmount(a);
				return resources.GetResourceAmount(b).CompareTo(resourceAmount2);
			});
			break;
		case RestoreMode.Least_Complete:
			list.Sort(delegate(BlueprintScriptableObject a, BlueprintScriptableObject b)
			{
				int resourceAmount3 = resources.GetResourceAmount(a);
				int resourceAmount4 = resources.GetResourceAmount(b);
				return resourceAmount3.CompareTo(resourceAmount4);
			});
			break;
		}
		foreach (BlueprintScriptableObject item in list)
		{
			if (concVal <= 0)
			{
				break;
			}
			int resourceAmount = resources.GetResourceAmount(item);
			int num2 = Math.Min(resources.GetResourceMax(item) - resourceAmount, concVal);
			resources.Restore(item, num2);
			concVal -= num2;
		}
	}

	public static void SpendUltimateResources(MechanicEntity entity, int value, RestoreMode mode)
	{
		PartAbilityResourceCollection resources = entity.GetAbilityResourcesOptional();
		if (resources == null)
		{
			return;
		}
		List<BlueprintScriptableObject> list = resources.GetResources.Select((AbilityResource res) => res.Blueprint).ToList();
		list.Shuffle(PFStatefulRandom.Mechanics);
		switch (mode)
		{
		case RestoreMode.Spread_Equal:
		{
			int num = value / list.Count;
			int num2 = value % list.Count;
			for (int i = 0; i < list.Count; i++)
			{
				int amount = num + ((i < num2) ? 1 : 0);
				resources.Spend(list[i], amount);
			}
			return;
		}
		case RestoreMode.Most_Complete:
			list.Sort(delegate(BlueprintScriptableObject a, BlueprintScriptableObject b)
			{
				int resourceAmount = resources.GetResourceAmount(a);
				return resources.GetResourceAmount(b).CompareTo(resourceAmount);
			});
			break;
		case RestoreMode.Least_Complete:
			list.Sort(delegate(BlueprintScriptableObject a, BlueprintScriptableObject b)
			{
				int resourceAmount2 = resources.GetResourceAmount(a);
				int resourceAmount3 = resources.GetResourceAmount(b);
				return resourceAmount2.CompareTo(resourceAmount3);
			});
			break;
		}
		foreach (BlueprintScriptableObject item in list)
		{
			if (value <= 0)
			{
				break;
			}
			int num3 = Math.Max(resources.GetResourceAmount(item), value);
			resources.Spend(item, num3);
			value -= num3;
		}
	}

	public override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error("Target is missing");
			return;
		}
		int commonVal = CommonValue.Calculate(base.Context);
		int concVal = ConcurrentValue.Calculate(base.Context);
		GrantUltimateResources(base.Target.Entity, commonVal, concVal, restoreMode);
	}
}
