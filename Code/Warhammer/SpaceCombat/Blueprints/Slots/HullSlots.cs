using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Warhammer.SpaceCombat.Blueprints.Slots;

[Serializable]
public class HullSlots
{
	[Header("Main Slots")]
	[SerializeField]
	[FormerlySerializedAs("PlasmaDrives")]
	private BlueprintItemPlasmaDrivesReference m_PlasmaDrives;

	[SerializeField]
	private BlueprintItemVoidShieldGeneratorReference m_VoidShieldGenerator;

	[SerializeField]
	private BlueprintItemWarpDrivesReference m_WarpDrives;

	[SerializeField]
	private BlueprintItemGellerFieldDeviceReference m_GellerFieldDevice;

	[SerializeField]
	private BlueprintItemLifeSustainerReference m_LifeSustainer;

	[SerializeField]
	private BlueprintItemBridgeReference m_Bridge;

	[SerializeField]
	private BlueprintItemAugerArrayReference m_AugerArray;

	[SerializeField]
	private BlueprintItemArmorPlatingReference m_ArmorPlating;

	[SerializeField]
	private BlueprintItemArsenalReference[] m_Arsenals;

	public List<WeaponSlotData> Weapons;

	public BlueprintItemPlasmaDrives PlasmaDrives
	{
		get
		{
			return m_PlasmaDrives?.Get();
		}
		set
		{
			m_PlasmaDrives = value.ToReference<BlueprintItemPlasmaDrivesReference>();
		}
	}

	public BlueprintItemVoidShieldGenerator VoidShieldGenerator
	{
		get
		{
			return m_VoidShieldGenerator?.Get();
		}
		set
		{
			m_VoidShieldGenerator = value.ToReference<BlueprintItemVoidShieldGeneratorReference>();
		}
	}

	public BlueprintItemWarpDrives WarpDrives
	{
		get
		{
			return m_WarpDrives?.Get();
		}
		set
		{
			m_WarpDrives = value.ToReference<BlueprintItemWarpDrivesReference>();
		}
	}

	public BlueprintItemGellerFieldDevice GellerFieldDevice
	{
		get
		{
			return m_GellerFieldDevice?.Get();
		}
		set
		{
			m_GellerFieldDevice = value.ToReference<BlueprintItemGellerFieldDeviceReference>();
		}
	}

	public BlueprintItemLifeSustainer LifeSustainer
	{
		get
		{
			return m_LifeSustainer?.Get();
		}
		set
		{
			m_LifeSustainer = value.ToReference<BlueprintItemLifeSustainerReference>();
		}
	}

	public BlueprintItemBridge Bridge
	{
		get
		{
			return m_Bridge?.Get();
		}
		set
		{
			m_Bridge = value.ToReference<BlueprintItemBridgeReference>();
		}
	}

	public BlueprintItemAugerArray AugerArray
	{
		get
		{
			return m_AugerArray?.Get();
		}
		set
		{
			m_AugerArray = value.ToReference<BlueprintItemAugerArrayReference>();
		}
	}

	public BlueprintItemArmorPlating ArmorPlating
	{
		get
		{
			return m_ArmorPlating?.Get();
		}
		set
		{
			m_ArmorPlating = value.ToReference<BlueprintItemArmorPlatingReference>();
		}
	}

	public ReferenceArrayProxy<BlueprintItemArsenal> Arsenals
	{
		get
		{
			BlueprintReference<BlueprintItemArsenal>[] arsenals = m_Arsenals;
			return arsenals;
		}
	}
}
