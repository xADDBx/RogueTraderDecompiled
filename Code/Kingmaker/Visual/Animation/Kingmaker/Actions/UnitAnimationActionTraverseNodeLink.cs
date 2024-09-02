using System;
using System.Collections.Generic;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationClimb", menuName = "Animation Manager/Actions/Unit Traverse NodeLink")]
public class UnitAnimationActionTraverseNodeLink : UnitAnimationAction
{
	[Serializable]
	private class ClipSet
	{
		[SerializeField]
		private AnimationClipWrapper m_TraverseInHorizontal;

		[SerializeField]
		private AnimationClipWrapper m_TraverseInVertical;

		[SerializeField]
		private AnimationClipWrapper m_Traverse;

		[SerializeField]
		private AnimationClipWrapper m_TraverseOutVertical;

		[SerializeField]
		private AnimationClipWrapper m_TraverseOutHorizontal;

		public float VerticalSpeed = 3f;

		public AnimationClipWrapper GetAnimationClip(WarhammerNodeLinkTraverser.State animState)
		{
			return animState switch
			{
				WarhammerNodeLinkTraverser.State.TraverseDownHorizontalIn => m_TraverseInHorizontal, 
				WarhammerNodeLinkTraverser.State.TraverseIn => m_TraverseInVertical, 
				WarhammerNodeLinkTraverser.State.Traverse => m_Traverse, 
				WarhammerNodeLinkTraverser.State.TraverseOut => m_TraverseOutVertical, 
				WarhammerNodeLinkTraverser.State.TraverseUpHorizontalOut => m_TraverseOutHorizontal, 
				_ => throw new ArgumentOutOfRangeException("animState", animState, null), 
			};
		}

		public IEnumerable<AnimationClipWrapper> GetClips()
		{
			yield return m_TraverseInVertical;
			yield return m_TraverseInHorizontal;
			yield return m_Traverse;
			yield return m_TraverseOutVertical;
			yield return m_TraverseOutHorizontal;
		}
	}

	private class Data
	{
		public WarhammerNodeLinkTraverser.State AnimationType;
	}

	[Serializable]
	private class ClipSetByHeight
	{
		public float TraverseHeight;

		[SerializeField]
		private ClipSet m_UpAnimationsSet;

		[SerializeField]
		private ClipSet m_DownAnimationsSet;

		[SerializeField]
		private float m_DownVerticalInDistance = 3.3f;

		[SerializeField]
		private float m_UpVerticalOutDistance = 3.3f;

		private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

		public ClipSet UpAnimationsSet => m_UpAnimationsSet;

		public ClipSet DownAnimationsSet => m_DownAnimationsSet;

		public float DownVerticalInDistance => m_DownVerticalInDistance;

		public float UpVerticalOutDistance => m_UpVerticalOutDistance;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				if (m_ClipWrappersHashSet == null)
				{
					m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
					m_ClipWrappersHashSet.AddRange(m_UpAnimationsSet.GetClips());
					m_ClipWrappersHashSet.AddRange(m_DownAnimationsSet.GetClips());
				}
				return m_ClipWrappersHashSet;
			}
		}
	}

	private const float DefaultSpeedScale = 1f;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	[FormerlySerializedAs("m_SlipSetsBy")]
	[SerializeField]
	private ClipSetByHeight[] m_SlipSetsByHeight = new ClipSetByHeight[0];

	[SerializeField]
	private AnimationClipWrapper m_HorizontalTraverse;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if (m_ClipWrappersHashSet == null)
			{
				m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
				ClipSetByHeight[] slipSetsByHeight = m_SlipSetsByHeight;
				foreach (ClipSetByHeight clipSetByHeight in slipSetsByHeight)
				{
					m_ClipWrappersHashSet.AddRange(clipSetByHeight.ClipWrappers);
				}
			}
			return m_ClipWrappersHashSet;
		}
	}

	public override UnitAnimationType Type => UnitAnimationType.TraverseNodeLink;

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		ClipSetByHeight clipSetByHeight = m_SlipSetsByHeight.FirstItem((ClipSetByHeight x) => Math.Abs(handle.Unit.MovementAgent.NodeLinkTraverser.TraverseHeight - x.TraverseHeight) < 0.5f) ?? m_SlipSetsByHeight.FirstItem();
		if (clipSetByHeight != null)
		{
			handle.Unit.MovementAgent.NodeLinkTraverser.OutUpVerticalClipDuration = clipSetByHeight.UpAnimationsSet.GetAnimationClip(WarhammerNodeLinkTraverser.State.TraverseOut)?.Length ?? 0f;
			handle.Unit.MovementAgent.NodeLinkTraverser.OutUpVerticalDistance = clipSetByHeight.UpVerticalOutDistance;
			handle.Unit.MovementAgent.NodeLinkTraverser.OutUpHorizontalClipDuration = clipSetByHeight.UpAnimationsSet.GetAnimationClip(WarhammerNodeLinkTraverser.State.TraverseUpHorizontalOut)?.Length ?? 0f;
			handle.Unit.MovementAgent.NodeLinkTraverser.InUpVerticalClipDuration = clipSetByHeight.UpAnimationsSet.GetAnimationClip(WarhammerNodeLinkTraverser.State.TraverseIn)?.Length ?? 0f;
			handle.Unit.MovementAgent.NodeLinkTraverser.InDownVerticalClipDuration = clipSetByHeight.DownAnimationsSet.GetAnimationClip(WarhammerNodeLinkTraverser.State.TraverseIn)?.Length ?? 0f;
			handle.Unit.MovementAgent.NodeLinkTraverser.InDownVerticalDistance = clipSetByHeight.DownVerticalInDistance;
			handle.Unit.MovementAgent.NodeLinkTraverser.InDownHorizontalClipDuration = clipSetByHeight.DownAnimationsSet.GetAnimationClip(WarhammerNodeLinkTraverser.State.TraverseDownHorizontalIn)?.Length ?? 0f;
			handle.Unit.MovementAgent.NodeLinkTraverser.OutDownVerticalClipDuration = clipSetByHeight.DownAnimationsSet.GetAnimationClip(WarhammerNodeLinkTraverser.State.TraverseOut)?.Length ?? 0f;
			handle.Unit.MovementAgent.NodeLinkTraverser.OutDownVerticalClipDuration = clipSetByHeight.DownAnimationsSet.GetAnimationClip(WarhammerNodeLinkTraverser.State.TraverseOut)?.Length ?? 0f;
			handle.Unit.MovementAgent.NodeLinkTraverser.OnlyHorizontalTraverseTime = m_HorizontalTraverse?.Length ?? 0f;
			WarhammerNodeLinkTraverser nodeLinkTraverser = handle.Unit.MovementAgent.NodeLinkTraverser;
			float verticalSpeed = (handle.Unit.MovementAgent.NodeLinkTraverser.VerticalSpeed = (handle.Unit.MovementAgent.NodeLinkTraverser.IsUpTraverse ? clipSetByHeight.UpAnimationsSet.VerticalSpeed : clipSetByHeight.DownAnimationsSet.VerticalSpeed));
			nodeLinkTraverser.VerticalSpeed = verticalSpeed;
			handle.HasCrossfadePriority = true;
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		WarhammerNodeLinkTraverser.State lastState = handle.Unit.MovementAgent.NodeLinkTraverser.LastState;
		if (handle.ActionData is Data data && data.AnimationType == lastState)
		{
			AnimationBase activeAnimation = handle.ActiveAnimation;
			if (activeAnimation == null || activeAnimation.State == AnimationState.Finished)
			{
				handle.Unit.MovementAgent.NodeLinkTraverser.ForceNextState = true;
			}
		}
		else if (handle.Unit.MovementAgent.NodeLinkTraverser.LastState.IsTraverseState())
		{
			StartClip(handle, lastState);
		}
		else
		{
			handle.Release();
		}
	}

	private void StartClip(UnitAnimationActionHandle handle, WarhammerNodeLinkTraverser.State animationType)
	{
		handle.ActionData = new Data
		{
			AnimationType = animationType
		};
		ClipSetByHeight clipSetByHeight = m_SlipSetsByHeight.FirstItem((ClipSetByHeight x) => Math.Abs(handle.Unit.MovementAgent.NodeLinkTraverser.TraverseHeight - x.TraverseHeight) < 0.5f) ?? m_SlipSetsByHeight.FirstItem();
		if (clipSetByHeight == null)
		{
			return;
		}
		ClipSet clipSet = (handle.Unit.MovementAgent.NodeLinkTraverser.IsUpTraverse ? clipSetByHeight.UpAnimationsSet : clipSetByHeight.DownAnimationsSet);
		AnimationClipWrapper animationClipWrapper = (handle.Unit.MovementAgent.NodeLinkTraverser.IsOnlyHorizontalTraverse ? m_HorizontalTraverse : clipSet.GetAnimationClip(animationType));
		float speedScale = 1f;
		ClipDurationType duration = ClipDurationType.Endless;
		switch (animationType)
		{
		case WarhammerNodeLinkTraverser.State.Traverse:
			if (handle.Unit.MovementAgent.NodeLinkTraverser.IsOnlyHorizontalTraverse)
			{
				duration = ClipDurationType.Endless;
				float num = handle.Unit.MovementAgent.NodeLinkTraverser.TraverseDistance / handle.Unit.MovementAgent.NodeLinkTraverser.GetTraverseSpeedMps();
				speedScale = animationClipWrapper.Length / num;
			}
			else
			{
				duration = ClipDurationType.Endless;
				float num2 = handle.Unit.MovementAgent.NodeLinkTraverser.TraverseHeight / handle.Unit.MovementAgent.NodeLinkTraverser.GetTraverseSpeedMps();
				speedScale = animationClipWrapper.Length / num2;
			}
			break;
		case WarhammerNodeLinkTraverser.State.TraverseOut:
			duration = ClipDurationType.Endless;
			break;
		case WarhammerNodeLinkTraverser.State.TraverseUpHorizontalOut:
			duration = ClipDurationType.Endless;
			break;
		case WarhammerNodeLinkTraverser.State.TraverseIn:
			duration = ClipDurationType.Endless;
			break;
		case WarhammerNodeLinkTraverser.State.TraverseDownHorizontalIn:
			duration = ClipDurationType.Endless;
			break;
		default:
			if (animationClipWrapper.Length > handle.Unit.MovementAgent.NodeLinkTraverser.GetStateDuration())
			{
				speedScale = animationClipWrapper.Length / handle.Unit.MovementAgent.NodeLinkTraverser.GetStateDuration();
			}
			break;
		}
		handle.SpeedScale = speedScale;
		if (animationClipWrapper != null)
		{
			handle.StartClip(animationClipWrapper, duration);
		}
	}
}
