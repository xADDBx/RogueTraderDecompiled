using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculatePassedAreaEffects : RulebookEvent
{
	private readonly HashSet<EntityRef<AreaEffectEntity>> m_PassedAreas = new HashSet<EntityRef<AreaEffectEntity>>();

	private readonly CustomGridNodeBase fromNode;

	private readonly CustomGridNodeBase toNode;

	public HashSet<EntityRef<AreaEffectEntity>> PassedAreas => m_PassedAreas;

	public RuleCalculatePassedAreaEffects([NotNull] MechanicEntity initiator, CustomGridNodeBase from, CustomGridNodeBase to)
		: base(initiator)
	{
		fromNode = from;
		toNode = to;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		GatherAllAreasOnPath();
	}

	private void GatherAllAreasOnPath()
	{
		int num = fromNode.CellDistanceTo(toNode);
		if (num == 0)
		{
			return;
		}
		Linecast.Ray2NodeOffsets offsets = new Linecast.Ray2NodeOffsets(fromNode.CoordinatesInGrid, (toNode.Vector3Position - fromNode.Vector3Position).To2D());
		foreach (CustomGridNodeBase item in new Linecast.Ray2Nodes((CustomGridGraph)fromNode.Graph, in offsets))
		{
			if (item == null || CustomGraphHelper.GetWarhammerLength(item.CoordinatesInGrid - fromNode.CoordinatesInGrid) > num)
			{
				break;
			}
			foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
			{
				if (!m_PassedAreas.Contains(areaEffect) && areaEffect.CoveredNodes.Contains(item))
				{
					m_PassedAreas.Add(areaEffect);
				}
			}
		}
	}
}
