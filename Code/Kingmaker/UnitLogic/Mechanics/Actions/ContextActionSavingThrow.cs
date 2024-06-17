using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("5e2f2046f3bd6984c8833bf019c8e8ad")]
public class ContextActionSavingThrow : ContextAction
{
	[Serializable]
	private struct ConditionalDCIncrease
	{
		public ConditionsChecker Condition;

		public ContextValue Value;
	}

	public SavingThrowType Type;

	public bool FromBuff;

	[SerializeField]
	private ConditionalDCIncrease[] m_ConditionalDCIncrease = new ConditionalDCIncrease[0];

	public bool HasCustomDC;

	[ShowIf("HasCustomDC")]
	public ContextValue CustomDC;

	public ActionList Actions;

	public override string GetCaption()
	{
		return "Saving throw " + Type;
	}

	public override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error(this, "Can't use ContextActionSavingThrow because target is not an unit");
		}
		else
		{
			if (base.Context == null)
			{
				return;
			}
			int num = 0;
			if (m_ConditionalDCIncrease != null)
			{
				ConditionalDCIncrease[] conditionalDCIncrease = m_ConditionalDCIncrease;
				for (int i = 0; i < conditionalDCIncrease.Length; i++)
				{
					ConditionalDCIncrease conditionalDCIncrease2 = conditionalDCIncrease[i];
					if (conditionalDCIncrease2.Condition.Check())
					{
						num += conditionalDCIncrease2.Value.Calculate(base.Context);
					}
				}
			}
			int dc = (HasCustomDC ? CustomDC.Calculate(base.Context) : num);
			RulePerformSavingThrow rule = base.Context.TriggerRule(CreateSavingThrow(base.Target.Entity, dc, persistentSpell: false));
			using (ContextData<SavingThrowData>.Request().Setup(rule))
			{
				Actions.Run();
			}
		}
	}

	[CanBeNull]
	public static ContextActionApplyBuff FindApplyBuffAction(ActionList actions)
	{
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction gameAction in actions2)
		{
			ContextActionApplyBuff contextActionApplyBuff = ((gameAction is ContextActionConditionalSaved contextActionConditionalSaved) ? FindApplyBuffAction(contextActionConditionalSaved.Succeed) : (gameAction as ContextActionApplyBuff));
			if (contextActionApplyBuff != null)
			{
				return contextActionApplyBuff;
			}
		}
		return null;
	}

	private RulePerformSavingThrow CreateSavingThrow(MechanicEntity unit, int dc, bool persistentSpell)
	{
		return new RulePerformSavingThrow(unit, Type, dc)
		{
			Buff = ((!FromBuff) ? null : FindApplyBuffAction(Actions)?.Buff),
			PersistentSpell = persistentSpell
		};
	}
}
