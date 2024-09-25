using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.EntitySystem.Persistence;

public interface ISavesUpdatedHandler : ISubscriber
{
	void OnSaveListUpdated();
}
