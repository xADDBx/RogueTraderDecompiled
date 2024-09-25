using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.PubSubSystem;

public interface INonStackModifierHandler : ISubscriber
{
	void HandleNonStackModifierAdded(UnitPartNonStackBonuses unitPart, ModifiableValue modifiable, ModifiableValue.Modifier mod);
}
