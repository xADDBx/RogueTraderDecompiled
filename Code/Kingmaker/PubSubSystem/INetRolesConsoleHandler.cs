using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetRolesConsoleHandler : ISubscriber
{
	void HandleUpdateCharactersNavigation(UnitReference focusCharacter);
}
