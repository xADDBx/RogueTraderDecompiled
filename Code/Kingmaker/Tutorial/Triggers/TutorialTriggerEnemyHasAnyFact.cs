using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("Triggers on enter combat against enemy with any of EnemyFacts.\n't|SourceUnit' - mob with necessary fact")]
[TypeId("d2b273840d695b5488343c7d068edba0")]
public class TutorialTriggerEnemyHasAnyFact : TutorialTriggerEnterCombatWithUnit, IHashable
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference[] m_EnemyFacts;

	public ReferenceArrayProxy<BlueprintUnitFact> EnemyFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] enemyFacts = m_EnemyFacts;
			return enemyFacts;
		}
	}

	protected override bool IsSuitableUnit(BaseUnitEntity unit)
	{
		foreach (BlueprintUnitFact enemyFact in EnemyFacts)
		{
			if (unit.Facts.Contains(enemyFact))
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnSetupContext(TutorialContext context, BaseUnitEntity unit)
	{
		context[TutorialContextKey.SourceUnit] = unit;
		context[TutorialContextKey.TargetUnit] = unit;
		context.RevealUnitInfo = unit;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
