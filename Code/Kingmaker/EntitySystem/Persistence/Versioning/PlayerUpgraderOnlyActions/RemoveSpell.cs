using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UnitLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("a29087832b9e49a694a8b842e5f48d4a")]
public class RemoveSpell : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_Spell;

	[SerializeField]
	[InfoBox("Remove from all spellbooks if not specified")]
	private BlueprintSpellbookReference m_Spellbook;

	public BlueprintAbility Spell => m_Spell;

	[CanBeNull]
	public BlueprintSpellbook Spellbook => m_Spellbook;

	public override string GetCaption()
	{
		if (Spellbook == null)
		{
			return $"Remove spell {Spell}";
		}
		return $"Remove spell {Spell} from {Spellbook}";
	}

	protected override void RunActionOverride()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			Upgrade(allCharacter);
		}
	}

	private void Upgrade(BaseUnitEntity unit)
	{
		foreach (Spellbook spellbook in unit.Spellbooks)
		{
			if (Spellbook == null || spellbook.Blueprint == Spellbook)
			{
				spellbook.RemoveSpell(Spell);
			}
		}
	}
}
