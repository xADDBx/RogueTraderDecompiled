using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("bc21b4860a4e0284eb894e8448f8958b")]
public abstract class BlueprintStarshipItem : BlueprintItemEquipment
{
	public StarshipEquipmentEntity StarshipEE;

	public bool IsBroken;

	[SerializeField]
	public List<BlueprintPartsCargoReference> AssembleItemRequirements;

	public int AssembleItemRequiredScrap;

	public int DisassembleScrapGiven;
}
