using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationActionCastSpell", menuName = "Animation Manager/Actions/Unit Cast Spell (WH)")]
public class WarhammerUnitAnimationActionCastSpell : UnitAnimationAction, IUnitAnimationActionHasVariants
{
	public class Data
	{
		public readonly List<ClipSettings> Sequence = new List<ClipSettings>();

		public int Index { get; set; } = -1;


		public bool Invalid { get; set; }
	}

	[Serializable]
	public class ClipSettings
	{
		[AssetPicker("")]
		[ValidateNotNull]
		[DrawEventWarning]
		public AnimationClipWrapper ClipWrapper;

		public bool IsValid => ClipWrapper?.AnimationClip != null;
	}

	[Serializable]
	public class AttackVariantSettings
	{
		public List<ClipSettings> Ready = new List<ClipSettings>();

		public List<ClipSettings> Unready = new List<ClipSettings>();

		public List<ClipSettings> Attack = new List<ClipSettings>();

		public IEnumerable<AnimationClipWrapper> ClipWrappers => Unpack((ClipSettings i) => i.ClipWrapper, Ready, Unready, Attack).Distinct();

		public IEnumerable<AnimationClipWrapper> AttackClipWrappers => Unpack((ClipSettings i) => i.ClipWrapper, Attack).Distinct();

		private IEnumerable<T> Unpack<T>(Func<ClipSettings, T> unpack, params List<ClipSettings>[] settingsLists)
		{
			foreach (List<ClipSettings> list in settingsLists)
			{
				foreach (ClipSettings item in list)
				{
					yield return unpack(item);
				}
			}
		}

		[CanBeNull]
		public ClipSettings GetReadyVariantRandom(UnitAnimationActionHandle handle)
		{
			return Ready.Random(PFStatefulRandom.Visuals.Animation);
		}

		[CanBeNull]
		public ClipSettings GetUnreadyVariantRandom(UnitAnimationActionHandle handle)
		{
			return Unready.Random(PFStatefulRandom.Visuals.Animation);
		}

		[CanBeNull]
		public ClipSettings GetAttackVariantRandom(UnitAnimationActionHandle handle)
		{
			return Attack.Random(PFStatefulRandom.Visuals.Animation);
		}
	}

	[Serializable]
	public class WeaponStyleSettings
	{
		public WeaponAnimationStyle Style;

		[ValidateNotEmpty]
		public AttackVariantSettings Single = new AttackVariantSettings();

		public AttackVariantSettings Burst = new AttackVariantSettings();

		public IEnumerable<AnimationClipWrapper> ClipWrappers => Single.ClipWrappers.Concat(Burst.ClipWrappers);

		[CanBeNull]
		private ClipSettings GetAnimationVariant(UnitAnimationActionHandle handle, bool required, Func<UnitAnimationActionHandle, AttackVariantSettings, ClipSettings> getter, string name)
		{
			AttackVariantSettings arg = ((handle.IsBurst || handle.NeedPreparingForShooting) ? Burst : Single);
			ClipSettings clipSettings = getter(handle, arg);
			if (clipSettings == null && required)
			{
				string text = ((handle.IsBurst || handle.NeedPreparingForShooting) ? "Burst" : "Single");
				PFLog.Default.Error(handle.Action, $"Has no {name} weapon animation for {text}: {handle.Action.name} ({Style})");
				arg = ((handle.IsBurst || handle.NeedPreparingForShooting) ? Single : Burst);
				clipSettings = getter(handle, arg);
			}
			if (clipSettings == null && required)
			{
				PFLog.Default.Error(handle.Action, $"Has no {name} weapon animation: {handle.Action.name} ({Style})");
			}
			return clipSettings;
		}

		[CanBeNull]
		public ClipSettings GetAttackVariant(UnitAnimationActionHandle handle)
		{
			return GetAnimationVariant(handle, required: true, (UnitAnimationActionHandle h, AttackVariantSettings s) => s.GetAttackVariantRandom(h), "Attack");
		}

		[CanBeNull]
		public ClipSettings GetReadyVariant(UnitAnimationActionHandle handle)
		{
			return GetAnimationVariant(handle, handle.IsBurst || handle.NeedPreparingForShooting, (UnitAnimationActionHandle h, AttackVariantSettings s) => s.GetReadyVariantRandom(h), "Ready");
		}

		[CanBeNull]
		public ClipSettings GetUnreadyVariant(UnitAnimationActionHandle handle)
		{
			return GetAnimationVariant(handle, handle.IsBurst || handle.NeedPreparingForShooting, (UnitAnimationActionHandle h, AttackVariantSettings s) => s.GetUnreadyVariantRandom(h), "Unready");
		}
	}

	[Serializable]
	public class AnimationStyleEntry
	{
		public UnitAnimationActionCastSpell.CastAnimationStyle Style;

		public bool IsOffHand;

		public WeaponStyleSettings Default;

		public List<WeaponStyleSettings> WeaponStyleSettings;
	}

	[SerializeField]
	private List<AnimationStyleEntry> m_Animations = new List<AnimationStyleEntry>();

	public override UnitAnimationType Type => UnitAnimationType.CastSpell;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Animations.SelectMany((AnimationStyleEntry entry) => entry.WeaponStyleSettings.SelectMany((WeaponStyleSettings i) => i.ClipWrappers).Concat(entry.Default.ClipWrappers));

	private static Data GetData(UnitAnimationActionHandle handle)
	{
		return (Data)handle.ActionData;
	}

	private ClipSettings GetAttackVariant(UnitAnimationActionHandle handle)
	{
		return GetAnimationVariant(handle)?.GetAttackVariant(handle);
	}

	private ClipSettings GetReadyVariant(UnitAnimationActionHandle handle)
	{
		return GetAnimationVariant(handle)?.GetReadyVariant(handle);
	}

	private ClipSettings GetUnreadyVariant(UnitAnimationActionHandle handle)
	{
		return GetAnimationVariant(handle)?.GetUnreadyVariant(handle);
	}

	private WeaponStyleSettings GetAnimationVariant(UnitAnimationActionHandle handle)
	{
		AnimationStyleEntry animationStyleEntry = m_Animations.FirstOrDefault((AnimationStyleEntry i) => i.Style == handle.CastStyle && i.IsOffHand == handle.CastInOffhand);
		WeaponStyleSettings weaponStyleSettings = animationStyleEntry?.WeaponStyleSettings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.AttackWeaponStyle);
		if (weaponStyleSettings == null || !handle.Manager.IsInCombat)
		{
			weaponStyleSettings = animationStyleEntry?.Default;
		}
		if (weaponStyleSettings == null)
		{
			PFLog.Default.Error(this, $"No animation for spell cast style {handle.CastStyle} weapon style '{handle.AttackWeaponStyle}' in action '{base.name}'");
		}
		return weaponStyleSettings;
	}

	public int GetVariantsCount(UnitAnimationActionHandle handle)
	{
		WeaponStyleSettings animationVariant = GetAnimationVariant(handle);
		if (animationVariant == null)
		{
			return 0;
		}
		int count = animationVariant.Burst.Attack.Count;
		if (handle.IsBurst && count > 0)
		{
			return count;
		}
		return animationVariant.Single.Attack.Count;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		Data data2 = (Data)(handle.ActionData = new Data());
		if (handle.IsBurst || (handle.NeedPreparingForShooting && handle.IsPreparingForShooting))
		{
			ClipSettings readyVariant = GetReadyVariant(handle);
			if (readyVariant != null)
			{
				if (!readyVariant.IsValid)
				{
					data2.Invalid = true;
					return;
				}
				data2.Sequence.Add(readyVariant);
			}
		}
		if (!handle.IsPreparingForShooting)
		{
			int num = ((!handle.IsBurst) ? 1 : handle.BurstCount);
			for (int i = 0; i < num; i++)
			{
				ClipSettings attackVariant = GetAttackVariant(handle);
				if (attackVariant?.ClipWrapper?.AnimationClip == null)
				{
					data2.Invalid = true;
					return;
				}
				data2.Sequence.Add(attackVariant);
			}
			if (handle.IsBurst || (handle.NeedPreparingForShooting && handle.IsPreparingForShooting))
			{
				ClipSettings unreadyVariant = GetUnreadyVariant(handle);
				if (unreadyVariant != null)
				{
					if (!unreadyVariant.IsValid)
					{
						data2.Invalid = true;
						return;
					}
					data2.Sequence.Add(unreadyVariant);
				}
			}
		}
		if ((!handle.NeedPreparingForShooting || handle.IsPreparingForShooting) && !Next(handle))
		{
			handle.Release();
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (GetData(handle).Invalid)
		{
			UpdateInvalid(handle);
		}
		else
		{
			Update(handle);
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		Data data = GetData(handle);
		if (!data.Invalid && data.Index >= data.Sequence.Count)
		{
			base.OnTransitionOutStarted(handle);
		}
	}

	private static void Update(UnitAnimationActionHandle handle)
	{
		if (handle.ActiveAnimation != null && !(handle.GetTime() < GetNextClipStartTime(handle)) && (!handle.NeedPreparingForShooting || !handle.IsPreparingForShooting) && (!handle.NeedPreparingForShooting || !(Math.Abs(handle.Manager.Orientation - handle.Manager.UseAbilityDirection) > 10f)) && !Next(handle))
		{
			handle.Release();
		}
	}

	private static bool Next(UnitAnimationActionHandle handle)
	{
		Data data = GetData(handle);
		ClipSettings clipSettings = data.Sequence.Get(data.Index + 1);
		if (clipSettings == null)
		{
			return false;
		}
		data.Index++;
		StartClip(handle, clipSettings);
		return true;
	}

	private static void StartClip(UnitAnimationActionHandle handle, ClipSettings settings)
	{
		handle.StartClip(settings.ClipWrapper, ClipDurationType.Endless);
	}

	private static float GetNextClipStartTime(UnitAnimationActionHandle handle)
	{
		Data data = GetData(handle);
		float num = 0f;
		for (int i = 0; i <= data.Index; i++)
		{
			num = ((data.Sequence.Count <= 2 || i <= 0 || i >= data.Sequence.Count - 2) ? (num + data.Sequence[i].ClipWrapper.Length) : (num + (data.Sequence[i].ClipWrapper.Length + handle.BurstAnimationDelay)));
		}
		return num;
	}

	private static void UpdateInvalid(UnitAnimationActionHandle handle)
	{
		int num = ((!handle.IsBurst) ? 1 : handle.BurstCount);
		float time = handle.GetTime();
		int actEventsCounter = Math.Clamp(Mathf.FloorToInt((time - 0.4f) / 0.3f), 0, num);
		handle.ActEventsCounter = actEventsCounter;
		if (time >= 0.8f + 0.3f * (float)num)
		{
			handle.Release();
		}
	}
}
