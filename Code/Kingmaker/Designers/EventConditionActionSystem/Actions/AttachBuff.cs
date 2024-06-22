using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/AttachBuff")]
[PlayerUpgraderAllowed(false)]
[AllowMultipleComponents]
[TypeId("0c996f778c13abb408bdd05f7f6fe317")]
public class AttachBuff : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[SerializeReference]
	public IntEvaluator Duration;

	public bool UseEndCondition;

	[ShowIf("UseEndCondition")]
	public BuffEndCondition EndCondition;

	[Tooltip("If action runs in AbilityExecutionContext - add ability fact as source")]
	public bool AddFactSource;

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetDescription()
	{
		string arg = (Duration ? Duration.ToString() : "бесконечно");
		return $"Навешивает на цель {Target} бафф {Buff} на время в раундах: {arg}";
	}

	protected override void RunAction()
	{
		if (Target == null)
		{
			throw new Exception("Trying to attach buff to null target");
		}
		BuffEndCondition endCondition = (UseEndCondition ? EndCondition : BuffEndCondition.RemainAfterCombat);
		Rounds? rounds = (Duration ? new Rounds?(Duration.GetValue().Rounds()) : null);
		BuffDuration duration = new BuffDuration(rounds, endCondition);
		Buff buff = Target.GetValue().Buffs.Add(Buff, duration);
		AddSource(buff);
	}

	private void AddSource(Buff buff)
	{
		if (buff != null && !TryAddAbilitySource(buff))
		{
			buff?.TryAddSource(this);
		}
	}

	private bool TryAddAbilitySource(Buff buff)
	{
		if (!AddFactSource)
		{
			return false;
		}
		if (buff == null)
		{
			return false;
		}
		if (!(ContextData<MechanicsContext.Data>.Current?.Context is AbilityExecutionContext abilityExecutionContext))
		{
			return false;
		}
		Ability fact = abilityExecutionContext.Ability.Fact;
		if (fact == null)
		{
			return false;
		}
		buff.AddSource(fact);
		return true;
	}

	public override string GetCaption()
	{
		return $"Attach Buff ({Buff}) to ({Target})";
	}
}
