using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("4ab8c580c9281bc47a38915cde4fe704")]
public class StarShipRapidReloadAction : ContextAction
{
	public bool AllowPenalted;

	[ShowIf("AllowPenalted")]
	public ActionList ActionsWhenReloadedPenalted;

	public bool AllowReloadAtAccelerationPhase;

	[ShowIf("AllowReloadAtAccelerationPhase")]
	public StarshipWeaponType ReloadWeapon;

	public override string GetCaption()
	{
		return "Spaceship RapidReload action";
	}

	protected override void RunAction()
	{
		if (!(base.Context?.MaybeCaster is StarshipEntity starshipEntity))
		{
			return;
		}
		if (starshipEntity.Navigation.IsAccelerationMovementPhase && AllowReloadAtAccelerationPhase)
		{
			foreach (WeaponSlot item in starshipEntity.Hull.WeaponSlots.Where(delegate(WeaponSlot slot)
			{
				if (slot.Weapon.Blueprint.WeaponType == ReloadWeapon)
				{
					ItemEntity sourceItem = slot.ActiveAbility.Data.SourceItem;
					if (sourceItem == null)
					{
						return false;
					}
					return sourceItem.Charges == 0;
				}
				return false;
			}))
			{
				item.Weapon.Reload();
			}
		}
		StarShipUnitPartRapidReload optional = starshipEntity.GetOptional<StarShipUnitPartRapidReload>();
		if (optional != null && optional.ReloadWeapons(AllowPenalted))
		{
			ActionsWhenReloadedPenalted.Run();
		}
	}
}
