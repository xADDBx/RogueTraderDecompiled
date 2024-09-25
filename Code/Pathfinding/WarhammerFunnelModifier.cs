using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_funnel_modifier.php")]
public class WarhammerFunnelModifier : IPathModifier
{
	public bool unwrap = true;

	public bool splitAtEveryPortal;

	public float shrink;

	int IPathModifier.Order => 10;

	void IPathModifier.Apply(Path p)
	{
		if (p.path == null || p.path.Count == 0 || p.vectorPath == null || p.vectorPath.Count == 0)
		{
			return;
		}
		List<Vector3> list = ListPool<Vector3>.Claim();
		List<WarhammerFunnel.PathPart> list2 = WarhammerFunnel.SplitIntoParts(p);
		if (list2.Count == 0)
		{
			return;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			WarhammerFunnel.PathPart part = list2[i];
			if (!part.isLink)
			{
				WarhammerFunnel.FunnelPortals funnelPortals = WarhammerFunnel.ConstructFunnelPortals(p.path, part);
				WarhammerFunnel.ShrinkPortals(funnelPortals, shrink);
				List<Vector3> list3 = WarhammerFunnel.Calculate(funnelPortals, unwrap, splitAtEveryPortal);
				list.AddRange(list3);
				ListPool<Vector3>.Release(ref funnelPortals.left);
				ListPool<Vector3>.Release(ref funnelPortals.right);
				ListPool<Vector3>.Release(ref list3);
			}
			else
			{
				if (i == 0 || list2[i - 1].isLink)
				{
					list.Add(part.startPoint);
				}
				if (i == list2.Count - 1 || list2[i + 1].isLink)
				{
					list.Add(part.endPoint);
				}
			}
		}
		ListPool<WarhammerFunnel.PathPart>.Release(ref list2);
		ListPool<Vector3>.Release(ref p.vectorPath);
		p.vectorPath = list;
	}

	void IPathModifier.PreProcess(Path path)
	{
	}
}
