using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Blueprints.Items.Components;

[TypeId("c037376abe12467bb7515452fc74e1c9")]
public class EquipmentRestrictionMainPlayer : EquipmentRestriction
{
	public override bool CanBeEquippedBy(MechanicEntity entity)
	{
		if (entity is UnitEntity { CopyOf: var copyOf } unitEntity)
		{
			return (copyOf.Entity ?? unitEntity) == Game.Instance.Player.MainCharacter.Entity;
		}
		return false;
	}
}
