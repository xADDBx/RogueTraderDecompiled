using System;
using Kingmaker.Sound;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Armors;

[Serializable]
public class ArmorVisualParameters
{
	[SerializeField]
	private AkSwitchReference m_SoundSwitch = new AkSwitchReference();

	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryPutSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryTakeSound;

	[SerializeField]
	[AkEventReference]
	private string m_AnimationFoleySound;

	[SerializeField]
	private GameObject m_OverrideMissEffect;

	public ArmorVisualParameters Prototype { get; set; }

	public AkSwitchReference SoundSwitch
	{
		get
		{
			if (Prototype == null || !m_SoundSwitch.Value.IsNullOrEmpty())
			{
				return m_SoundSwitch;
			}
			return Prototype.SoundSwitch;
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
		set
		{
			m_InventoryEquipSound = value;
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
		set
		{
			m_InventoryPutSound = value;
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
		set
		{
			m_InventoryTakeSound = value;
		}
	}

	public string AnimationFoleySound
	{
		get
		{
			if (Prototype == null || !string.IsNullOrEmpty(m_AnimationFoleySound))
			{
				return m_AnimationFoleySound;
			}
			return Prototype.AnimationFoleySound;
		}
	}

	public GameObject OverrideMissEffect
	{
		get
		{
			if (!m_OverrideMissEffect)
			{
				return Prototype?.m_OverrideMissEffect;
			}
			return m_OverrideMissEffect;
		}
	}
}
