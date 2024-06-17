using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartForceMove : BaseUnitPart, IHashable
{
	public class Chunk : IHashable
	{
		[JsonProperty]
		public float MaxTime;

		[JsonProperty]
		public float PassedTime;

		[JsonProperty]
		public bool ProvokeAttackOfOpportunity;

		[JsonProperty]
		public bool IgnoreNavmesh;

		[JsonProperty]
		public int CollisionDamageRank;

		[JsonProperty]
		public EntityRef<MechanicEntity> CollisionEntityRef;

		[JsonProperty(IsReference = false)]
		public Vector3 TargetPosition;

		[JsonProperty]
		public MechanicEntity Pusher;

		public bool IsFinished => PassedTime >= MaxTime;

		public float RemainingDistance => Mathf.Max(0f, MaxTime - PassedTime);

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref MaxTime);
			result.Append(ref PassedTime);
			result.Append(ref ProvokeAttackOfOpportunity);
			result.Append(ref IgnoreNavmesh);
			result.Append(ref CollisionDamageRank);
			EntityRef<MechanicEntity> obj = CollisionEntityRef;
			Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref TargetPosition);
			Hash128 val2 = ClassHasher<MechanicEntity>.GetHash128(Pusher);
			result.Append(ref val2);
			return result;
		}
	}

	[JsonProperty]
	private readonly Queue<Chunk> m_Chunks = new Queue<Chunk>();

	public Chunk Active
	{
		get
		{
			Chunk result;
			while (m_Chunks.TryPeek(out result))
			{
				if (!result.IsFinished)
				{
					return result;
				}
				m_Chunks.Dequeue();
			}
			return null;
		}
	}

	public Chunk Push(GraphNode targetNode, bool provokeAttackOfOpportunity, int cellsRemaining = 0, MechanicEntity pusher = null, MechanicEntity collisionEntity = null)
	{
		if ((bool)base.Owner.Features.DisablePush || base.Owner.View.AnimationManager == null)
		{
			return null;
		}
		Vector3 vector = targetNode.Vector3Position - base.Owner.Position;
		float num = ((base.Owner.View.AnimationManager.ForceMoveTime == null) ? (vector.magnitude / 5f) : base.Owner.View.AnimationManager.ForceMoveTime[Mathf.RoundToInt(vector.magnitude / GraphParamsMechanicsCache.GridCellSize)]);
		if (num == 0f)
		{
			PFLog.Default.Error("Push time is zero");
			return null;
		}
		base.Owner.View.MovementAgent.Blocker.Unblock();
		base.Owner.View.MovementAgent.Blocker.Block(targetNode);
		Chunk chunk = new Chunk
		{
			MaxTime = num,
			ProvokeAttackOfOpportunity = provokeAttackOfOpportunity,
			CollisionDamageRank = cellsRemaining,
			CollisionEntityRef = collisionEntity,
			TargetPosition = targetNode.Vector3Position,
			Pusher = pusher
		};
		m_Chunks.Enqueue(chunk);
		return chunk;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Queue<Chunk> chunks = m_Chunks;
		if (chunks != null)
		{
			foreach (Chunk item in chunks)
			{
				Hash128 val2 = ClassHasher<Chunk>.GetHash128(item);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
