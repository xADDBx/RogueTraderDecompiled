using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationActionParry", menuName = "Animation Manager/Actions/Unit Parry (WH)")]
public class WarhammerUnitAnimationActionParry : UnitAnimationAction
{
	public class Data
	{
		public WeaponStyleSettings Settings;
	}

	[Serializable]
	public class WeaponStyleSettings
	{
		public WeaponAnimationStyle Style;

		public bool IsOffHand;

		public AnimationClipWrapper Start;

		[HideInInspector]
		public AnimationClipWrapper Middle;

		[HideInInspector]
		public AnimationClipWrapper End;

		private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				if (m_ClipWrappersHashSet != null)
				{
					return m_ClipWrappersHashSet;
				}
				m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
				m_ClipWrappersHashSet.Add(Start);
				m_ClipWrappersHashSet.Add(Middle);
				m_ClipWrappersHashSet.Add(End);
				return m_ClipWrappersHashSet;
			}
		}
	}

	[SerializeField]
	private List<WeaponStyleSettings> m_Settings = new List<WeaponStyleSettings>();

	public List<WeaponStyleSettings> Settings => m_Settings;

	public override UnitAnimationType Type => UnitAnimationType.Parry;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Settings.SelectMany((WeaponStyleSettings setting) => setting.ClipWrappers);

	public override bool BlocksCover => true;

	private static Data GetData(UnitAnimationActionHandle handle)
	{
		return (Data)handle.ActionData;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		Data data = new Data();
		handle.ActionData = data;
		WeaponStyleSettings weaponStyleSettings = Settings.FirstOrDefault((WeaponStyleSettings x) => x.Style == (handle.CastInOffhand ? handle.Manager.ActiveOffHandWeaponStyle : handle.Manager.ActiveMainHandWeaponStyle) && handle.CastInOffhand == x.IsOffHand);
		if (weaponStyleSettings == null)
		{
			weaponStyleSettings = Settings.FirstOrDefault((WeaponStyleSettings x) => x.Style == handle.Manager.ActiveMainHandWeaponStyle);
		}
		if (weaponStyleSettings == null)
		{
			weaponStyleSettings = Settings.FirstOrDefault((WeaponStyleSettings x) => x.Style == handle.Manager.ActiveOffHandWeaponStyle);
		}
		if (weaponStyleSettings == null)
		{
			handle.Release();
			return;
		}
		data.Settings = weaponStyleSettings;
		handle.StartClip(data.Settings.Start, ClipDurationType.Oneshot);
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		Data data = GetData(handle);
		if (handle.GetTime() > data.Settings.Start.Length * 3f)
		{
			handle.Release();
		}
		else if (!(handle.GetTime() < data.Settings.Start.Length))
		{
			handle.Release();
		}
	}
}
