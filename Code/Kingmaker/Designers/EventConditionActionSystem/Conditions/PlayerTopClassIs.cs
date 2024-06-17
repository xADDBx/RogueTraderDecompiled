using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("4a551886f958a84419c9ada793775f3f")]
public class PlayerTopClassIs : Condition
{
	public bool CheckGroup;

	[HideIf("CheckGroup")]
	[SerializeField]
	private BlueprintCharacterClassReference m_CharacterClass;

	[ShowIf("CheckGroup")]
	[SerializeField]
	private BlueprintCharacterClassGroupReference m_CharacterClassGroup;

	public BlueprintCharacterClass CharacterClass
	{
		get
		{
			return m_CharacterClass?.Get();
		}
		set
		{
			m_CharacterClass = value.ToReference<BlueprintCharacterClassReference>();
		}
	}

	public BlueprintCharacterClassGroup CharacterClassGroup
	{
		get
		{
			return m_CharacterClassGroup?.Get();
		}
		set
		{
			m_CharacterClassGroup = value.ToReference<BlueprintCharacterClassGroupReference>();
		}
	}

	protected override string GetConditionCaption()
	{
		if (!CheckGroup)
		{
			return $"Player top class is {CharacterClass}";
		}
		return $"Player top class is in {CharacterClassGroup} group";
	}

	protected override bool CheckCondition()
	{
		List<BlueprintCharacterClass> list = new List<BlueprintCharacterClass>();
		int num = Game.Instance.Player.MainCharacterEntity.ToBaseUnitEntity().Progression.Classes.Max((ClassData _classData) => _classData.Level);
		foreach (ClassData @class in Game.Instance.Player.MainCharacterEntity.ToBaseUnitEntity().Progression.Classes)
		{
			if (@class.Level >= num)
			{
				list.Add(@class.CharacterClass);
			}
		}
		if (CheckGroup)
		{
			if (CharacterClassGroup == null)
			{
				PFLog.Default.ErrorWithReport(this, "Character class group is not set.");
				return false;
			}
			return list.Any((BlueprintCharacterClass _characterClass1) => CharacterClassGroup.CharacterClasses.Any((BlueprintCharacterClass _characterClass2) => _characterClass1 == _characterClass2));
		}
		if (CharacterClass == null)
		{
			PFLog.Default.ErrorWithReport(this, "Character class is not set.");
			return false;
		}
		return list.Any((BlueprintCharacterClass _characterClass) => _characterClass == CharacterClass);
	}
}
