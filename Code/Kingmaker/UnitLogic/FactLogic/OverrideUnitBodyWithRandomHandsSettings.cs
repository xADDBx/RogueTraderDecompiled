using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("16aa52a700244b00b8648b54c979e25a")]
[AllowedOn(typeof(BlueprintUnit))]
public class OverrideUnitBodyWithRandomHandsSettings : UnitFactComponentDelegate, IHashable
{
	public UnitItemEquipmentHandSettingsWithWeights[] SettingsWithWeights;

	public float TotalWeightPercent => SettingsWithWeights.Sum((UnitItemEquipmentHandSettingsWithWeights e) => e.Weight);

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
