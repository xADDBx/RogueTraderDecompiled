using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationInteract", menuName = "Animation Manager/Actions/Unit Interact")]
public class UnitAnimationActionInteract : UnitAnimationAction
{
	[Serializable]
	public class Setting
	{
		public UnitAnimationInteractionType Type;

		[AssetPicker("")]
		[ValidateNotNull]
		public AnimationClipWrapper ClipWrapper;

		public void Validate(ValidationContext context)
		{
			if (Type == UnitAnimationInteractionType.None)
			{
				context.AddError("Invalid type: None");
			}
			if (!ClipWrapper)
			{
				context.AddError("Clip is not configured");
			}
		}
	}

	[SerializeField]
	private List<Setting> m_Settings = new List<Setting>();

	public List<Setting> Settings => m_Settings;

	public override UnitAnimationType Type => UnitAnimationType.Interact;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Settings.Select((Setting s) => s.ClipWrapper).NotNull();

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.StartClip(GetClipWrapper(handle), ClipDurationType.Oneshot);
	}

	public float GetDuration(UnitAnimationActionHandle handle)
	{
		return GetClipWrapper(handle).Length;
	}

	private AnimationClipWrapper GetClipWrapper(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper obj = m_Settings.FirstOrDefault((Setting s) => s.Type == handle.InteractionType)?.ClipWrapper;
		if (!obj)
		{
			throw new Exception($"Can't find animation for intaraction {handle.InteractionType}");
		}
		return obj;
	}

	public void AddToSettings(UnitAnimationInteractionType type, AnimationClipWrapper clipWrapper)
	{
		if (!m_Settings.Any((Setting x) => x.Type == type))
		{
			m_Settings.Add(new Setting
			{
				Type = type
			});
		}
		m_Settings.First((Setting x) => x.Type == type).ClipWrapper = clipWrapper;
	}
}
