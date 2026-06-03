using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Actions;
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
	public enum JumpPhaseType
	{
		In,
		Fly,
		Out
	}

	public class Chunk : IHashable
	{
		[JsonProperty]
		public float MaxTime;

		[JsonProperty]
		public float PassedTime;

		[JsonProperty]
		public float InClipTime;

		[JsonProperty]
		public float OutClipTime;

		[JsonProperty]
		public float Speed;

		[JsonProperty]
		public JumpPhaseType JumpPhase;

		[JsonProperty(IsReference = false)]
		public Vector3 TargetPosition;

		public bool IsMaxTimePassed => PassedTime >= MaxPassedTime;

		public bool IsMaxFlyTimePassed => PassedTime >= MaxPassedFlyTime;

		public float MaxPassedFlyTime => MaxTime + InClipTime;

		public float MaxPassedTime => MaxTime + InClipTime + OutClipTime;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref MaxTime);
			result.Append(ref PassedTime);
			result.Append(ref InClipTime);
			result.Append(ref OutClipTime);
			result.Append(ref Speed);
			result.Append(ref JumpPhase);
			result.Append(ref TargetPosition);
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
				if (!result.IsMaxTimePassed)
				{
					return result;
				}
				m_Chunks.Dequeue();
			}
			return null;
		}
	}

	public Chunk Jump(GraphNode targetNode, int cellsRemaining = 0, UnitAnimationJumpSubType jumpSubType = UnitAnimationJumpSubType.Jump, float speed = 5f)
	{
		if (base.Owner.View.AnimationManager == null)
		{
			return null;
		}
		float magnitude = (targetNode.Vector3Position - base.Owner.Position).magnitude;
		if (speed.Approximately(0f))
		{
			speed = 5f;
		}
		float num = magnitude / speed;
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
			TargetPosition = targetNode.Vector3Position,
			Speed = speed
		};
		ExecuteJumpAnimationAction(chunk, magnitude, jumpSubType);
		m_Chunks.Enqueue(chunk);
		return chunk;
	}

	protected override void OnDetach()
	{
		FinishJumpFlyAnimation();
	}

	private void ExecuteJumpAnimationAction(Chunk chunk, float shift, UnitAnimationJumpSubType jumpSubType)
	{
		UnitAnimationManager unitAnimationManager = base.Owner?.View?.AnimationManager;
		if (unitAnimationManager == null || base.Owner.State.IsProne)
		{
			return;
		}
		UnitAnimationAction action = unitAnimationManager.GetAction(jumpSubType.ToAnimationType());
		if (action == null)
		{
			return;
		}
		AnimationActionHandle animationActionHandle = unitAnimationManager.CreateHandle(action);
		if (animationActionHandle == null)
		{
			return;
		}
		if (action is UnitAnimationActionJump unitAnimationActionJump)
		{
			chunk.InClipTime = unitAnimationActionJump.GetInClipLength();
			if (jumpSubType != 0)
			{
				chunk.OutClipTime = unitAnimationActionJump.GetOutClipLength();
			}
			if (!unitAnimationActionJump.LoopedFly)
			{
				chunk.MaxTime = unitAnimationActionJump.GetFlyClipLength();
				chunk.Speed = shift / unitAnimationActionJump.GetFlyClipLength();
			}
		}
		unitAnimationManager.Execute(animationActionHandle);
	}

	public void FinishJumpFlyAnimation()
	{
		UnitAnimationManager unitAnimationManager = base.Owner?.View?.AnimationManager;
		if (!(unitAnimationManager == null) && unitAnimationManager.CurrentAction is UnitAnimationActionHandle { Action: UnitAnimationActionJump action } unitAnimationActionHandle)
		{
			action.FinishFly(unitAnimationActionHandle);
		}
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
