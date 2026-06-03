using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

namespace Kingmaker.PubSubSystem;

public interface IUnitContextActionKillHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleOnContextActionKill(MechanicEntity caster, MechanicEntity target, BlueprintMechanicEntityFact blueprint, RulePerformSavingThrow rule);
}
