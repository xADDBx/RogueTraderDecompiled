using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Owlcat.QA.Validation;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("b1dda40dbcb7b8647af156cf539d3ed8")]
public class BlueprintStarship : BlueprintUnit
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintStarship>
	{
	}

	[Header("=== Starship Part ===")]
	[SerializeField]
	private BlueprintStarshipSoundSettings.Reference m_SoundSettings;

	[Header("Starship Stats")]
	public int HullIntegrity = 100;

	public int ArmourFore;

	public int ArmourPort;

	public int ArmourStarboard;

	public int ArmourAft;

	public int Inertia = 3;

	public int StarshipSpeed = 10;

	public int Evasion;

	public int Morale = 100;

	public int Initiative;

	public int InspirationAmount;

	public int TurretRating;

	public int TurretRadius;

	public int MilitaryRating;

	public int CrewQuartersThroughput = 5;

	[SerializeField]
	private List<ShipModuleSettings> m_Modules;

	private int? m_CrewCount;

	public float SpeedOnStarSystemMap = 1f;

	public float AiDesiredDistanceToPlayer = 3f;

	public HullSlots HullSlots = new HullSlots();

	public float ApproachStarSystemObjectRadius = 10f;

	public bool IsSoftUnit;

	[Header("Posts")]
	public List<PostData> Posts;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintAreaEnterPointReference m_ShipAreaEnterPoint;

	[Header("Big Ship Description")]
	[CanBeNull]
	public Sprite PlayerShipBigPicture;

	[CanBeNull]
	[SerializeField]
	private LocalizedString m_PlayerShipName;

	[CanBeNull]
	[SerializeField]
	private LocalizedString m_PlayerShipDescription;

	[CanBeNull]
	[SerializeField]
	private LocalizedString m_PlayerShipFlavor;

	[CanBeNull]
	[SerializeField]
	private PlayerShipType m_ShipType;

	public IReadOnlyList<ShipModuleSettings> Modules => m_Modules;

	public int CrewCount
	{
		get
		{
			int valueOrDefault = m_CrewCount.GetValueOrDefault();
			if (!m_CrewCount.HasValue)
			{
				valueOrDefault = Modules.Sum((ShipModuleSettings x) => x.CrewCount);
				m_CrewCount = valueOrDefault;
			}
			return m_CrewCount.Value;
		}
	}

	public List<WeaponSlotData> Weapons => HullSlots.Weapons;

	public string PlayerShipName => m_PlayerShipName;

	public string PlayerShipDescription => m_PlayerShipDescription;

	public string PlayerShipFlavor => m_PlayerShipFlavor;

	public PlayerShipType ShipType => m_ShipType;

	public BlueprintAreaEnterPoint ShipAreaEnterPoint => m_ShipAreaEnterPoint?.Get();

	[CanBeNull]
	public BlueprintStarshipSoundSettings SoundSettings => m_SoundSettings;
}
