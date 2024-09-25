using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Common;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
using UnityEngine;
using UnityEngine.VFX;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

public class StarshipView : MonoBehaviour, IEntitySubscriber, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, EntitySubscriber>
{
	public UnitEntityView UnitEntityView;

	public ParticlesSnapMap particleSnapMap;

	[Tooltip("Front red, right blue, back yellow, left green")]
	public StarshipFxHitMask starshipFxHitMask;

	public Mesh mesh;

	public Renderer BaseRenderer;

	public Renderer StarMapVisualRenderer;

	public List<StarshipItemSlot> ItemSlots = new List<StarshipItemSlot>();

	private bool IsPlasmaDriveInit;

	private VisualEffect[] m_DriveVisualEffects;

	private UnitMovementAgentBase m_UnitMovementAgent;

	private bool m_HasMovementAgent;

	private void Start()
	{
		SetAllEquipment();
		m_DriveVisualEffects = GetComponentsInChildren<VisualEffect>();
		m_UnitMovementAgent = GetComponentInParent<UnitMovementAgentBase>();
		m_HasMovementAgent = m_UnitMovementAgent != null;
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void Update()
	{
		if (!IsPlasmaDriveInit)
		{
			ReinitPlasmaDriveVFX();
		}
		if (!m_HasMovementAgent || m_DriveVisualEffects.Length == 0)
		{
			return;
		}
		VisualEffect[] driveVisualEffects = m_DriveVisualEffects;
		foreach (VisualEffect visualEffect in driveVisualEffects)
		{
			if (visualEffect != null && visualEffect.HasFloat("Intencity"))
			{
				visualEffect.SetFloat("Intencity", Mathf.Clamp01(m_UnitMovementAgent.SpeedIndicator));
			}
		}
	}

	public Mesh FindMesh()
	{
		if (mesh != null)
		{
			return mesh;
		}
		mesh = base.gameObject.GetComponent<MeshFilter>().sharedMesh;
		if (mesh == null)
		{
			return null;
		}
		return mesh;
	}

	public void SetAllEquipment()
	{
		if (UnitEntityView == null || UnitEntityView.Data == null)
		{
			return;
		}
		PartStarshipHull hull = UnitEntityView.Data.GetHull();
		if (hull == null)
		{
			return;
		}
		foreach (ItemSlot equipmentSlot in hull.HullSlots.EquipmentSlots)
		{
			if (equipmentSlot.HasItem)
			{
				EquipItemFromItemSlot(equipmentSlot.Item, isEquip: true);
			}
		}
		foreach (Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot weaponSlot in hull.HullSlots.WeaponSlots)
		{
			if (weaponSlot.HasItem)
			{
				EquipItemFromItemSlot(weaponSlot.Item, isEquip: true);
			}
		}
		UIDollRooms.Instance?.ShipDollRoom?.UpdateStarshipRenderers();
		UIDollRooms.Instance?.CharGenShipDollRoom?.UpdateStarshipRenderers();
		StandardMaterialController componentInParent = GetComponentInParent<StandardMaterialController>();
		if (componentInParent != null)
		{
			componentInParent.InvalidateRenderersAndMaterials();
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (slot.HasItem)
		{
			EquipItemFromItemSlot(slot.Item, isEquip: true);
		}
		else
		{
			EquipItemFromItemSlot(previousItem, isEquip: false);
		}
		UIDollRooms.Instance?.ShipDollRoom?.UpdateStarshipRenderers();
	}

	private void EquipItemFromItemSlot(ItemEntity itemSlot, bool isEquip)
	{
		BlueprintItem blueprint = itemSlot.Blueprint;
		if (!(blueprint is BlueprintItemArmorPlating blueprintItemArmorPlating))
		{
			if (!(blueprint is BlueprintItemAugerArray blueprintItemAugerArray))
			{
				if (!(blueprint is BlueprintItemBridge blueprintItemBridge))
				{
					if (!(blueprint is BlueprintItemGellerFieldDevice blueprintItemGellerFieldDevice))
					{
						if (!(blueprint is BlueprintItemLifeSustainer blueprintItemLifeSustainer))
						{
							if (!(blueprint is BlueprintItemPlasmaDrives blueprintItemPlasmaDrives))
							{
								if (!(blueprint is BlueprintItemVoidShieldGenerator blueprintItemVoidShieldGenerator))
								{
									if (!(blueprint is BlueprintItemWarpDrives blueprintItemWarpDrives))
									{
										if (!(blueprint is BlueprintStarshipWeapon weaponBP))
										{
											_ = blueprint is BlueprintItemArsenal;
										}
										else
										{
											EquipWeapon(weaponBP, isEquip);
										}
									}
									else if (blueprintItemWarpDrives.StarshipEE != null)
									{
										EquipItem(blueprintItemWarpDrives.StarshipEE.EEArtSlotsDescription, isEquip);
									}
								}
								else if (blueprintItemVoidShieldGenerator.StarshipEE != null)
								{
									EquipItem(blueprintItemVoidShieldGenerator.StarshipEE.EEArtSlotsDescription, isEquip);
								}
							}
							else if (blueprintItemPlasmaDrives.StarshipEE != null)
							{
								EquipItem(blueprintItemPlasmaDrives.StarshipEE.EEArtSlotsDescription, isEquip);
							}
						}
						else if (blueprintItemLifeSustainer.StarshipEE != null)
						{
							EquipItem(blueprintItemLifeSustainer.StarshipEE.EEArtSlotsDescription, isEquip);
						}
					}
					else if (blueprintItemGellerFieldDevice.StarshipEE != null)
					{
						EquipItem(blueprintItemGellerFieldDevice.StarshipEE.EEArtSlotsDescription, isEquip);
					}
				}
				else if (blueprintItemBridge.StarshipEE != null)
				{
					EquipItem(blueprintItemBridge.StarshipEE.EEArtSlotsDescription, isEquip);
				}
			}
			else if (blueprintItemAugerArray.StarshipEE != null)
			{
				EquipItem(blueprintItemAugerArray.StarshipEE.EEArtSlotsDescription, isEquip);
			}
		}
		else if (blueprintItemArmorPlating.StarshipEE != null)
		{
			EquipItem(blueprintItemArmorPlating.StarshipEE.EEArtSlotsDescription, isEquip);
		}
	}

	private void EquipItem(List<StarshipSlotDescription> artSlots, bool isEquip)
	{
		foreach (StarshipSlotDescription artSlot in artSlots)
		{
			GameObject prefab = artSlot.Prefab;
			foreach (RequiredSlotVariant requiredSlots in artSlot.RequiredSlots)
			{
				List<StarshipItemSlot> list = ItemSlots.FindAll((StarshipItemSlot x) => x.Type == requiredSlots.SlotType);
				foreach (StarshipItemSlot item in list)
				{
					if (item.itemPrefab != null)
					{
						Object.Destroy(item.itemPrefab);
					}
				}
				if (!isEquip)
				{
					continue;
				}
				foreach (StarshipItemSlot item2 in list)
				{
					(item2.itemPrefab = Object.Instantiate(prefab, item2.transform.position, Quaternion.identity, item2.transform)).transform.localEulerAngles = Vector3.zero;
				}
			}
		}
	}

	private void EquipWeapon(BlueprintStarshipWeapon weaponBP, bool isEquip)
	{
		if (weaponBP.StarshipEE == null)
		{
			return;
		}
		List<StarshipSlotDescription> eEArtSlotsDescription = weaponBP.StarshipEE.EEArtSlotsDescription;
		if (eEArtSlotsDescription == null || eEArtSlotsDescription.Count <= 0)
		{
			return;
		}
		foreach (StarshipSlotDescription item in eEArtSlotsDescription)
		{
			GameObject prefab = item.Prefab;
			foreach (RequiredSlotVariant requiredSlots in item.RequiredSlots)
			{
				List<StarshipItemSlot> list = ItemSlots.FindAll((StarshipItemSlot x) => x.Type == requiredSlots.SlotType);
				foreach (StarshipItemSlot item2 in list)
				{
					if (item2.itemPrefab != null)
					{
						Object.Destroy(item2.itemPrefab);
					}
				}
				if (!isEquip)
				{
					continue;
				}
				foreach (StarshipItemSlot item3 in list)
				{
					GameObject gameObject = Object.Instantiate(prefab, item3.transform.position, Quaternion.identity, item3.transform);
					WeaponParticlesSnapMap component = gameObject.GetComponent<WeaponParticlesSnapMap>();
					if (component != null)
					{
						foreach (FxLocator fxLocator in component.FxLocators)
						{
							StarshipFxLocator component2 = fxLocator.GetComponent<StarshipFxLocator>();
							if (component2 != null)
							{
								component2.weaponSlotType = GetSlotType(requiredSlots.SlotType);
							}
						}
					}
					item3.itemPrefab = gameObject;
					gameObject.transform.localEulerAngles = Vector3.zero;
				}
			}
		}
	}

	private WeaponSlotType GetSlotType(StarshipItemSlotType weaponSlotType)
	{
		return weaponSlotType switch
		{
			StarshipItemSlotType.Dorsal => WeaponSlotType.Dorsal, 
			StarshipItemSlotType.Keel => WeaponSlotType.Keel, 
			StarshipItemSlotType.Port => WeaponSlotType.Port, 
			StarshipItemSlotType.Prow => WeaponSlotType.Prow, 
			StarshipItemSlotType.Starboard => WeaponSlotType.Starboard, 
			_ => WeaponSlotType.Dorsal, 
		};
	}

	public void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void FillItemsSlots()
	{
	}

	public void OnDrawGizmosSelected()
	{
		if (starshipFxHitMask.frontHitPositions.Count > 0)
		{
			Gizmos.color = Color.red;
			foreach (Vector3 frontHitPosition in starshipFxHitMask.frontHitPositions)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(frontHitPosition), 0.01f);
			}
		}
		if (starshipFxHitMask.leftHitPositions.Count > 0)
		{
			Gizmos.color = Color.green;
			foreach (Vector3 leftHitPosition in starshipFxHitMask.leftHitPositions)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(leftHitPosition), 0.01f);
			}
		}
		if (starshipFxHitMask.rightHitPositions.Count > 0)
		{
			Gizmos.color = Color.blue;
			foreach (Vector3 rightHitPosition in starshipFxHitMask.rightHitPositions)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(rightHitPosition), 0.01f);
			}
		}
		if (starshipFxHitMask.backHitPositions.Count <= 0)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		foreach (Vector3 backHitPosition in starshipFxHitMask.backHitPositions)
		{
			Gizmos.DrawSphere(base.transform.TransformPoint(backHitPosition), 0.01f);
		}
	}

	public void ReinitPlasmaDriveVFX()
	{
		if (m_DriveVisualEffects == null || m_DriveVisualEffects.Length == 0)
		{
			return;
		}
		VisualEffect[] driveVisualEffects = m_DriveVisualEffects;
		foreach (VisualEffect visualEffect in driveVisualEffects)
		{
			if (!(visualEffect == null))
			{
				visualEffect.gameObject.SetActive(value: false);
				visualEffect.gameObject.SetActive(value: true);
			}
		}
		IsPlasmaDriveInit = true;
	}

	public IEntity GetSubscribingEntity()
	{
		return UnitEntityView.Data;
	}
}
