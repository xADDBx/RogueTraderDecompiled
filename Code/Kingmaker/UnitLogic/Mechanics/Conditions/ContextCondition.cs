using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("cfd45b6275ae2234cb9b5d7d4d10c02e")]
public abstract class ContextCondition : Condition
{
	[CanBeNull]
	protected AbilityExecutionContext AbilityContext => Context as AbilityExecutionContext;

	protected MechanicsContext Context => ContextData<MechanicsContext.Data>.Current?.Context;

	protected TargetWrapper Target => ContextData<MechanicsContext.Data>.Current?.CurrentTarget;
}
