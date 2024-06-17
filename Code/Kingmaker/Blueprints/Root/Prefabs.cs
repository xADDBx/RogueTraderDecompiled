using System;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.ResourceLinks;
using Kingmaker.UI;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class Prefabs
{
	public Mesh UnitCoreCollider;

	public float CoreColliderHeight = 0.7f;

	public float SecondaryColliderWidthCoeff = 1.3f;

	public GameObject StonedFxPrefab;

	public GameObject PersonalEnemyFxPrefab;

	public DroppedLoot DroppedLootBag;

	public PrefabLink DroppedLootBagAttachedLink;

	public DroppedLoot BreathOfMoneyLootBag;

	[ValidateIsPrefab]
	[ValidateHasComponent(typeof(EquipmentOffsets))]
	public EquipmentOffsets DefaultConsumableOffsets;

	public SectorMapPassageView WarpPassagePrefab;

	public DissolveSettings FogOfWarDissolveSettings;

	[AssetPicker("")]
	public UICamera UICamera;

	[AssetPicker("")]
	public UnitEntityView EmptyUnit;

	public CharacterBonesSetup CharacterBonesSetup;

	public GameObject DefaultInteractWithMeltaChargeFxPrefab;

	public GameObject StarSystemPlayerShip;

	[ValidateIsPrefab]
	[ValidateNotNull]
	[ValidateHasComponent(typeof(VideoPlayerHelper))]
	public GameObject PlayVideoCanvas;
}
