using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Visual.CharacterSystem;

namespace Kingmaker.Cheats;

internal class CheatsSilly
{
	public static void RegisterCheats(KeyboardAccess keyboard)
	{
	}

	private static void UpdateFeedAbility(bool add)
	{
		BlueprintAbility sillyFeedAbility = BlueprintRoot.Instance.Cheats.SillyFeedAbility;
		if (!sillyFeedAbility)
		{
			return;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		if (mainCharacterEntity == null)
		{
			return;
		}
		if (add)
		{
			if (!mainCharacterEntity.Abilities.Contains(sillyFeedAbility))
			{
				mainCharacterEntity.AddFact(sillyFeedAbility);
			}
		}
		else
		{
			mainCharacterEntity.Facts.Remove(sillyFeedAbility);
		}
	}

	private static void UpdatePartyShirt(bool useShort)
	{
		KingmakerEquipmentEntity sillyShirt = BlueprintRoot.Instance.Cheats.SillyShirt;
		if (!sillyShirt)
		{
			return;
		}
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (allCharacter.IsInGame && allCharacter.View != null && allCharacter.View.CharacterAvatar != null)
			{
				IEnumerable<EquipmentEntity> ees = sillyShirt.Load(allCharacter.Gender, allCharacter.Progression.Race.RaceId);
				if (useShort)
				{
					allCharacter.View.CharacterAvatar.AddEquipmentEntities(ees);
				}
				else
				{
					allCharacter.View.CharacterAvatar.RemoveEquipmentEntities(ees);
				}
			}
		}
	}

	private static void UpdatePartyNoArmor(bool b)
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (allCharacter.IsInGame && allCharacter.View != null && allCharacter.View.CharacterAvatar != null)
			{
				IEnumerable<EquipmentEntity> ees = allCharacter.Body.AllSlots.SelectMany(allCharacter.View.ExtractEquipmentEntities);
				allCharacter.View.CharacterAvatar.RemoveEquipmentEntities(ees);
				allCharacter.View.UpdateBodyEquipmentModel();
			}
		}
	}
}
