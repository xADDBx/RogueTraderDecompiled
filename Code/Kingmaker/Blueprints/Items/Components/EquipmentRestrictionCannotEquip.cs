using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[TypeId("2e766de45e01f4240ad42efa31128e69")]
public class EquipmentRestrictionCannotEquip : EquipmentRestriction
{
	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		return false;
	}
}
