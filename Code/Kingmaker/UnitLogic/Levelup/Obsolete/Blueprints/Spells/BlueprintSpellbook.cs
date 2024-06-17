using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[TypeId("a42bdead3c333d744a3f0354bbe6c27c")]
public class BlueprintSpellbook : BlueprintScriptableObject, IUIDataProvider
{
	public const int MaxPossibleSpellLevel = 10;

	public LocalizedString Name;

	public bool IsMythic;

	[ValidateNotNull]
	[Tooltip("Spells per day table. Further modified by casting stat.")]
	[SerializeField]
	private BlueprintSpellsTableReference m_SpellsPerDay;

	[CanBeNull]
	[Tooltip("Spells known table. Only for spontaneous casters (but not for Arcanist).")]
	[SerializeField]
	private BlueprintSpellsTableReference m_SpellsKnown;

	[Tooltip("Spell slots table for arcanist.")]
	[SerializeField]
	[ShowIf("IsArcanist")]
	private BlueprintSpellsTableReference m_SpellSlots;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintSpellListReference m_SpellList;

	[SerializeField]
	[ShowIf("IsMythic")]
	private BlueprintSpellListReference m_MythicSpellList;

	[SerializeField]
	private BlueprintCharacterClassReference m_CharacterClass;

	public StatType CastingAttribute;

	[Tooltip("Spontaneous casting (sorcerers, oracles, bard)")]
	public bool Spontaneous;

	[Tooltip("Spells count learned on each level. For wizards.")]
	public int SpellsPerLevel;

	[Tooltip("No need to learn spells to memorize them. For clerics / druids / rangers.")]
	public bool AllSpellsKnown;

	[Tooltip("For UI name.")]
	public CantripsType CantripsType;

	[Tooltip("For Ranger - his caster level is class level minus 3")]
	public int CasterLevelModifier;

	public bool CanCopyScrolls;

	public bool IsArcane = true;

	public bool IsArcanist;

	[FormerlySerializedAs("HasSpecialSpelllist")]
	public bool HasSpecialSpellList;

	[FormerlySerializedAs("SpecialSpelllistName")]
	[ShowIf("HasSpecialSpellList")]
	public LocalizedString SpecialSpellListName;

	public string DisplayName => Name;

	public BlueprintSpellsTable SpellsPerDay => m_SpellsPerDay?.Get();

	public BlueprintSpellsTable SpellsKnown => m_SpellsKnown?.Get();

	public BlueprintSpellsTable SpellSlots => m_SpellSlots?.Get() ?? SpellsPerDay;

	public BlueprintSpellList SpellList => m_SpellList?.Get();

	public BlueprintSpellList MythicSpellList => m_MythicSpellList?.Get();

	public BlueprintCharacterClass CharacterClass => m_CharacterClass?.Get();

	public bool MemorizeSpells
	{
		get
		{
			if (Spontaneous)
			{
				return IsArcanist;
			}
			return true;
		}
	}

	public bool IsAlchemist => false;

	public bool IsSinMagicSpecialist => false;

	public int MaxSpellLevel
	{
		get
		{
			int result = -1;
			for (int i = 0; i <= 10; i++)
			{
				if (SpellsPerDay.GetCount(SpellsPerDay.Levels.Length - 1, i).HasValue)
				{
					result = i;
				}
			}
			return result;
		}
	}

	string IUIDataProvider.Name => Name;

	string IUIDataProvider.Description => Name;

	Sprite IUIDataProvider.Icon => null;

	string IUIDataProvider.NameForAcronym => name;
}
