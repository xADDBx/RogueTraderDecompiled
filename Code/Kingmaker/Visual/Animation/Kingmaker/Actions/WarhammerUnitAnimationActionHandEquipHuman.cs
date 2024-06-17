using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationActionHandEquipHuman", menuName = "Animation Manager/Actions/Warhammer Unit Hand Equip|Unequip Human")]
public class WarhammerUnitAnimationActionHandEquipHuman : UnitAnimationAction, IUnitAnimationActionHandEquip
{
	private enum ActionType
	{
		MainHandEquip,
		MainHandUnequip,
		OffHandEquip,
		OffHandUnequip
	}

	[Serializable]
	public class EquipmentStyleSetting
	{
		public WeaponAnimationStyle Style;

		[SerializeField]
		private EquipmentSlotSetting[] m_Slots = new EquipmentSlotSetting[0];

		public EquipmentSlotSetting[] Slots => m_Slots;

		public IEnumerable<AnimationClipWrapper> GetAll()
		{
			return m_Slots.Select((EquipmentSlotSetting i) => i.ClipWrapper);
		}
	}

	[Serializable]
	public class EquipmentSlotSetting
	{
		public UnitEquipmentAnimationSlotType Slot;

		[AssetPicker("")]
		[ValidateNotNull]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public AnimationClipWrapper ClipWrapper;

		public AnimationClipWrapper IdleToCombatClipWrapper;

		public AnimationClipWrapper InCombatClipWrapper;

		public AnimationClipWrapper CombatToIdleClipWrapper;

		public AnimationClipWrapper HalfAnimationClipWrapper;

		public void Validate(ValidationContext context)
		{
			if (Slot == UnitEquipmentAnimationSlotType.None)
			{
				context.AddError("Slot can't be None");
			}
			if (!ClipWrapper)
			{
				context.AddError("Clip wrapper is missing");
			}
			if ((bool)ClipWrapper && ClipWrapper.IsLooping)
			{
				context.AddError("Clip wrapper is looping");
			}
		}
	}

	private class EquipHandleData
	{
		public PlayableInfo FullBody;

		public PlayableInfo UpperBody;
	}

	[SerializeField]
	private ActionType m_Type;

	[SerializeField]
	private EquipmentStyleSetting[] m_Styles = new EquipmentStyleSetting[0];

	public bool IsMainHand
	{
		get
		{
			if (m_Type != 0)
			{
				return m_Type == ActionType.MainHandUnequip;
			}
			return true;
		}
	}

	public bool IsEquip
	{
		get
		{
			if (m_Type != 0)
			{
				return m_Type == ActionType.OffHandEquip;
			}
			return true;
		}
	}

	public EquipmentStyleSetting[] Styles => m_Styles;

	public override UnitAnimationType Type
	{
		get
		{
			if (!IsEquip)
			{
				if (!IsMainHand)
				{
					return UnitAnimationType.OffHandUnequip;
				}
				return UnitAnimationType.MainHandUnequip;
			}
			if (!IsMainHand)
			{
				return UnitAnimationType.OffHandEquip;
			}
			return UnitAnimationType.MainHandEquip;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Styles.SelectMany((EquipmentStyleSetting s) => s.Slots.SelectMany((EquipmentSlotSetting sl) => new AnimationClipWrapper[5] { sl.ClipWrapper, sl.IdleToCombatClipWrapper, sl.InCombatClipWrapper, sl.CombatToIdleClipWrapper, sl.HalfAnimationClipWrapper }));

	public void SetType(bool isMain, bool isEquip)
	{
		m_Type = ((!isMain) ? (isEquip ? ActionType.OffHandEquip : ActionType.OffHandUnequip) : ((!isEquip) ? ActionType.MainHandUnequip : ActionType.MainHandEquip));
	}

	public float GetDuration(UnitAnimationActionHandle handle)
	{
		return GetClip(handle)?.AnimationClip.length ?? 0.01f;
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper clip = GetClip(handle);
		if ((bool)clip)
		{
			handle.StartClip(clip, ClipDurationType.Oneshot);
			if (handle.ActiveAnimation == null)
			{
				handle.Release();
			}
			handle.ActiveAnimation.DoNotZeroOtherAnimations = false;
			AvatarMask mask = ((0 < AvatarMasks.Count) ? AvatarMasks[0] : null);
			EquipHandleData actionData = new EquipHandleData
			{
				FullBody = (PlayableInfo)handle.ActiveAnimation.Find(null, isAdditive: false),
				UpperBody = (PlayableInfo)handle.ActiveAnimation.Find(mask, isAdditive: false)
			};
			handle.ActionData = actionData;
		}
		else
		{
			handle.Release();
		}
		handle.Manager.CurrentEquipHandle = handle;
		handle.Manager.PreviousInCombat = handle.Manager.IsInCombat;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		base.OnUpdate(handle, deltaTime);
		(handle.ActionData as EquipHandleData)?.FullBody?.SetWeightMultiplier((!(handle.Manager.NewSpeed > 0f)) ? 1 : 0);
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		base.OnFinish(handle);
		handle.Manager.CurrentEquipHandle = null;
	}

	[CanBeNull]
	private AnimationClipWrapper GetClip(UnitAnimationActionHandle handle)
	{
		EquipmentSlotSetting equipmentSlotSetting = m_Styles.FirstOrDefault((EquipmentStyleSetting i) => i.Style == handle.AttackWeaponStyle)?.Slots.FirstOrDefault((EquipmentSlotSetting i) => i.Slot == handle.EquipmentSlot);
		AnimationClipWrapper animationClipWrapper = null;
		if (handle.Manager.IsInCombat && handle.Manager.PreviousInCombat && equipmentSlotSetting?.InCombatClipWrapper != null)
		{
			animationClipWrapper = (equipmentSlotSetting.HalfAnimationClipWrapper ? equipmentSlotSetting.HalfAnimationClipWrapper : equipmentSlotSetting.InCombatClipWrapper);
		}
		if (handle.Manager.IsInCombat && !handle.Manager.PreviousInCombat && equipmentSlotSetting?.InCombatClipWrapper != null)
		{
			animationClipWrapper = equipmentSlotSetting.IdleToCombatClipWrapper;
		}
		if (!handle.Manager.IsInCombat && handle.Manager.PreviousInCombat && equipmentSlotSetting?.InCombatClipWrapper != null)
		{
			animationClipWrapper = equipmentSlotSetting.CombatToIdleClipWrapper;
		}
		if (animationClipWrapper == null && equipmentSlotSetting?.ClipWrapper != null)
		{
			animationClipWrapper = equipmentSlotSetting?.ClipWrapper;
		}
		if (!animationClipWrapper)
		{
			PFLog.Default.Error($"{base.name} can't find clip for slot {handle.EquipmentSlot}");
		}
		return animationClipWrapper;
	}
}
