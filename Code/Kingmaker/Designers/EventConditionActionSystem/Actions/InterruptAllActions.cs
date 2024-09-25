using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5a5562d34459ae64dbcf70310e467944")]
public class InterruptAllActions : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	public override string GetDescription()
	{
		return $"Отменяет все команды у юнита {m_Unit}.";
	}

	public override string GetCaption()
	{
		return $"Interrupt all actions for {m_Unit}";
	}

	protected override void RunAction()
	{
		m_Unit.GetValue().Commands.InterruptAllInterruptible();
	}
}
