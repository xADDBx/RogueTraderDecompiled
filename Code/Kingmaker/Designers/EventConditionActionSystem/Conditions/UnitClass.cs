using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("5630fd6e5ff6d5845b423f9495bce210")]
public class UnitClass : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	[FormerlySerializedAs("Class")]
	private BlueprintCharacterClassReference m_Class;

	[SerializeReference]
	public IntEvaluator MinLevel;

	public BlueprintCharacterClass Class => m_Class?.Get();

	protected override string GetConditionCaption()
	{
		return $"{Unit} Class is {m_Class.Get()}";
	}

	protected override bool CheckCondition()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Condition {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return false;
		}
		ClassData classData = baseUnitEntity.Progression.GetClassData(Class);
		if (classData != null)
		{
			return classData.Level >= (MinLevel ? MinLevel.GetValue() : 0);
		}
		return false;
	}
}
