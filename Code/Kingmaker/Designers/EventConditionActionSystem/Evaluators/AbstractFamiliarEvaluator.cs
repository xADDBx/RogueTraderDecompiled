using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("35d0c0936813454197973cab07403552")]
public abstract class AbstractFamiliarEvaluator : AbstractUnitEvaluator
{
	protected abstract BaseUnitEntity Leader { get; }

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Leader?.GetFamiliarLeaderOptional()?.FirstFamiliar;
	}
}
