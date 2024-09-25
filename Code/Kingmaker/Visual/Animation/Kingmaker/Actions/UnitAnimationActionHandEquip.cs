using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Equipment;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimatioHandEquip", menuName = "Animation Manager/Actions/Unit Hand Equip|Unequip")]
public class UnitAnimationActionHandEquip : UnitAnimationAction, IUnitAnimationActionHandEquip
{
	private enum ActionType
	{
		MainHandEquip,
		MainHandUnequip,
		OffHandEquip,
		OffHandUnequip
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
	private EquipmentSlotSetting[] m_Slots = new EquipmentSlotSetting[0];

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

	public EquipmentSlotSetting[] Slots => m_Slots;

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

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Slots.Select((EquipmentSlotSetting i) => i.ClipWrapper);

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
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
			EquipHandleData actionData = new EquipHandleData
			{
				FullBody = (PlayableInfo)handle.ActiveAnimation.Find(null, isAdditive: false),
				UpperBody = (PlayableInfo)handle.ActiveAnimation.Find(AvatarMasks[0], isAdditive: false)
			};
			handle.ActionData = actionData;
		}
		else
		{
			handle.Release();
		}
		handle.Manager.CurrentEquipHandle = handle;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		base.OnUpdate(handle, deltaTime);
		(handle.ActionData as EquipHandleData)?.FullBody.SetWeightMultiplier((!(handle.Manager.Speed > 0f)) ? 1 : 0);
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		base.OnFinish(handle);
		handle.Manager.CurrentEquipHandle = null;
	}

	[CanBeNull]
	private AnimationClipWrapper GetClip(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper clipWrapper = m_Slots.FirstOrDefault((EquipmentSlotSetting i) => i.Slot == handle.EquipmentSlot).ClipWrapper;
		if (!clipWrapper)
		{
			PFLog.Default.Error($"{base.name} can't find clip for slot {handle.EquipmentSlot}");
		}
		return clipWrapper;
	}

	public IEnumerable<UnitEquipmentAnimationSlotType> GetSlotsForConfig()
	{
		yield return (!IsMainHand) ? UnitEquipmentAnimationSlotType.FrontRight : UnitEquipmentAnimationSlotType.FrontLeft;
		yield return UnitEquipmentAnimationSlotType.BackLeft;
		yield return UnitEquipmentAnimationSlotType.BackRight;
		yield return UnitEquipmentAnimationSlotType.BackCenter;
	}
}
