using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "Reload", menuName = "Animation Manager/Actions/Reload")]
public class UnitAnimationActionReload : UnitAnimationAction
{
	[Serializable]
	public class WeaponStyleSettings
	{
		[SerializeField]
		private WeaponAnimationStyle m_Style;

		public bool IsOffHand;

		[AssetPicker("")]
		[SerializeField]
		private AnimationClipWrapper m_Clip;

		public WeaponAnimationStyle Style => m_Style;

		public AnimationClipWrapper Clip => m_Clip;
	}

	[AssetPicker("")]
	[SerializeField]
	private AnimationClipWrapper m_DefaultClip;

	[SerializeField]
	private WeaponStyleSettings[] m_StyleSettings = Array.Empty<WeaponStyleSettings>();

	public override UnitAnimationType Type => UnitAnimationType.Reload;

	public override bool IsAdditive => false;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			yield return m_DefaultClip;
			WeaponStyleSettings[] styleSettings = m_StyleSettings;
			foreach (WeaponStyleSettings weaponStyleSettings in styleSettings)
			{
				yield return weaponStyleSettings.Clip;
			}
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper animationClipWrapper = m_StyleSettings.FirstOrDefault((WeaponStyleSettings v) => v.Style == handle.AttackWeaponStyle && v.IsOffHand == handle.CastInOffhand)?.Clip;
		if (!animationClipWrapper)
		{
			animationClipWrapper = m_StyleSettings.FirstOrDefault((WeaponStyleSettings v) => v.Style == handle.AttackWeaponStyle)?.Clip;
		}
		if (!animationClipWrapper)
		{
			animationClipWrapper = m_DefaultClip;
		}
		handle.StartClip(animationClipWrapper);
	}
}
