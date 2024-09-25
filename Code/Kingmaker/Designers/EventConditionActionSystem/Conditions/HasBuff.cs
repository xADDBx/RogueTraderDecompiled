using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/HasBuff")]
[AllowMultipleComponents]
[TypeId("0cbc5ea4bddfb1543be0d2e54a3eacd0")]
public class HasBuff : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override string GetConditionCaption()
	{
		return $"Has Buff ({Buff})";
	}

	protected override bool CheckCondition()
	{
		if (!(Target?.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {Target} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return false;
		}
		foreach (Buff buff in baseUnitEntity.Buffs)
		{
			if (buff.Blueprint == Buff)
			{
				return true;
			}
		}
		return false;
	}
}
