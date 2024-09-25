using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.PubSubSystem;

public interface IUIContextActionKillHandler : ISubscriber
{
	void HandleOnContextActionKill(MechanicEntity caster, MechanicEntity target, BlueprintMechanicEntityFact blueprint, RulePerformSavingThrow rule);
}
