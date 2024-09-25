using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.EquipmentEnchants;

[AllowedOn(typeof(BlueprintItemEnchantment))]
[AllowMultipleComponents]
[TypeId("01f56971790b338448973ff85009d309")]
public class AddUnitFeatureEquipment : ItemEnchantmentComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public BlueprintFeature Feature => m_Feature?.Get();

	protected override void OnActivate()
	{
		MechanicEntity wielder = base.Owner.Wielder;
		if (wielder != null && wielder.Facts.FindBySource(Feature, base.Fact, this) == null)
		{
			wielder.AddFact(Feature, base.Context).AddSource(base.Fact, this);
		}
	}

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner.Wielder);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
