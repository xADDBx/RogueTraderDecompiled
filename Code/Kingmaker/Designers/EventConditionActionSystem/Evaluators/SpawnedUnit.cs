using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("5abcc51ecc3df064ebb6c4ec13a8a8e9")]
public class SpawnedUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		SpawnedUnitData current = ContextData<SpawnedUnitData>.Current;
		if (current != null)
		{
			return current.Unit;
		}
		Dictionary<string, object> dictionary = ContextData<NamedParametersContext.ContextData>.Current?.Context?.Params;
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue("Spawned", out var value))
		{
			if (value is string uniqueId)
			{
				UnitReference unitReference = new UnitReference(uniqueId);
				dictionary["Spawned"] = unitReference;
				return unitReference.ToAbstractUnitEntity();
			}
			return (value as IEntityRef)?.Get<AbstractUnitEntity>();
		}
		return null;
	}

	public override string GetCaption()
	{
		return "Spawned Unit";
	}
}
