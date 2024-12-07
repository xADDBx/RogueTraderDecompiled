using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("852774e73084467cb52eda217e6e0765")]
public class AddDelayedSoulMarkShift : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public BaseUnitEvaluator Unit;

	[SerializeField]
	private SoulMarkDirection m_Direction;

	[SerializeField]
	private int m_Value;

	public override string GetDescription()
	{
		return $"Добавляет юниту {Unit} отложенные конвикшен поинты {m_Value} в {m_Direction}";
	}

	public override string GetCaption()
	{
		return $"Add Delayed {m_Value} conviction points  in {m_Direction} SoulMark to {Unit})";
	}

	protected override void RunAction()
	{
		Unit.GetValue().AddDelayedSoulMarkValue(m_Direction, m_Value);
	}
}
