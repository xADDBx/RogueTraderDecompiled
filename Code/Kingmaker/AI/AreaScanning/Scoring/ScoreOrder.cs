using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.AI.AreaScanning.Scoring;

[Serializable]
public class ScoreOrder
{
	[SerializeField]
	[HideInInspector]
	private ScorePair[] order;

	public IEnumerable<ScoreType> Order => order.Select((ScorePair x) => x.type);

	public ScoreOrder()
	{
		order = new ScorePair[ScoreHelper.ScoreTypesNum];
		for (int i = 0; i < ScoreHelper.ScoreTypesNum; i++)
		{
			order[i] = new ScorePair
			{
				type = (ScoreType)i,
				factor = ScoreFactor.Default
			};
		}
	}

	public ScoreOrder(ScoreOrder other)
	{
		order = new ScorePair[ScoreHelper.ScoreTypesNum];
		for (int i = 0; i < ScoreHelper.ScoreTypesNum; i++)
		{
			order[i] = other.order[i];
		}
	}

	private ScoreOrder(List<ScorePair> pairs)
	{
		order = new ScorePair[ScoreHelper.ScoreTypesNum];
		for (int i = 0; i < ScoreHelper.ScoreTypesNum; i++)
		{
			ScoreType scoreType = (ScoreType)i;
			if (pairs.All((ScorePair p) => p.type != scoreType))
			{
				pairs.Add(new ScorePair
				{
					type = scoreType,
					factor = ScoreFactor.Default
				});
			}
		}
		for (int j = 0; j < ScoreHelper.ScoreTypesNum; j++)
		{
			order[j] = pairs[j];
		}
	}

	public ScoreOrder WithForcedPriority(ScorePair forcedPair)
	{
		List<ScorePair> list = order.ToList();
		int num = list.Count - 1;
		while (list[num].type != forcedPair.type)
		{
			num--;
		}
		while (num > 0)
		{
			list[num] = list[num - 1];
			num--;
		}
		list[0] = forcedPair;
		return new ScoreOrder(list);
	}

	public IEnumerator<ScorePair> GetEnumerator()
	{
		return order.Cast<ScorePair>().GetEnumerator();
	}

	public void SetFactor(ScoreType type, ScoreFactor factor)
	{
		for (int i = 0; i < order.Length; i++)
		{
			if (order[i].type == type)
			{
				order[i].factor = factor;
				break;
			}
		}
	}

	public int Compare(ScoreSet s1, ScoreSet s2)
	{
		if (s1.IsZero && s2.IsZero)
		{
			return 0;
		}
		if (s1.IsZero)
		{
			return -1;
		}
		if (s2.IsZero)
		{
			return 1;
		}
		ScorePair[] array = order;
		for (int i = 0; i < array.Length; i++)
		{
			ScorePair scorePair = array[i];
			if (scorePair.factor == ScoreFactor.Ignored)
			{
				continue;
			}
			int num = s1.Get(scorePair.type).CompareTo(s2.Get(scorePair.type));
			if (num != 0)
			{
				if (scorePair.factor != 0)
				{
					return -num;
				}
				return num;
			}
		}
		return 0;
	}
}
