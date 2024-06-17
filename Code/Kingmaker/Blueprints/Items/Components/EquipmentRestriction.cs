using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItemEquipment))]
[TypeId("7f0e948cbdc624e43b28a69961c69517")]
public abstract class EquipmentRestriction : BlueprintComponent
{
	public abstract bool CanBeEquippedBy(MechanicEntity unit);
}
