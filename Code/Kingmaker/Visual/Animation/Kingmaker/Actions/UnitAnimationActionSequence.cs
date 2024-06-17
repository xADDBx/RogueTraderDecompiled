using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationSequence", menuName = "Animation Manager/Actions/Clip Sequence")]
public class UnitAnimationActionSequence : UnitAnimationAction
{
	[Serializable]
	public class Entry
	{
		[AssetPicker("")]
		[ValidateNotNull]
		public AnimationClipWrapper ClipWrapper;

		public float OverrideDuration;

		public float SpeedCoeff = 1f;

		public float BlendOutTime;
	}

	private class HandleData
	{
		public int CurrentClipIndex;
	}

	[SerializeField]
	[ValidateNotEmpty]
	private List<Entry> m_Clips;

	public override UnitAnimationType Type => UnitAnimationType.None;

	public override bool SupportCaching => false;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Clips.Select((Entry e) => e.ClipWrapper).Distinct();

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		HandleData actionData = new HandleData();
		handle.ActionData = actionData;
		StartNextClip(handle, 0);
	}

	private void StartNextClip(UnitAnimationActionHandle handle, int idx)
	{
		Entry entry = m_Clips[idx];
		handle.StartClip(entry.ClipWrapper);
		handle.ActiveAnimation.SetSpeed(entry.SpeedCoeff);
		if (entry.OverrideDuration > 0f)
		{
			handle.ActiveAnimation.OverrideDuration(entry.OverrideDuration);
		}
		handle.ActiveAnimation.TransitionOut = entry.BlendOutTime;
		handle.ActiveAnimation.TransitionOutStartTime = handle.ActiveAnimation.GetDuration() - entry.BlendOutTime;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		HandleData handleData = (HandleData)handle.ActionData;
		handleData.CurrentClipIndex++;
		if (handleData.CurrentClipIndex >= m_Clips.Count)
		{
			base.OnTransitionOutStarted(handle);
		}
		else
		{
			StartNextClip(handle, handleData.CurrentClipIndex);
		}
	}
}
