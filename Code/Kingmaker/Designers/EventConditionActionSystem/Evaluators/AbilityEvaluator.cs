using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UnitLogic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("537af97113c948e78f26ecc655d73668")]
public class AbilityEvaluator : GenericEvaluator<AbilityData>
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_Blueprint;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Caster;

	public BlueprintAbility Blueprint => m_Blueprint;

	public override string GetCaption()
	{
		return $"Ability [{Blueprint}] from [{Caster}]";
	}

	protected override AbilityData GetValueInternal()
	{
		if (!(Caster.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Evaluator {this}, {Caster} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return null;
		}
		foreach (Ability rawFact in baseUnitEntity.Abilities.RawFacts)
		{
			if (rawFact.Blueprint == Blueprint)
			{
				return rawFact.Data;
			}
		}
		foreach (Spellbook spellbook in baseUnitEntity.Spellbooks)
		{
			foreach (AbilityData allKnownSpell in spellbook.GetAllKnownSpells())
			{
				if (allKnownSpell.Blueprint == Blueprint)
				{
					return allKnownSpell;
				}
			}
		}
		throw new FailToEvaluateException(this);
	}
}
