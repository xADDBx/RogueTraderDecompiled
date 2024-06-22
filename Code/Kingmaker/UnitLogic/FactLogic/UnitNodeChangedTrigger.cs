using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("225169bcb6b94a8686a6bc4293d85d24")]
public class UnitNodeChangedTrigger : UnitBuffComponentDelegate, IUnitNodeChangedHandler<EntitySubscriber>, IUnitNodeChangedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitNodeChangedHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[Tooltip("Run actions for each old node")]
	private ActionList m_OldNodesActions;

	[SerializeField]
	[Tooltip("Run actions for each new node")]
	private ActionList m_NewNodesActions;

	[SerializeField]
	private bool m_AllowNotInCombat;

	public void HandleUnitNodeChanged(GraphNode oldNode)
	{
		if (!base.Owner.IsInCombat && !m_AllowNotInCombat)
		{
			return;
		}
		if (base.Fact.Owner.SizeRect.Width == 1)
		{
			if (oldNode != null)
			{
				Run(oldNode, m_OldNodesActions);
			}
			if (base.Fact.Owner.CurrentNode.node != null)
			{
				Run(base.Fact.Owner.CurrentNode.node, m_NewNodesActions);
			}
			return;
		}
		NodeList occupiedNodes = base.Fact.Owner.GetOccupiedNodes(oldNode);
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>(base.Fact.Owner.GetOccupiedNodes());
		foreach (CustomGridNodeBase item in occupiedNodes)
		{
			hashSet.Remove(item);
		}
		foreach (CustomGridNodeBase item2 in occupiedNodes)
		{
			Run(item2, m_OldNodesActions);
		}
		foreach (GraphNode item3 in hashSet)
		{
			Run(item3, m_NewNodesActions);
		}
	}

	private void Run(GraphNode node, ActionList actionList)
	{
		TargetWrapper target = node.Vector3Position;
		using (base.Fact.MaybeContext?.GetDataScope(target))
		{
			base.Fact.RunActionInContext(actionList, target);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
