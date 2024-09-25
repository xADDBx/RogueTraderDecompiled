using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Random;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("044aea76c9fc23843b947396486cb518")]
public class StarshipReloadByAmmo : ContextAction
{
	public StarshipWeaponType WeaponType;

	[SerializeField]
	private BlueprintStarshipAmmoReference m_AmmoReference;

	public int reloadChancesForMatching;

	public int reloadChancesOtherwise;

	public ActionList ReloadActions;

	public BlueprintStarshipAmmo AmmoReference => m_AmmoReference?.Get();

	public override string GetCaption()
	{
		return "Run actions if starship weapon requires reloading";
	}

	protected override void RunAction()
	{
		if (!(base.Context?.MaybeCaster is StarshipEntity starshipEntity))
		{
			return;
		}
		foreach (ItemEntityStarshipWeapon item in starshipEntity.Hull.Weapons.Where((ItemEntityStarshipWeapon weapon) => weapon.Blueprint.WeaponType == WeaponType && weapon.Charges == 0))
		{
			int num = ((item.Ammo?.Blueprint == AmmoReference) ? reloadChancesForMatching : reloadChancesOtherwise);
			if (num >= 100 || (num > 0 && PFStatefulRandom.SpaceCombat.Range(0, 100) < num))
			{
				ReloadActions.Run();
			}
		}
	}
}
