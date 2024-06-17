using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetRoleHighlight : ISubscriber
{
	void HandleRoleHighlight(UnitReference unit, bool highlight);
}
