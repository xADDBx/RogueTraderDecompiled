using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerNodeLink : GraphModifier, INodeLink
{
	private const float DefaultCost = 1f;

	private const float MaxNodeOffsetY = 1f;

	[SerializeField]
	private Transform m_Start;

	[SerializeField]
	private Transform m_End;

	[SerializeField]
	private bool m_OverrideCost;

	[SerializeField]
	[ShowIf("m_OverrideCost")]
	private float m_Cost = 1f;

	[SerializeField]
	private Size m_MaxCreatureSize = Size.Medium;

	[SerializeField]
	private Bounds m_Bounds;

	[SerializeField]
	private bool m_IsHighTraverse;

	[SerializeField]
	private bool m_IsJumpTraverse;

	[SerializeField]
	private WarhammerNodeLink m_ConnectedNode;

	[InfoBox("By default only humanoids (units with IsHumanoid == True in their Army type) and player characters can use this links.\nUse this option to set different conditions, like \"Unit has fact\" or \"Unit is a daemon\"\nIMPORTANT: If you still need it to be a humanoid or player character, you should set it manually.")]
	[SerializeField]
	private PropertyCalculator m_CustomConditionForEntity;

	private CustomGridNodeBase m_StartNode;

	private CustomGridNodeBase m_EndNode;

	private Vector3 m_StartPosition;

	private Vector3 m_EndPosition;

	private readonly List<ILinkTraversalProvider> m_CurrentTraverserList = new List<ILinkTraversalProvider>();

	private int m_CompositeWidth;

	private bool CanUseTraverserLink => m_CurrentTraverserList.All((ILinkTraversalProvider t) => t.AllowOtherToUseLink);

	public float CostFactor
	{
		get
		{
			if (!m_OverrideCost)
			{
				return 1f;
			}
			return m_Cost;
		}
	}

	public GraphNode StartNode => m_StartNode;

	public GraphNode EndNode => m_EndNode;

	public Vector3 EndToStartDirection { get; private set; }

	public Bounds Bounds => m_Bounds;

	public bool IsHighTraverse => m_IsHighTraverse;

	public bool IsJumpTraverse => m_IsJumpTraverse;

	public Transform TransformStart => m_Start;

	public Transform TransformEnd => m_End;

	public bool IsCorrectSize(ILinkTraversalProvider traverser)
	{
		return traverser.SizeRect.Width <= m_CompositeWidth;
	}

	public Vector3 GetConnectionPosition(ILinkTraversalProvider traverser)
	{
		if (traverser.SizeRect.Width == 1 || m_ConnectedNode == null)
		{
			return base.transform.position;
		}
		return base.transform.parent.position;
	}

	public bool CanBuildPathThroughLink(ILinkTraversalProvider traverser)
	{
		AbstractUnitEntity abstractUnitEntity = (AbstractUnitEntity)traverser.Traverser;
		if (!IsCorrectSize(traverser))
		{
			return false;
		}
		if (!m_CustomConditionForEntity.Empty)
		{
			return m_CustomConditionForEntity.GetBoolValue(new PropertyContext(abstractUnitEntity, null));
		}
		bool num = abstractUnitEntity.Blueprint.Army?.IsHumanoid ?? false;
		bool isPlayerFaction = abstractUnitEntity.IsPlayerFaction;
		return num || isPlayerFaction;
	}

	public bool CanStartTraverse(ILinkTraversalProvider traverser)
	{
		_ = (AbstractUnitEntity)traverser.Traverser;
		if (traverser.SizeRect.Width == 1)
		{
			return CanUseTraverserLink;
		}
		if (!CanUseTraverserLink)
		{
			return false;
		}
		if (m_ConnectedNode == null)
		{
			return true;
		}
		return m_ConnectedNode.CanUseTraverserLink;
	}

	public bool IsInTraverse()
	{
		return m_CurrentTraverserList != null;
	}

	public bool IsInTraverse(ILinkTraversalProvider traverser)
	{
		return m_CurrentTraverserList.Contains(traverser);
	}

	public void StartTransition(ILinkTraversalProvider traverser)
	{
		m_CurrentTraverserList.Add(traverser);
		if (traverser.SizeRect.Width != 1 && m_ConnectedNode != null)
		{
			m_ConnectedNode.m_CurrentTraverserList.Add(traverser);
		}
	}

	public bool ConnectsNodes(GraphNode from, GraphNode to)
	{
		if (from == StartNode)
		{
			return to == EndNode;
		}
		if (from == EndNode)
		{
			return to == StartNode;
		}
		return false;
	}

	public float GetHeight()
	{
		return Math.Abs(EndNode.Vector3Position.y - StartNode.Vector3Position.y);
	}

	public void CompleteTransition(ILinkTraversalProvider traverser)
	{
		if (m_CurrentTraverserList.Contains(traverser))
		{
			m_CurrentTraverserList.Remove(traverser);
			if (traverser.SizeRect.Width != 1 && m_ConnectedNode != null)
			{
				m_ConnectedNode.m_CurrentTraverserList.Remove(traverser);
			}
		}
	}

	public override void OnPostScan()
	{
		base.OnPostScan();
		AddConnection();
		Initialize();
	}

	public override void OnPostCacheLoad()
	{
		base.OnPostCacheLoad();
		AddConnection();
		Initialize();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Initialize();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		RemoveConnection();
	}

	private void Initialize()
	{
		CalculateTraverseOffset();
		CalculateCompositeWidth();
		WarmupPropertyCalculator();
	}

	private void CalculateTraverseOffset()
	{
		Vector3 startPosition = m_StartPosition;
		startPosition.y = 0f;
		Vector3 endPosition = m_EndPosition;
		endPosition.y = 0f;
		EndToStartDirection = (startPosition - endPosition).normalized;
	}

	private void CalculateCompositeWidth()
	{
		m_CompositeWidth = SizePathfindingHelper.GetRectForSize(m_MaxCreatureSize).Width;
		if (!(m_ConnectedNode == null))
		{
			m_CompositeWidth += SizePathfindingHelper.GetRectForSize(m_ConnectedNode.m_MaxCreatureSize).Width;
		}
	}

	private void AddConnection()
	{
		if (m_Start != null && m_End != null)
		{
			m_StartPosition = m_Start.position;
			m_EndPosition = m_End.position;
		}
		else
		{
			PFLog.Default.Error("WarhammerNodeLink is not setup properly or doesn't belong to mechanic scene");
		}
		NNConstraint @default = NNConstraint.Default;
		@default.constrainWalkability = NNConstraint.WalkableConstraintType.StaticWalkabale;
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(m_StartPosition, null, @default).node;
		CustomGridNodeBase customGridNodeBase2 = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(m_EndPosition, null, @default).node;
		if (customGridNodeBase == null)
		{
			PFLog.Default.Error("WarhammerNodeLink not found startNode");
		}
		else if (customGridNodeBase2 == null)
		{
			PFLog.Default.Error("WarhammerNodeLink not found endNode");
		}
		else if (!(Math.Abs(customGridNodeBase2.Vector3Position.y - m_EndPosition.y) > 1f) && !(Math.Abs(customGridNodeBase.Vector3Position.y - m_StartPosition.y) > 1f))
		{
			m_StartNode = customGridNodeBase;
			m_EndNode = customGridNodeBase2;
			NodeLinksCache.Add(m_StartNode, this);
			NodeLinksCache.Add(m_EndNode, this);
			NNConstraint none = NNConstraint.None;
			int graphIndex = (int)m_StartNode.GraphIndex;
			none.graphMask = ~(1 << graphIndex);
			uint cost = (uint)Mathf.RoundToInt((float)((Int3)(m_StartPosition - m_EndPosition)).costMagnitude * m_Cost);
			m_StartNode.AddConnection(m_EndNode, cost, this);
			m_EndNode.AddConnection(m_StartNode, cost, this);
		}
	}

	private void RemoveConnection()
	{
		if (m_StartNode != null)
		{
			NodeLinksCache.Remove(m_StartNode);
		}
		if (m_EndNode != null)
		{
			NodeLinksCache.Remove(m_EndNode);
		}
	}

	private void WarmupPropertyCalculator()
	{
		if (!m_CustomConditionForEntity.Empty)
		{
			m_CustomConditionForEntity.GetBoolValue(new PropertyContext(Game.Instance.DefaultUnit, null));
		}
	}
}
