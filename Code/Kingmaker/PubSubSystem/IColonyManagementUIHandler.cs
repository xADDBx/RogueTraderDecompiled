using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonyManagementUIHandler : ISubscriber
{
	void HandleColonyManagementPage(Colony colony);
}
