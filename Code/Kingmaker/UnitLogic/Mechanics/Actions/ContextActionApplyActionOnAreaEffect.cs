using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("4e8a6b3e7cdb39b4ca7e9fccaf2ae18c")]
public class ContextActionApplyActionOnAreaEffect : ContextAction
{
	[Tooltip("Blueprint AreaEffect, к которому будет применяться Action")]
	[SerializeField]
	[FormerlySerializedAs("AreaEffect")]
	private BlueprintAbilityAreaEffectReference m_AreaEffect;

	[SerializeField]
	private ActionList m_Actions;

	[Tooltip("Если выбрано, то Actions могут применяться на клетки, занятые юнитами")]
	[SerializeField]
	private bool m_ApplyWithUnitInNode;

	[SerializeField]
	private bool m_ApplyOnEveryNode;

	[SerializeField]
	[HideIf("m_ApplyOnEveryNode")]
	private int m_NumberOfNodes;

	public override string GetCaption()
	{
		return $"Apply action on a random cell inside {m_AreaEffect}";
	}

	protected override void RunAction()
	{
		if (TryGetActiveAreaEffect(out var areaEffect, m_AreaEffect.Get()))
		{
			RunActionsOnNodes(areaEffect, m_ApplyWithUnitInNode, m_NumberOfNodes);
		}
	}

	private bool TryGetActiveAreaEffect(out AreaEffectEntity areaEffect, BlueprintAbilityAreaEffect areaEffectToSearch)
	{
		areaEffect = Game.Instance.State.AreaEffects.All.FirstOrDefault((AreaEffectEntity area) => area.Blueprint == areaEffectToSearch);
		return areaEffect != null;
	}

	private void RunActionsOnNodes(AreaEffectEntity area, bool useOccupiedCells, int numberOfNodes)
	{
		foreach (CustomGridNodeBase coveredNode in area.CoveredNodes)
		{
			if (!useOccupiedCells && coveredNode.ContainsUnit())
			{
				continue;
			}
			if (m_ApplyOnEveryNode)
			{
				RunActionsOnNode(coveredNode);
				continue;
			}
			if (numberOfNodes > 0)
			{
				RunActionsOnNode(coveredNode);
				numberOfNodes--;
				continue;
			}
			break;
		}
	}

	private void RunActionsOnNode(CustomGridNodeBase node)
	{
		using (base.Context.GetDataScope(new TargetWrapper(node.Vector3Position)))
		{
			m_Actions.Run();
		}
	}
}
