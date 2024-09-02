using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartJump : BaseUnitPart, IHashable
{
	public class Chunk : IHashable
	{
		[JsonProperty]
		public float MaxTime;

		[JsonProperty]
		public float PassedTime;

		[JsonProperty]
		public float InClipTime;

		[JsonProperty]
		public float Speed;

		[JsonProperty]
		public bool ProvokeAttackOfOpportunity;

		[JsonProperty]
		public bool IgnoreNavmesh;

		[JsonProperty]
		public bool PrepareForJump;

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
			result.Append(ref InClipTime);
			result.Append(ref Speed);
			result.Append(ref ProvokeAttackOfOpportunity);
			result.Append(ref IgnoreNavmesh);
			result.Append(ref PrepareForJump);
			result.Append(ref TargetPosition);
			Hash128 val = ClassHasher<MechanicEntity>.GetHash128(Pusher);
			result.Append(ref val);
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

	public Chunk Jump(GraphNode targetNode, bool provokeAttackOfOpportunity, int cellsRemaining = 0, MechanicEntity pusher = null, bool useAttack = false)
	{
		if (base.Owner.View.AnimationManager == null)
		{
			return null;
		}
		Vector3 vector = targetNode.Vector3Position - base.Owner.Position;
		float num = vector.magnitude / 5f;
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
			TargetPosition = targetNode.Vector3Position,
			Pusher = pusher,
			PrepareForJump = true,
			Speed = 5f
		};
		UnitAnimationActionHandle unitAnimationActionHandle = base.Owner?.View?.EntityData?.MaybeAnimationManager?.CreateHandle(UnitAnimationType.Jump, errorOnEmpty: false);
		if (base.Owner?.View?.AnimationManager?.GetAction(UnitAnimationType.Jump) is UnitAnimationActionJump unitAnimationActionJump)
		{
			chunk.InClipTime = unitAnimationActionJump.GetInClipLenght();
			chunk.MaxTime = unitAnimationActionJump.GetFlyClipLenght() + unitAnimationActionJump.GetInClipLenght();
			chunk.Speed = vector.magnitude / unitAnimationActionJump.GetFlyClipLenght();
		}
		if (unitAnimationActionHandle != null)
		{
			unitAnimationActionHandle.NeedAttackAfterJump = useAttack;
			base.Owner.View.EntityData.MaybeAnimationManager.Execute(unitAnimationActionHandle);
		}
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
