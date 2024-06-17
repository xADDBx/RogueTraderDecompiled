using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetDescription()
	{
		string arg = (Duration ? Duration.ToString() : "бесконечно");
		return $"Навешивает на цель {Target} бафф {Buff} на время в раундах: {arg}";
	}

	public override void RunAction()
	{
		if (Target == null)
		{
			throw new Exception("Trying to attach buff to null target");
		}
		BuffEndCondition endCondition = (UseEndCondition ? EndCondition : BuffEndCondition.RemainAfterCombat);
		Rounds? rounds = (Duration ? new Rounds?(Duration.GetValue().Rounds()) : null);
		BuffDuration duration = new BuffDuration(rounds, endCondition);
		Target.GetValue().Buffs.Add(Buff, duration)?.TryAddSource(this);
	}

	public override string GetCaption()
	{
		return $"Attach Buff ({Buff}) to ({Target})";
	}
}
