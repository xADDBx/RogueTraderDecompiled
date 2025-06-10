using System;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace Kingmaker.View.Equipment;

public class HandEquipmentHelper : CustomYieldInstruction, IDisposable
{
	private readonly UnitAnimationActionHandle m_Handle;

	private bool m_IsDisposed;

	public Action Callback;

	public float Duration => ((IUnitAnimationActionHandEquip)(m_Handle?.Action))?.GetDuration(m_Handle) ?? 0f;

	public override bool keepWaiting
	{
		get
		{
			try
			{
				return Next();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				return false;
			}
		}
	}

	public void Scale(float scale)
	{
		if (m_Handle != null)
		{
			m_Handle.SpeedScale = scale;
		}
	}

	public static HandEquipmentHelper StartEquipMainHand(UnitAnimationManager manager, UnitEquipmentAnimationSlotType slot, WeaponAnimationStyle style)
	{
		return new HandEquipmentHelper(manager, slot, equip: true, mainHand: true, style);
	}

	public static HandEquipmentHelper StartEquipOffHand(UnitAnimationManager manager, UnitEquipmentAnimationSlotType slot, WeaponAnimationStyle style)
	{
		return new HandEquipmentHelper(manager, slot, equip: true, mainHand: false, style);
	}

	public static HandEquipmentHelper StartUnequipMainHand(UnitAnimationManager manager, UnitEquipmentAnimationSlotType slot, WeaponAnimationStyle style)
	{
		return new HandEquipmentHelper(manager, slot, equip: false, mainHand: true, style);
	}

	public static HandEquipmentHelper StartUnequipOffHand(UnitAnimationManager manager, UnitEquipmentAnimationSlotType slot, WeaponAnimationStyle style)
	{
		return new HandEquipmentHelper(manager, slot, equip: false, mainHand: false, style);
	}

	private HandEquipmentHelper(UnitAnimationManager manager, UnitEquipmentAnimationSlotType slot, bool equip, bool mainHand, WeaponAnimationStyle style)
	{
		m_Handle = manager.CreateHandle((!equip) ? (mainHand ? UnitAnimationType.MainHandUnequip : UnitAnimationType.OffHandUnequip) : (mainHand ? UnitAnimationType.MainHandEquip : UnitAnimationType.OffHandEquip), errorOnEmpty: false);
		if (m_Handle != null)
		{
			m_Handle.EquipmentSlot = slot;
			m_Handle.AttackWeaponStyle = style;
			manager.Execute(m_Handle);
		}
	}

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_Handle?.Release();
			m_IsDisposed = true;
		}
	}

	public bool Next()
	{
		if (m_IsDisposed)
		{
			PFLog.Default.Error("Do not use disposed HandEquipmentHelper");
			return false;
		}
		if (Callback != null && (m_Handle == null || m_Handle.IsActed || m_Handle.IsReleased))
		{
			Callback();
			Callback = null;
		}
		UnitAnimationActionHandle handle = m_Handle;
		if (handle != null)
		{
			return !handle.IsReleased;
		}
		return false;
	}
}
