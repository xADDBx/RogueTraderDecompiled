using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;

[Serializable]
[TypeId("0034ba7e84b2499fb7bb2cbc587249ef")]
public class ListBarkBanterEvaluator : BarkBanterEvaluator
{
	[SerializeField]
	private BlueprintBarkBanterList.Reference[] m_BarkBanterLists;

	public override string GetCaption()
	{
		return "Get bark banter from lists";
	}

	protected override BlueprintBarkBanter GetValueInternal()
	{
		return GetValueImpl(m_BarkBanterLists.Dereference());
	}

	[Cheat]
	public static void Debug_Show_Banter_List(BlueprintBarkBanterList list)
	{
		BlueprintBarkBanter barkBanter = GetValueImpl(Enumerable.Repeat(list, 1));
		EventBus.RaiseEvent(delegate(IBarkBanterPlayedHandler e)
		{
			e.HandleBarkBanter(barkBanter);
		});
	}

	private static BlueprintBarkBanter GetValueImpl(IEnumerable<BlueprintBarkBanterList> list)
	{
		return GetWeightedBarkBanters(BlueprintRoot.Instance.Dialog.DefaultBlueprintBarkBanterLists.Concat(list)).Random(PFStatefulRandom.Designers);
	}

	private static List<BlueprintBarkBanter> GetWeightedBarkBanters(IEnumerable<BlueprintBarkBanterList> lists)
	{
		List<BlueprintBarkBanter> list = new List<BlueprintBarkBanter>();
		float num = float.MinValue;
		foreach (BlueprintBarkBanterList list2 in lists)
		{
			if (Math.Abs(list2.Weight - num) < float.Epsilon)
			{
				list.AddRange(from v in list2.GetBarkBanters()
					where v?.CanBePlayed() ?? false
					select v);
			}
			else if (list2.Weight > num)
			{
				IEnumerable<BlueprintBarkBanter> any = GetAny(list2.GetBarkBanters());
				if (any.Any())
				{
					list.Clear();
					list.AddRange(any);
					num = list2.Weight;
				}
			}
		}
		return list;
	}

	private static IEnumerable<BlueprintBarkBanter> GetAny(IEnumerable<BlueprintBarkBanter> barkBanters)
	{
		return barkBanters.Where((BlueprintBarkBanter e) => e?.CanBePlayed() ?? false);
	}
}
