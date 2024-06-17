using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.SpaceCombat.MeteorStream;

public class MeteorEntity : MechanicEntity<BlueprintMeteor>, IHashable
{
	private readonly struct NodeCollector : Linecast.ICanTransitionBetweenCells
	{
		private readonly Dictionary<float, List<CustomGridNodeBase>> m_NodesOnLine;

		public NodeCollector(Dictionary<float, List<CustomGridNodeBase>> nodesOnLine)
		{
			m_NodesOnLine = nodesOnLine;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			if (m_NodesOnLine.TryGetValue(distanceFactor, out var value))
			{
				value.Add(nodeTo);
			}
			else
			{
				m_NodesOnLine.Add(distanceFactor, new List<CustomGridNodeBase> { nodeTo });
			}
			return true;
		}
	}

	private class TraverseData
	{
		public readonly float Distance;

		public readonly List<CustomGridNodeBase> Nodes;

		public TraverseData(float dist, List<CustomGridNodeBase> nodes)
		{
			Distance = dist;
			Nodes = nodes;
		}
	}

	private readonly WarhammerSingleNodeBlocker m_NodeBlocker;

	private Queue<TraverseData> m_NodesToTraverse;

	[JsonProperty]
	private MeteorStreamEntity m_StreamEntity;

	[JsonProperty]
	private Vector3 m_Position;

	public override IntRect SizeRect => SizePathfindingHelper.GetRectForSize(base.Blueprint.MeteorSize);

	public IEnumerable<CustomGridNodeBase> GetDangerNodes => m_NodesToTraverse.SelectMany((TraverseData x) => x.Nodes).Distinct().ToList();

	public bool NeedToBeDestroyed { get; private set; }

	public override Vector3 Position
	{
		get
		{
			return m_Position;
		}
		set
		{
			Transform transform = base.View.Or(null)?.transform;
			if (transform != null)
			{
				transform.position = SizePathfindingHelper.FromMechanicsToViewPosition(this, value);
			}
			m_Position = value;
			base.CurrentNode = default(NNInfo);
			base.CurrentUnwalkableNode = null;
		}
	}

	public MeteorEntity(JsonConstructorMark _)
		: base(_)
	{
		m_NodeBlocker = new WarhammerSingleNodeBlocker(this);
	}

	public MeteorEntity(string uniqueId, bool isInGame, BlueprintMeteor blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
		m_NodeBlocker = new WarhammerSingleNodeBlocker(this);
	}

	public void Init(MeteorStreamEntity parent)
	{
		m_StreamEntity = parent;
	}

	protected override IEntityViewBase CreateViewForData()
	{
		if (base.Blueprint.Prefab == null)
		{
			return null;
		}
		MechanicEntityView mechanicEntityView = Object.Instantiate(base.Blueprint.Prefab, m_StreamEntity.View.ViewTransform, worldPositionStays: true);
		AttachView(mechanicEntityView);
		mechanicEntityView.SetVisible(visible: true);
		Position = m_Position;
		return mechanicEntityView;
	}

	public void CreateAndAttachView()
	{
		CreateViewForData();
	}

	public void Move(Vector2 dist)
	{
		Position += dist.To3D();
	}

	public void SetPosition(Vector2 pos)
	{
		Position = pos.To3D();
	}

	public void CheckCollision(float distance)
	{
		if (m_NodesToTraverse.Count <= 0 || distance < m_NodesToTraverse.Peek().Distance)
		{
			return;
		}
		IEnumerable<CustomGridNodeBase> enumerable = m_NodesToTraverse.Dequeue().Nodes.Where((CustomGridNodeBase x) => WarhammerBlockManager.Instance.NodeContainsAny(x));
		IEnumerable<MechanicEntity> allUnits = Game.Instance.TurnController.AllUnits;
		foreach (CustomGridNodeBase occupiedNode in enumerable)
		{
			MechanicEntity mechanicEntity = allUnits.FirstOrDefault((MechanicEntity x) => x.GetOccupiedNodes().Any((CustomGridNodeBase y) => y == occupiedNode));
			if (mechanicEntity != null)
			{
				ProcessCollision(mechanicEntity);
			}
		}
	}

	private void ProcessCollision(MechanicEntity entity)
	{
		PartStarshipNavigation optional = entity.GetOptional<PartStarshipNavigation>();
		if (optional != null && optional.IsSoftUnit)
		{
			Game.Instance.EntityDestroyer.Destroy(entity);
			return;
		}
		entity.GetOptional<PartHealth>()?.DealDamage(base.Blueprint.Damage);
		NeedToBeDestroyed = true;
	}

	public void BlockCurrentNode()
	{
		m_NodeBlocker.BlockAtCurrentPosition();
	}

	public void UnblockCurrentNode()
	{
		m_NodeBlocker.Unblock();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		CalculatePath();
	}

	public void CalculatePath()
	{
		NodeList nodes = GridAreaHelper.GetNodes(Position, SizeRect);
		Dictionary<float, List<CustomGridNodeBase>> dictionary = new Dictionary<float, List<CustomGridNodeBase>>();
		NodeCollector condition = new NodeCollector(dictionary);
		Vector3 vector = new Vector3(m_StreamEntity.StreamDirection.x, 0f, m_StreamEntity.StreamDirection.y) * m_StreamEntity.CellSize;
		foreach (CustomGridNodeBase item in nodes)
		{
			Vector3 end = item.Vector3Position + vector;
			Linecast.LinecastGrid(item.Graph, item.Vector3Position, end, item, out var _, ref condition);
		}
		m_NodesToTraverse = (from x in dictionary
			orderby x.Key
			select new TraverseData(x.Key, x.Value)).ToQueue();
	}

	protected override void OnDestroy()
	{
		base.View.DestroyViewObject();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MeteorStreamEntity>.GetHash128(m_StreamEntity);
		result.Append(ref val2);
		result.Append(ref m_Position);
		return result;
	}
}
