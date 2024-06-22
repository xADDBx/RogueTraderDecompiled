using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("5f33a20511714a6198dab30ec21f3f15")]
public class RomanceSetMaximum : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintRomanceCounterReference m_Romance;

	[SerializeReference]
	public IntEvaluator ValueEvaluator;

	public BlueprintRomanceCounter Romance => m_Romance?.Get();

	public override string GetDescription()
	{
		return $"Выставляет новый максимум {ValueEvaluator} для романса {Romance}\n" + "ВНИМАНИЕ! Флаги, используемые в романсе, анлокаются при первом обращении к романсу и остаются анлокнутыми даже после лока романса.\nПри работе с романсами не используйте блоки для работы с флагами.";
	}

	protected override void RunAction()
	{
		int value = ValueEvaluator.GetValue();
		Romance.UnlockFlags();
		Romance.MaxValueFlag.Value = value;
		if (Romance.CounterFlag.Value > value)
		{
			Romance.CounterFlag.Value = value;
		}
	}

	public override string GetCaption()
	{
		return $"Set romance {Romance} max to {ValueEvaluator}";
	}
}
