using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICustomCombatText : ISubscriber
{
	void HandleCustomCombatText(BaseUnitEntity targetUnit, string text);
}
