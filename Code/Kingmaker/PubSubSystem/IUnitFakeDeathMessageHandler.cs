using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitFakeDeathMessageHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitFakeDeathMessage(LocalizedString fakeDeathMessage);
}
