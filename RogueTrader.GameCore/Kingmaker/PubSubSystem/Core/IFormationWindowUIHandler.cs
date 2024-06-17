using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IFormationWindowUIHandler : ISubscriber
{
	void HandleOpenFormation();

	void HandleCloseFormation();
}
