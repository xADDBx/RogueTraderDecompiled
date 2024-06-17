using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/UnitInScriptZone")]
[AllowMultipleComponents]
[TypeId("f12047e0588c11f4ba28423a0b8c3e8e")]
public class UnitInScriptZone : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator ScriptZone;

	protected override string GetConditionCaption()
	{
		return $"{Unit} is in {ScriptZone}";
	}

	protected override bool CheckCondition()
	{
		ScriptZone scriptZone = ScriptZone.GetValue().View as ScriptZone;
		if (scriptZone != null && scriptZone.Data.ContainsUnit(Unit.GetValue()))
		{
			return true;
		}
		return false;
	}
}
