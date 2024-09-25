using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationActionDialog : UnitAnimationAction, IAddInspectorGUI
{
	[SerializeField]
	[HideInInspector]
	private AnimationClipWrapper[] m_ClipWrappers = new AnimationClipWrapper[0];

	[SerializeField]
	[AddInspector]
	private bool m_Dummy;

	public override UnitAnimationType Type => UnitAnimationType.Dialog;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_ClipWrappers;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper animationClipWrapper = m_ClipWrappers.ElementAtOrDefault(handle.Variant);
		if ((bool)animationClipWrapper)
		{
			handle.StartClip(animationClipWrapper);
		}
		else
		{
			handle.Release();
		}
	}

	public void SetClip(DialogAnimation type, AnimationClipWrapper clip)
	{
		if (m_ClipWrappers == null)
		{
			m_ClipWrappers = new AnimationClipWrapper[Enum.GetValues(typeof(DialogAnimation)).Length];
		}
		m_ClipWrappers[(int)type] = clip;
	}
}
