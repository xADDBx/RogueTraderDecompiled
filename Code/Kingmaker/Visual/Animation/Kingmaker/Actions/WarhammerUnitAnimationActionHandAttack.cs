using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Mechadendrites;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationActionHandAttack", menuName = "Animation Manager/Actions/Unit Hand Attack (WH)")]
public class WarhammerUnitAnimationActionHandAttack : UnitAnimationAction, IUnitAnimationActionHasVariants
{
	public class Data
	{
		public readonly List<ClipSettings> Sequence = new List<ClipSettings>();

		public AnimationClipWrapper OffHandWrapper;

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

		public List<ClipSettings> LowRecoil = new List<ClipSettings>();

		public List<ClipSettings> MediumRecoil = new List<ClipSettings>();

		public List<ClipSettings> HighRecoil = new List<ClipSettings>();

		public List<ClipSettings> PlasmaRecoil = new List<ClipSettings>();

		public List<ClipSettings> LinearFlamerRecoil = new List<ClipSettings>();

		public List<ClipSettings> CornerFlamerRecoil = new List<ClipSettings>();

		public List<ClipSettings> LaserRecoil = new List<ClipSettings>();

		public List<ClipSettings> MeleeCornerAttack = new List<ClipSettings>();

		public bool UseStrictAnimationOrderInsteadOfRandom;

		[Space(4f)]
		public List<AlternativeAttackVariantSettings> AlternativeAnimations = new List<AlternativeAttackVariantSettings>();

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				IEnumerable<AnimationClipWrapper> enumerable = Unpack((ClipSettings i) => i.ClipWrapper, Ready, Unready, LowRecoil, MediumRecoil, HighRecoil, PlasmaRecoil, LaserRecoil, LinearFlamerRecoil, CornerFlamerRecoil, MeleeCornerAttack).Distinct();
				if (AlternativeAnimations.Empty())
				{
					return enumerable;
				}
				foreach (AlternativeAttackVariantSettings alternativeAnimation in AlternativeAnimations)
				{
					enumerable.Concat(alternativeAnimation.Settings.ClipWrappers);
				}
				return enumerable.Distinct();
			}
		}

		public IEnumerable<AnimationClipWrapper> AttackClipWrappers
		{
			get
			{
				IEnumerable<AnimationClipWrapper> enumerable = Unpack((ClipSettings i) => i.ClipWrapper, LowRecoil, MediumRecoil, HighRecoil, PlasmaRecoil, LinearFlamerRecoil, LaserRecoil, CornerFlamerRecoil, MeleeCornerAttack).Distinct();
				if (AlternativeAnimations.Empty())
				{
					return enumerable;
				}
				foreach (AlternativeAttackVariantSettings alternativeAnimation in AlternativeAnimations)
				{
					enumerable.Concat(alternativeAnimation.Settings.AttackClipWrappers);
				}
				return enumerable.Distinct();
			}
		}

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

		public ReadonlyList<ClipSettings> GetAttackVariantsList(UnitAnimationActionHandle handle)
		{
			if (handle.AlternativeStyle != 0)
			{
				if (!AlternativeAnimations.Empty())
				{
					return AlternativeAnimations.First((AlternativeAttackVariantSettings alt) => alt.Style == handle.AlternativeStyle)?.Settings?.GetAttackVariantsList(handle) ?? GetAttackVariantsList(handle.Recoil);
				}
				return GetAttackVariantsList(handle.Recoil);
			}
			return GetAttackVariantsList(handle.Recoil);
		}

		public ReadonlyList<ClipSettings> GetAttackVariantsList(RecoilStrength recoil)
		{
			return recoil switch
			{
				RecoilStrength.Low => LowRecoil, 
				RecoilStrength.Medium => MediumRecoil, 
				RecoilStrength.High => HighRecoil, 
				RecoilStrength.Plasma => PlasmaRecoil, 
				RecoilStrength.CornerFlamer => CornerFlamerRecoil, 
				RecoilStrength.LinearFlamer => LinearFlamerRecoil, 
				RecoilStrength.Laser => LaserRecoil, 
				_ => throw new ArgumentOutOfRangeException("recoil", recoil, null), 
			};
		}

		[CanBeNull]
		public ClipSettings GetAttackVariantRandom(UnitAnimationActionHandle handle, int count = 0)
		{
			if (handle.AlternativeStyle != 0 && !AlternativeAnimations.Empty() && AlternativeAnimations.Any((AlternativeAttackVariantSettings alt) => alt.Style == handle.AlternativeStyle))
			{
				ClipSettings attackVariantRandom = AlternativeAnimations.First((AlternativeAttackVariantSettings alt) => alt.Style == handle.AlternativeStyle).Settings.GetAttackVariantRandom(handle, count);
				if (attackVariantRandom != null)
				{
					return attackVariantRandom;
				}
			}
			RecoilStrength recoil = handle.Recoil;
			ClipSettings clipSettings = (UseStrictAnimationOrderInsteadOfRandom ? GetAttackVariantsList(recoil)[count % GetAttackVariantsList(recoil).Count] : GetAttackVariantsList(recoil).Random(PFStatefulRandom.Visuals.Animation1));
			if (clipSettings == null)
			{
				clipSettings = recoil switch
				{
					RecoilStrength.Low => Randomize(RecoilStrength.Medium) ?? Randomize(RecoilStrength.High), 
					RecoilStrength.Medium => Randomize(RecoilStrength.Low) ?? Randomize(RecoilStrength.High), 
					RecoilStrength.High => Randomize(RecoilStrength.Medium) ?? Randomize(RecoilStrength.Low), 
					RecoilStrength.Plasma => Randomize(RecoilStrength.Low) ?? Randomize(RecoilStrength.Medium) ?? Randomize(RecoilStrength.High), 
					RecoilStrength.LinearFlamer => Randomize(RecoilStrength.Low) ?? Randomize(RecoilStrength.Medium) ?? Randomize(RecoilStrength.High), 
					RecoilStrength.CornerFlamer => Randomize(RecoilStrength.Low) ?? Randomize(RecoilStrength.Medium) ?? Randomize(RecoilStrength.High), 
					RecoilStrength.Laser => Randomize(RecoilStrength.Low) ?? Randomize(RecoilStrength.Medium) ?? Randomize(RecoilStrength.High), 
					_ => throw new ArgumentOutOfRangeException("recoil", recoil, null), 
				};
			}
			if (handle.IsCornerAttack && MeleeCornerAttack.Count > 0)
			{
				clipSettings = (UseStrictAnimationOrderInsteadOfRandom ? MeleeCornerAttack[count % MeleeCornerAttack.Count] : MeleeCornerAttack.Random(PFStatefulRandom.Visuals.Animation1));
			}
			return clipSettings;
			[CanBeNull]
			ClipSettings Randomize(RecoilStrength r)
			{
				return GetAttackVariantsList(r).Random(PFStatefulRandom.Visuals.Animation1);
			}
		}

		[CanBeNull]
		public ClipSettings GetReadyVariantRandom(UnitAnimationActionHandle handle)
		{
			object obj;
			if (handle.AlternativeStyle != 0)
			{
				if (AlternativeAnimations.Empty())
				{
					return Ready.Random(PFStatefulRandom.Visuals.Animation1);
				}
				obj = AlternativeAnimations.First((AlternativeAttackVariantSettings alt) => alt.Style == handle.AlternativeStyle)?.Settings?.GetReadyVariantRandom(handle);
				if (obj == null)
				{
					return Ready.Random(PFStatefulRandom.Visuals.Animation1);
				}
			}
			else
			{
				obj = Ready.Random(PFStatefulRandom.Visuals.Animation1);
			}
			return (ClipSettings)obj;
		}

		[CanBeNull]
		public ClipSettings GetUnreadyVariantRandom(UnitAnimationActionHandle handle, int count = 0)
		{
			object obj;
			if (handle.AlternativeStyle != 0)
			{
				if (AlternativeAnimations.Empty())
				{
					if (!UseStrictAnimationOrderInsteadOfRandom)
					{
						return Unready.Random(PFStatefulRandom.Visuals.Animation1);
					}
					return Unready[count % Unready.Count];
				}
				obj = AlternativeAnimations.First((AlternativeAttackVariantSettings alt) => alt.Style == handle.AlternativeStyle)?.Settings?.GetUnreadyVariantRandom(handle, count);
				if (obj == null)
				{
					if (!UseStrictAnimationOrderInsteadOfRandom)
					{
						return Unready.Random(PFStatefulRandom.Visuals.Animation1);
					}
					return Unready[count % Unready.Count];
				}
			}
			else
			{
				if (!UseStrictAnimationOrderInsteadOfRandom)
				{
					return Unready.Random(PFStatefulRandom.Visuals.Animation1);
				}
				obj = Unready[count % Unready.Count];
			}
			return (ClipSettings)obj;
		}
	}

	[Serializable]
	public class AlternativeAttackVariantSettings
	{
		public AnimationAlternativeStyle Style;

		[Space(4f)]
		public AttackVariantSettings Settings;
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
			AttackVariantSettings arg = ((handle.IsBurst || (handle.Manager.NeedStepOut && handle.Manager.StepOutDirectionAnimationType != 0) || handle.NeedPreparingForShooting) ? Burst : Single);
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
		public ClipSettings GetAttackVariant(UnitAnimationActionHandle handle, int count)
		{
			return GetAnimationVariant(handle, required: true, (UnitAnimationActionHandle h, AttackVariantSettings s) => s.GetAttackVariantRandom(h, count), "Attack");
		}

		[CanBeNull]
		public ClipSettings GetReadyVariant(UnitAnimationActionHandle handle)
		{
			return GetAnimationVariant(handle, handle.IsBurst || handle.NeedPreparingForShooting, (UnitAnimationActionHandle h, AttackVariantSettings s) => s.GetReadyVariantRandom(h), "Ready");
		}

		[CanBeNull]
		public ClipSettings GetUnreadyVariant(UnitAnimationActionHandle handle, int count)
		{
			return GetAnimationVariant(handle, handle.IsBurst || handle.NeedPreparingForShooting, (UnitAnimationActionHandle h, AttackVariantSettings s) => s.GetUnreadyVariantRandom(h, count), "Unready");
		}
	}

	[Serializable]
	private class OffHandAnimationSettings
	{
		public WeaponAnimationStyle Style;

		[AssetPicker("")]
		[ValidateNotNull]
		[DrawEventWarning]
		public AnimationClipWrapper ClipWrapper;
	}

	[SerializeField]
	private bool m_IsMainHand;

	[SerializeField]
	private List<WeaponStyleSettings> m_Settings = new List<WeaponStyleSettings>();

	[SerializeField]
	private List<OffHandAnimationSettings> m_OffHandSettings = new List<OffHandAnimationSettings>();

	[SerializeField]
	private AvatarMask m_OffHandMask;

	public List<WeaponStyleSettings> Settings => m_Settings;

	public override UnitAnimationType Type
	{
		get
		{
			if (!m_IsMainHand)
			{
				return UnitAnimationType.OffHandAttack;
			}
			return UnitAnimationType.MainHandAttack;
		}
	}

	public bool IsMainHand
	{
		get
		{
			return m_IsMainHand;
		}
		set
		{
			m_IsMainHand = value;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Settings.SelectMany((WeaponStyleSettings i) => i.ClipWrappers).Concat(from i in m_OffHandSettings.EmptyIfNull()
		select i.ClipWrapper);

	private static Data GetData(UnitAnimationActionHandle handle)
	{
		return (Data)handle.ActionData;
	}

	private ClipSettings GetAttackVariant(UnitAnimationActionHandle handle, int count = 0)
	{
		WeaponStyleSettings weaponStyleSettings = m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.AttackWeaponStyle);
		if (weaponStyleSettings == null)
		{
			PFLog.Default.Error(this, $"No animation for weapon style '{handle.AttackWeaponStyle}' in action '{base.name}'");
		}
		return weaponStyleSettings?.GetAttackVariant(handle, count);
	}

	private ClipSettings GetReadyVariant(UnitAnimationActionHandle handle)
	{
		WeaponStyleSettings weaponStyleSettings = m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.AttackWeaponStyle);
		if (weaponStyleSettings == null)
		{
			PFLog.Default.Error(this, $"No animation for weapon style '{handle.AttackWeaponStyle}' in action '{base.name}'");
		}
		return weaponStyleSettings?.GetReadyVariant(handle);
	}

	private ClipSettings GetUnreadyVariant(UnitAnimationActionHandle handle, int count = 0)
	{
		WeaponStyleSettings weaponStyleSettings = m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.AttackWeaponStyle);
		if (weaponStyleSettings == null)
		{
			PFLog.Default.Error(this, $"No animation for weapon style '{handle.AttackWeaponStyle}' in action '{base.name}'");
		}
		return weaponStyleSettings?.GetUnreadyVariant(handle, count);
	}

	public int GetVariantsCount(UnitAnimationActionHandle handle)
	{
		WeaponStyleSettings weaponStyleSettings = m_Settings.FirstOrDefault((WeaponStyleSettings i) => i.Style == handle.AttackWeaponStyle);
		if (weaponStyleSettings == null)
		{
			return 0;
		}
		int count = weaponStyleSettings.Burst.GetAttackVariantsList(handle.Recoil).Count;
		if (handle.IsBurst && count > 0)
		{
			return count;
		}
		return weaponStyleSettings.Single.GetAttackVariantsList(handle.Recoil).Count;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.SkipFirstTick = false;
		Data data2 = (Data)(handle.ActionData = new Data());
		handle.SpeedScale = Game.CombatAnimSpeedUp;
		if ((handle.IsBurst && (!handle.Manager.NeedStepOut || handle.Manager.StepOutDirectionAnimationType == UnitAnimationActionCover.StepOutDirectionAnimationType.None)) || (handle.NeedPreparingForShooting && handle.IsPreparingForShooting))
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
			WeaponAnimationStyle attackWeaponStyle = handle.AttackWeaponStyle;
			int num = ((!handle.IsBurst) ? 1 : handle.BurstCount);
			for (int i = 0; i < num; i++)
			{
				ClipSettings clipSettings = GetAttackVariant(handle, i);
				if (handle.IsBladeDance)
				{
					if (i % 2 == 0)
					{
						handle.AttackWeaponStyle = handle.Manager.ActiveMainHandWeaponStyle;
						clipSettings = GetAttackVariant(handle, i);
					}
					else
					{
						handle.AttackWeaponStyle = handle.Manager.ActiveOffHandWeaponStyle;
						clipSettings = ((WarhammerUnitAnimationActionHandAttack)handle.Manager.GetAction(UnitAnimationType.OffHandAttack))?.GetAttackVariant(handle, i) ?? GetAttackVariant(handle, i);
					}
				}
				if (clipSettings?.ClipWrapper?.AnimationClip == null)
				{
					data2.Invalid = true;
					return;
				}
				data2.Sequence.Add(clipSettings);
			}
			handle.AttackWeaponStyle = attackWeaponStyle;
			if ((handle.IsBurst && (!handle.Manager.NeedStepOut || handle.Manager.StepOutDirectionAnimationType == UnitAnimationActionCover.StepOutDirectionAnimationType.None)) || (handle.NeedPreparingForShooting && handle.IsPreparingForShooting))
			{
				ClipSettings unreadyVariant = GetUnreadyVariant(handle, num - 1);
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
		if (handle.Manager.NeedStepOut && handle.Manager.StepOutDirectionAnimationType != 0)
		{
			data2.Invalid = true;
			return;
		}
		WeaponAnimationStyle offHandStyle = (IsMainHand ? handle.Manager.ActiveOffHandWeaponStyle : handle.Manager.ActiveMainHandWeaponStyle);
		data2.OffHandWrapper = m_OffHandSettings.FirstOrDefault((OffHandAnimationSettings s) => s.Style == offHandStyle)?.ClipWrapper;
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
			UpdateInternal(handle);
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

	private void UpdateInternal(UnitAnimationActionHandle handle)
	{
		AnimationBase activeAnimation = handle.ActiveAnimation;
		bool flag = false;
		if (handle.Manager.TryGetComponent<MechadendriteSettings>(out var component))
		{
			UnitEntityView componentInParent = component.GetComponentInParent<UnitEntityView>();
			flag = (componentInParent?.AnimationManager?.BlockAttackAnimation).GetValueOrDefault() || !(componentInParent.AnimationManager.CurrentAction.Action is WarhammerUnitAnimationActionHandAttack);
		}
		if (handle.Manager.BlockAttackAnimation || flag)
		{
			handle.SpeedScale = 0f;
		}
		else
		{
			handle.SpeedScale = Game.CombatAnimSpeedUp;
		}
		if (activeAnimation != null && !(handle.GetTime() < GetNextClipStartTime(handle)) && (!handle.NeedPreparingForShooting || !handle.IsPreparingForShooting) && (!handle.NeedPreparingForShooting || !(Math.Abs(handle.Manager.Orientation - handle.Manager.UseAbilityDirection) > 10f)) && !Next(handle))
		{
			handle.Release();
		}
	}

	private bool Next(UnitAnimationActionHandle handle)
	{
		Data data = GetData(handle);
		ClipSettings clipSettings = data.Sequence.Get(data.Index + 1);
		if (clipSettings == null)
		{
			return false;
		}
		data.Index++;
		StartClip(handle, clipSettings, data.OffHandWrapper);
		if (handle.ActiveAnimation != null)
		{
			if (data.Index > 0)
			{
				handle.ActiveAnimation.TransitionIn = 0f;
			}
			if (data.Index < data.Sequence.Count - 1)
			{
				handle.ActiveAnimation.ChangeTransitionTime(0f);
			}
		}
		return true;
	}

	private void StartClip(UnitAnimationActionHandle handle, ClipSettings settings, AnimationClipWrapper offHandWrapper)
	{
		if (offHandWrapper != null)
		{
			handle.Manager.AddAnimationClip(handle, settings.ClipWrapper, null, useEmptyAvatarMask: true, isAdditive: false, ClipDurationType.Endless, new AnimationComposition(handle));
			handle.Manager.AddClipToComposition(handle, offHandWrapper, m_OffHandMask, isAdditive: false);
		}
		else
		{
			handle.StartClip(settings.ClipWrapper, ClipDurationType.Endless);
		}
		handle.ActiveAnimation.OnlySendEventsOnce = true;
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
		float num = 0.3f + handle.BurstAnimationDelay;
		int num2 = ((!handle.IsBurst) ? 1 : handle.BurstCount);
		float time = handle.GetTime();
		int actEventsCounter = Math.Clamp(Mathf.FloorToInt((time - 0.45f) / num), 0, num2);
		handle.ActEventsCounter = actEventsCounter;
		if (time >= 0.79999995f + num * (float)num2)
		{
			handle.Release();
		}
	}
}
