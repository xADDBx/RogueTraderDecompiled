using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFormationUIHandlers : ISubscriber
{
	void CurrentFormationChanged(int currentFormationIndex);
}
