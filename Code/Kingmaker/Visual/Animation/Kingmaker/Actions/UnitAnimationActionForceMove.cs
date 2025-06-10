using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionForceMove : UnitAnimationAction
{
	private enum State
	{
		Fly,
		StandUp
	}

	private class ActionData
	{
		public State State;
	}

	[Serializable]
	public class RangeSettings
	{
		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		private AnimationClipWrapper m_ClipOneCell;

		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		private AnimationClipWrapper m_ClipTwoCell;

		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		private AnimationClipWrapper m_ClipThreeCell;

		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		private AnimationClipWrapper m_ClipFourCell;

		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		private AnimationClipWrapper m_ClipFiveCell;

		[AssetPicker("")]
		[SerializeField]
		[ValidateNotNull]
		private AnimationClipWrapper m_StandUp;

		public AnimationClipWrapper StandUp => m_StandUp;

		public IEnumerable<AnimationClipWrapper> Clips
		{
			get
			{
				yield return m_ClipOneCell;
				yield return m_ClipTwoCell;
				yield return m_ClipThreeCell;
				yield return m_ClipFourCell;
				yield return m_ClipFiveCell;
				yield return m_StandUp;
			}
		}

		public AnimationClipWrapper GetClipByRange(int range, UnitAnimationActionForceMove owner)
		{
			if (m_ClipOneCell == null && m_ClipTwoCell == null && m_ClipThreeCell == null && m_ClipFourCell == null && m_ClipFiveCell == null)
			{
				return null;
			}
			switch (range)
			{
			case 1:
				if (!m_ClipOneCell)
				{
					return GetClipByRange(2, owner);
				}
				return m_ClipOneCell;
			case 2:
				if (!m_ClipTwoCell)
				{
					return GetClipByRange(3, owner);
				}
				return m_ClipTwoCell;
			case 3:
				if (!m_ClipThreeCell)
				{
					return GetClipByRange(4, owner);
				}
				return m_ClipThreeCell;
			case 4:
				if (!m_ClipFourCell)
				{
					return GetClipByRange(5, owner);
				}
				return m_ClipFourCell;
			case 5:
				if (!m_ClipFiveCell)
				{
					return GetClipByRange(1, owner);
				}
				return m_ClipFiveCell;
			default:
				LogChannel.Default.Error(owner, "Undefined range in force move action");
				return null;
			}
		}
	}

	[SerializeField]
	private RangeSettings m_RangeSettings;

	private float[] m_ClipsLengths;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_RangeSettings.Clips;

	public override UnitAnimationType Type => UnitAnimationType.ForceMove;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		ActionData actionData = new ActionData
		{
			State = State.Fly
		};
		handle.ActionData = actionData;
		int range = handle.Unit.Data.GetOptional<UnitPartForceMove>()?.Active?.DistanceCells ?? 1;
		handle.StartClip(GetClipByRange(range), ClipDurationType.Endless);
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (!(handle.ActionData is ActionData actionData))
		{
			handle.Release();
		}
		else if (!handle.Manager.IsForceMove && actionData.State == State.Fly && !handle.IsInterrupted)
		{
			actionData.State = State.StandUp;
			if (m_RangeSettings.StandUp == null)
			{
				handle.Release();
			}
			else
			{
				handle.StartClip(m_RangeSettings.StandUp, ClipDurationType.Oneshot);
			}
		}
		else if (handle.ActiveAnimation == null)
		{
			handle.Release();
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public float[] GetClipsLength()
	{
		if (m_ClipsLengths != null && m_ClipsLengths.Length != 0)
		{
			return m_ClipsLengths;
		}
		if (GetClipByRange(1) != null)
		{
			m_ClipsLengths = new float[6];
			for (int i = 1; i <= 5; i++)
			{
				m_ClipsLengths[i] = GetClipByRange(i).Length;
			}
			m_ClipsLengths[0] = m_ClipsLengths[1];
		}
		return m_ClipsLengths;
	}

	public AnimationClipWrapper GetClipByRange(int range)
	{
		return m_RangeSettings?.GetClipByRange(range, this);
	}
}
