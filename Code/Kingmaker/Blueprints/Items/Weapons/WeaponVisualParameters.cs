using System;
using Kingmaker.Sound;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Weapons;

[Serializable]
public class WeaponVisualParameters
{
	[SerializeField]
	private WeaponAnimationStyle m_WeaponAnimationStyle;

	[SerializeField]
	private UnitAnimationSpecialAttackType m_SpecialAnimation;

	[SerializeField]
	private float m_BurstAnimationDelay;

	[SerializeField]
	[ValidateIsPrefab]
	[ValidateHasComponent(typeof(EquipmentOffsets))]
	private GameObject m_WeaponModel;

	[SerializeField]
	[ValidateIsPrefab]
	[ValidateHasComponent(typeof(EquipmentOffsets))]
	private GameObject m_WeaponBeltModelOverride;

	[SerializeField]
	[ValidateIsPrefab]
	private GameObject m_WeaponSheathModelOverride;

	private WeaponEquipLinks m_CachedEquipLinks;

	private bool m_CachedEquipLinksUpToDate;

	[SerializeField]
	private WeaponSoundSizeType m_SoundSize;

	[SerializeField]
	private WeaponSoundType m_SoundType;

	[SerializeField]
	private AkSwitchReference m_SoundSizeSwitch;

	[SerializeField]
	private AkSwitchReference m_SoundTypeSwitch;

	[SerializeField]
	private AkSwitchReference m_MuffledTypeSwitch;

	[SerializeField]
	private WeaponMissSoundType m_MissSoundType;

	[SerializeField]
	[AkEventReference]
	private string m_EquipSound;

	[SerializeField]
	[AkEventReference]
	private string m_UnequipSound;

	[SerializeField]
	[AkEventReference]
	private string m_OutOfAmmoSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryPutSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryTakeSound;

	private static readonly UnitEquipmentVisualSlotType[] s_BackSlots = new UnitEquipmentVisualSlotType[2]
	{
		UnitEquipmentVisualSlotType.LeftBack01,
		UnitEquipmentVisualSlotType.RightBack01
	};

	private static readonly UnitEquipmentVisualSlotType[] s_AllSlots = new UnitEquipmentVisualSlotType[4]
	{
		UnitEquipmentVisualSlotType.LeftFront01,
		UnitEquipmentVisualSlotType.RightFront01,
		UnitEquipmentVisualSlotType.LeftBack01,
		UnitEquipmentVisualSlotType.RightBack01
	};

	private static readonly UnitEquipmentVisualSlotType[] s_ShieldSlots = new UnitEquipmentVisualSlotType[1] { UnitEquipmentVisualSlotType.Shield };

	public WeaponVisualParameters Prototype { get; set; }

	public bool HasSpecialAnimation => SpecialAnimation != UnitAnimationSpecialAttackType.None;

	public float BurstAnimationDelay => m_BurstAnimationDelay;

	public WeaponAnimationStyle AnimStyle
	{
		get
		{
			if (Prototype == null || m_WeaponAnimationStyle != 0)
			{
				return m_WeaponAnimationStyle;
			}
			return Prototype.AnimStyle;
		}
	}

	public GameObject Model
	{
		get
		{
			if (Prototype == null || !(m_WeaponModel == null))
			{
				return m_WeaponModel;
			}
			return Prototype.Model;
		}
	}

	public GameObject BeltModel
	{
		get
		{
			GameObject gameObject = ((Prototype != null && m_WeaponBeltModelOverride == null) ? Prototype.BeltModel : m_WeaponBeltModelOverride);
			if (!m_CachedEquipLinksUpToDate)
			{
				m_CachedEquipLinks = ObjectExtensions.Or(ObjectExtensions.Or(Model, null)?.GetComponent<WeaponEquipLinks>(), null);
				m_CachedEquipLinksUpToDate = true;
			}
			return m_CachedEquipLinks?.BeltModel ?? gameObject;
		}
	}

	public GameObject SheathModel
	{
		get
		{
			GameObject gameObject = ((Prototype != null && m_WeaponSheathModelOverride == null) ? Prototype.SheathModel : m_WeaponSheathModelOverride);
			if (!m_CachedEquipLinksUpToDate)
			{
				m_CachedEquipLinks = ObjectExtensions.Or(ObjectExtensions.Or(Model, null)?.GetComponent<WeaponEquipLinks>(), null);
				m_CachedEquipLinksUpToDate = true;
			}
			return m_CachedEquipLinks?.SheathModel ?? gameObject;
		}
	}

	public WeaponSoundSizeType SoundSize
	{
		get
		{
			if (Prototype == null || m_SoundSize != 0)
			{
				return m_SoundSize;
			}
			return Prototype.SoundSize;
		}
	}

	public WeaponSoundType SoundType
	{
		get
		{
			if (Prototype == null || m_SoundType != 0)
			{
				return m_SoundType;
			}
			return Prototype.SoundType;
		}
	}

	public AkSwitchReference SoundSizeSwitch
	{
		get
		{
			if (Prototype == null || !(m_SoundSizeSwitch.Value == ""))
			{
				return m_SoundSizeSwitch;
			}
			return Prototype.SoundSizeSwitch;
		}
	}

	public AkSwitchReference SoundTypeSwitch
	{
		get
		{
			if (Prototype == null || !(m_SoundTypeSwitch.Value == ""))
			{
				return m_SoundTypeSwitch;
			}
			return Prototype.SoundTypeSwitch;
		}
	}

	public AkSwitchReference MuffledTypeSwitch
	{
		get
		{
			if (Prototype == null || !(m_MuffledTypeSwitch.Value == ""))
			{
				return m_MuffledTypeSwitch;
			}
			return Prototype.MuffledTypeSwitch;
		}
	}

	public WeaponMissSoundType MissSoundType
	{
		get
		{
			if (Prototype == null || m_MissSoundType != 0)
			{
				return m_MissSoundType;
			}
			return Prototype.MissSoundType;
		}
	}

	public string EquipSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_EquipSound))
			{
				return m_EquipSound;
			}
			return Prototype.EquipSound;
		}
	}

	public string UnequipSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_UnequipSound))
			{
				return m_UnequipSound;
			}
			return Prototype.UnequipSound;
		}
	}

	public string OutOfAmmoSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_OutOfAmmoSound))
			{
				return m_OutOfAmmoSound;
			}
			return Prototype.OutOfAmmoSound;
		}
	}

	public UnitAnimationSpecialAttackType SpecialAnimation
	{
		get
		{
			if (Prototype == null || m_SpecialAnimation != 0)
			{
				return m_SpecialAnimation;
			}
			return Prototype.SpecialAnimation;
		}
	}

	public string InventoryEquipSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_InventoryEquipSound))
			{
				return m_InventoryEquipSound;
			}
			return Prototype.InventoryEquipSound;
		}
	}

	public string InventoryPutSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_InventoryPutSound))
			{
				return m_InventoryPutSound;
			}
			return Prototype.InventoryPutSound;
		}
	}

	public string InventoryTakeSound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_InventoryTakeSound))
			{
				return m_InventoryTakeSound;
			}
			return Prototype.InventoryTakeSound;
		}
	}

	public bool IsBow => false;

	public bool IsTorch => false;

	public bool HasQuiver => false;

	public bool IsTwoHanded
	{
		get
		{
			switch (AnimStyle)
			{
			case WeaponAnimationStyle.AxeTwoHanded:
			case WeaponAnimationStyle.Assault:
			case WeaponAnimationStyle.BrutalTwoHanded:
			case WeaponAnimationStyle.HeavyOnHip:
			case WeaponAnimationStyle.HeavyOnShoulder:
			case WeaponAnimationStyle.Rifle:
			case WeaponAnimationStyle.Staff:
			case WeaponAnimationStyle.EldarRifle:
			case WeaponAnimationStyle.EldarAssault:
			case WeaponAnimationStyle.EldarHeavyOnHip:
			case WeaponAnimationStyle.EldarHeavyOnShoulder:
			case WeaponAnimationStyle.TwoHandedHammer:
				return true;
			default:
				return false;
			}
		}
	}

	public UnitEquipmentVisualSlotType[] AttachSlots
	{
		get
		{
			switch (AnimStyle)
			{
			case WeaponAnimationStyle.Knife:
			case WeaponAnimationStyle.Fencing:
			case WeaponAnimationStyle.BrutalOneHanded:
			case WeaponAnimationStyle.Pistol:
			case WeaponAnimationStyle.OneHandedHammer:
				return s_AllSlots;
			case WeaponAnimationStyle.Shield:
				return s_ShieldSlots;
			case WeaponAnimationStyle.AxeTwoHanded:
			case WeaponAnimationStyle.Assault:
			case WeaponAnimationStyle.BrutalTwoHanded:
			case WeaponAnimationStyle.HeavyOnHip:
			case WeaponAnimationStyle.HeavyOnShoulder:
			case WeaponAnimationStyle.Rifle:
			case WeaponAnimationStyle.Staff:
			case WeaponAnimationStyle.EldarRifle:
			case WeaponAnimationStyle.EldarAssault:
			case WeaponAnimationStyle.EldarHeavyOnHip:
			case WeaponAnimationStyle.EldarHeavyOnShoulder:
			case WeaponAnimationStyle.TwoHandedHammer:
				return s_BackSlots;
			default:
				return s_AllSlots;
			}
		}
	}
}
