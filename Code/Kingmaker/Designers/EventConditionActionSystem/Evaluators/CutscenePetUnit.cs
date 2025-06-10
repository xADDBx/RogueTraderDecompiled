using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("712982c8d20b4248b196a035787cc9d8")]
public class CutscenePetUnit : AbstractUnitEvaluator
{
	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		Dictionary<string, object> dictionary = ContextData<NamedParametersContext.ContextData>.Current?.Context?.Params;
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue("Pet", out var value))
		{
			if (value is string uniqueId)
			{
				UnitReference unitReference = new UnitReference(uniqueId);
				dictionary["Pet"] = unitReference;
				return unitReference.ToAbstractUnitEntity();
			}
			return (value as IEntityRef)?.Get<AbstractUnitEntity>();
		}
		return null;
	}

	public override string GetCaption()
	{
		return "Pet Linked to Cutscene";
	}
}
