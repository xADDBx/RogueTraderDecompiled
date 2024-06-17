using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("4a8280d06cf242c5947d12391323a65e")]
public class ContextConditionCharacterClass : ContextCondition
{
	public bool CheckCaster;

	[SerializeField]
	[FormerlySerializedAs("Class")]
	private BlueprintCharacterClassReference m_Class;

	public int MinLevel;

	public BlueprintCharacterClass Class => m_Class?.Get();

	protected override string GetConditionCaption()
	{
		string arg = (CheckCaster ? "caster" : "target");
		string arg2 = ((MinLevel > 1) ? $"{MinLevel} levels" : "1 level");
		return $"{arg} has at least {arg2} in {Class}";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity mechanicEntity = (CheckCaster ? base.Context.MaybeCaster : base.Target.Entity);
		if (mechanicEntity == null)
		{
			PFLog.Default.Error("Target is missing");
			return false;
		}
		ClassData classData = mechanicEntity.GetProgressionOptional()?.GetClassData(Class);
		if (classData != null)
		{
			return classData.Level >= MinLevel;
		}
		return false;
	}
}
