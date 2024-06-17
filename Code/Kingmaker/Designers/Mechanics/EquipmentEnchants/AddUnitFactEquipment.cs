using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.EquipmentEnchants;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintItemEnchantment))]
[TypeId("2c3f9a0ccd69bd34eb2ba171172f8ee6")]
public class AddUnitFactEquipment : ItemEnchantmentComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitFactReference m_Blueprint;

	public BlueprintUnitFact Blueprint => m_Blueprint?.Get();

	protected override void OnActivate()
	{
		if (base.Owner.Wielder != null && base.Owner.Wielder.Facts.FindBySource(Blueprint, base.Fact, this) == null)
		{
			base.Owner.Wielder.AddFact(Blueprint).AddSource(base.Fact, this);
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
