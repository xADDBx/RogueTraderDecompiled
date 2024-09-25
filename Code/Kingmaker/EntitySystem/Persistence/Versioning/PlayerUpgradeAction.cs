using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Code.UnitLogic;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public class PlayerUpgradeAction : IHashable
{
	[JsonProperty]
	public PlayerUpgradeActionType Type;

	[JsonProperty]
	public BlueprintScriptableObject Blueprint;

	public void Apply()
	{
		switch (Type)
		{
		case PlayerUpgradeActionType.GiveObjective:
			if (Blueprint is BlueprintQuestObjective objecive3)
			{
				GameHelper.Quests.GiveObjective(objecive3);
			}
			break;
		case PlayerUpgradeActionType.CompleteObjective:
			if (Blueprint is BlueprintQuestObjective objecive)
			{
				GameHelper.Quests.CompleteObjective(objecive);
			}
			break;
		case PlayerUpgradeActionType.FailObjective:
			if (Blueprint is BlueprintQuestObjective objecive2)
			{
				GameHelper.Quests.FailObjective(objecive2);
			}
			break;
		case PlayerUpgradeActionType.ResetObjective:
			if (Blueprint is BlueprintQuestObjective bpObjective)
			{
				Game.Instance.Player.QuestBook.ResetObjective(bpObjective);
			}
			break;
		case PlayerUpgradeActionType.GiveItem:
			if (Blueprint is BlueprintItem newBpItem)
			{
				Game.Instance.Player.Inventory.Add(newBpItem);
			}
			break;
		case PlayerUpgradeActionType.RemoveItem:
			if (Blueprint is BlueprintItem bpItem)
			{
				Game.Instance.Player.Inventory.Remove(bpItem);
			}
			break;
		case PlayerUpgradeActionType.ForgetPartySpell:
			if (!(Blueprint is BlueprintAbility spell))
			{
				break;
			}
			{
				foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
				{
					foreach (Spellbook spellbook in allCrossSceneUnit.Spellbooks)
					{
						spellbook.RemoveSpell(spell);
					}
				}
				break;
			}
		case PlayerUpgradeActionType.UnrecruitCompanion:
			UnrecruitCompanion(Blueprint as BlueprintUnit);
			break;
		case PlayerUpgradeActionType.AttachAllPartyMembers:
		{
			foreach (BaseUnitEntity allCrossSceneUnit2 in Game.Instance.Player.AllCrossSceneUnits)
			{
				UnitPartCompanion optional = allCrossSceneUnit2.GetOptional<UnitPartCompanion>();
				if (optional != null && optional.State == CompanionState.InPartyDetached)
				{
					Game.Instance.Player.AttachPartyMember(allCrossSceneUnit2);
				}
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case PlayerUpgradeActionType.MakeUnitEssentialForGame:
		case PlayerUpgradeActionType.MakeUnitNotEssentialForGame:
			break;
		}
	}

	[CanBeNull]
	private BaseUnitEntity GetCharacter(BlueprintUnit blueprint)
	{
		if (!blueprint)
		{
			return null;
		}
		return Game.Instance.Player.AllCharacters.Find((BaseUnitEntity u) => u.Blueprint == blueprint);
	}

	private void UnrecruitCompanion(BlueprintUnit blueprintUnit)
	{
		Unrecruit unrecruit = new Unrecruit();
		unrecruit.CompanionBlueprint = blueprintUnit;
		unrecruit.OnUnrecruit = new ActionList();
		unrecruit.Run();
		BaseUnitEntity character = GetCharacter(blueprintUnit);
		if (character != null)
		{
			character.IsInGame = false;
		}
	}

	private static void DeactivateFact(BaseUnitEntity unit, BlueprintUnitFact blueprint)
	{
		unit.Facts.Get(blueprint)?.Deactivate();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref Type);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		return result;
	}
}
