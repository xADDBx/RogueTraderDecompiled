using System;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Mechadendrites;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Equipment;

public class UnitMechadendriteEquipmentData
{
	private readonly struct NotifyOwnerRenderersChangedScope : IDisposable
	{
		private readonly UnitMechadendriteEquipmentData m_Data;

		private readonly GameObject m_InitialVisualModel;

		public NotifyOwnerRenderersChangedScope(UnitMechadendriteEquipmentData data)
		{
			m_Data = data;
			m_InitialVisualModel = m_Data.VisualModel;
		}

		public void Dispose()
		{
			if (m_InitialVisualModel != m_Data.VisualModel)
			{
				m_Data.View.MarkRenderersAndCollidersAreUpdated();
			}
		}
	}

	public readonly AbstractUnitEntityView View;

	public readonly Character Character;

	private Transform m_BoneTransform;

	public ItemEntity VisibleItem { get; private set; }

	public GameObject VisualModel { get; private set; }

	private BlueprintItemMechadendrite VisibleItemBlueprint => (BlueprintItemMechadendrite)(VisibleItem?.Blueprint);

	private Transform BoneTransform
	{
		get
		{
			if (m_BoneTransform != null)
			{
				return m_BoneTransform;
			}
			if (Character != null)
			{
				m_BoneTransform = Character.transform.FindChildRecursive("C_back_weapon_slot_08_ADJ");
			}
			if (!(m_BoneTransform != null))
			{
				return null;
			}
			return m_BoneTransform;
		}
	}

	public UnitMechadendriteEquipmentData(AbstractUnitEntityView owner, Character character, ItemEntity item)
	{
		View = owner;
		Character = character;
		VisibleItem = item;
	}

	public void AttachModel()
	{
		using (new NotifyOwnerRenderersChangedScope(this))
		{
			if (Character == null)
			{
				DestroyModelIfExists();
			}
			else
			{
				if (VisualModel == null)
				{
					return;
				}
				Transform boneTransform = BoneTransform;
				VisualModel.SetActive(value: true);
				if (boneTransform == null)
				{
					DestroyModelIfExists();
					return;
				}
				VisualModel.transform.SetParent(boneTransform, worldPositionStays: true);
				VisualModel.transform.localPosition = Vector3.zero;
				VisualModel.transform.localRotation = Quaternion.identity;
				VisualModel.transform.localRotation = Quaternion.Euler(-90.877f, -4.442017f, 3.807999f);
				if (VisualModel.GetComponent<UnitAnimationManager>() != null)
				{
					Character.MechsAnimationManagers.Add(VisualModel.GetComponent<UnitAnimationManager>());
				}
			}
		}
	}

	public void RecreateModel()
	{
		DestroyModelIfExists();
		GameObject fxPrefab = GetFxPrefab();
		if ((bool)fxPrefab)
		{
			VisualModel = UnityEngine.Object.Instantiate(fxPrefab);
		}
	}

	private void DestroyModelIfExists()
	{
		if (View != null)
		{
			AbstractUnitEntity data = View.Data;
			if (data != null && data.GetOptional<UnitPartMechadendrites>()?.Mechadendrites.Count == 0)
			{
				View.Data.Remove<UnitPartMechadendrites>();
			}
		}
		if (!(VisualModel == null))
		{
			EquipmentOffsets component = VisualModel.GetComponent<EquipmentOffsets>();
			if ((bool)component && (bool)component.JointsParent)
			{
				UnityEngine.Object.Destroy(component.JointsParent.gameObject);
			}
			if (VisualModel.GetComponent<UnitAnimationManager>() != null && Character != null && Character.MechsAnimationManagers.Contains(VisualModel.GetComponent<UnitAnimationManager>()))
			{
				Character.MechsAnimationManagers.Remove(VisualModel.GetComponent<UnitAnimationManager>());
			}
			UnityEngine.Object.Destroy(VisualModel);
			VisualModel = null;
		}
	}

	public void DestroyModel()
	{
		if (VisualModel.GetComponent<UnitAnimationManager>() != null && Character != null && Character.MechsAnimationManagers.Contains(VisualModel.GetComponent<UnitAnimationManager>()))
		{
			Character.MechsAnimationManagers.Remove(VisualModel.GetComponent<UnitAnimationManager>());
		}
		UnityEngine.Object.Destroy(VisualModel);
		VisualModel = null;
	}

	private GameObject GetFxPrefab()
	{
		GameObject result = null;
		if ((bool)VisibleItemBlueprint)
		{
			result = VisibleItemBlueprint.Model;
		}
		return result;
	}
}
