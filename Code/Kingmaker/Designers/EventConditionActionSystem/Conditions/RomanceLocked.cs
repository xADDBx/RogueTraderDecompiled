using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("2673f0b17f1818c49b78926d72c57b35")]
public class RomanceLocked : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Romance")]
	private BlueprintRomanceCounterReference m_Romance;

	public override string GetDescription()
	{
		return $"Проверяет, залочен ли романс {m_Romance.Get()}\n" + "ВНИМАНИЕ! Флаги, используемые в романсе, анлокаются при первом обращении к романсу и остаются анлокнутыми даже после лока романса.\nПри работе с романсами не используйте блоки для работы с флагами.";
	}

	protected override string GetConditionCaption()
	{
		return $"Romance {m_Romance.Get()} locked";
	}

	protected override bool CheckCondition()
	{
		return m_Romance.Get().IsLocked;
	}
}
