using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.UnitLogic;

namespace Kingmaker;

[TypeId("01a02b734f4ec824085cbe26c6e563aa")]
public class FixDuplicatedWeaponFactsOnUnit : PlayerUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Deranks all weapon facts from party members and removes facts that are applied by weapons that aren't equipped";
	}

	protected override void RunActionOverride()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			foreach (Feature item in new List<Feature>(allCharacter.Facts.GetAll<Feature>()))
			{
				foreach (EntityFactSource item2 in new List<EntityFactSource>(item.Sources))
				{
					if (item2?.Entity is ItemEntityWeapon itemEntityWeapon)
					{
						DerankUntilFirstRank(item);
						if (allCharacter.Body.PrimaryHand.MaybeItem != itemEntityWeapon && allCharacter.Body.SecondaryHand.MaybeItem != itemEntityWeapon)
						{
							allCharacter.Facts.Remove(item);
						}
					}
				}
			}
		}
	}

	private void DerankUntilFirstRank(Feature feature)
	{
		int num = feature.GetRank() - 1;
		for (int i = 0; i < num; i++)
		{
			feature.RemoveRank();
		}
	}
}
