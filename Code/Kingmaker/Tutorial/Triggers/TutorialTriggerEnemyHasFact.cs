using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Tutorial.Solvers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("Triggers on enter combat against enemy with EnemyFact, if party member has one of the listed spells (if needed).\n't|TargetUnit' - mob with necessary fact\n't|SourceUnit' - ally with one of the listed spells (if NeedAllySpell = false, SourceUnit always null).")]
[AllowMultipleComponents]
[TypeId("e0e0a326bf8ab8e44a1c5a330fede01e")]
public class TutorialTriggerEnemyHasFact : TutorialTriggerEnterCombatWithUnit, IHashable
{
	private BaseUnitEntity m_Unit;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_EnemyFact;

	[SerializeField]
	[ValidateNoNullEntries]
	[ValidateNotEmpty]
	private BlueprintAbilityReference[] m_Spells;

	[SerializeField]
	private bool m_AllowItemsWithSpell;

	public BlueprintUnitFact EnemyFact => m_EnemyFact?.Get();

	public ReferenceArrayProxy<BlueprintAbility> Spells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] spells = m_Spells;
			return spells;
		}
	}

	protected override bool IsSuitableUnit(BaseUnitEntity unit)
	{
		if (!unit.Facts.Contains(EnemyFact))
		{
			return false;
		}
		IEnumerator<AbilityData> enumerator = PartySpellsEnumerator.Get(withAbilities: true);
		while (enumerator.MoveNext())
		{
			AbilityData current = enumerator.Current;
			if (!(current == null) && (m_AllowItemsWithSpell || current.SourceItem == null) && Spells.HasReference(current.Blueprint))
			{
				m_Unit = current.Caster as BaseUnitEntity;
				return true;
			}
		}
		return false;
	}

	protected override void OnSetupContext(TutorialContext context, BaseUnitEntity unit)
	{
		context[TutorialContextKey.TargetUnit] = unit;
		context.RevealUnitInfo = unit;
		context.SourceUnit = m_Unit;
		m_Unit = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
