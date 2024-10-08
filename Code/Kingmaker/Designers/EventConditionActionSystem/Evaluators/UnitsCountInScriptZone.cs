using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects.SriptZones;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("31dd684969ea477393d08b136c0d5d3f")]
public class UnitsCountInScriptZone : IntEvaluator
{
	[AllowedEntityType(typeof(ScriptZone))]
	public EntityReference ScriptZone;

	public override string GetCaption()
	{
		return $"Count of units in script zone {ScriptZone}";
	}

	protected override int GetValueInternal()
	{
		ScriptZone scriptZone = (ScriptZone)ScriptZone.FindView();
		if (scriptZone == null)
		{
			Element.LogError(this, "ScriptZone {0} is missing", ScriptZone);
			return 0;
		}
		return scriptZone.Count;
	}
}
