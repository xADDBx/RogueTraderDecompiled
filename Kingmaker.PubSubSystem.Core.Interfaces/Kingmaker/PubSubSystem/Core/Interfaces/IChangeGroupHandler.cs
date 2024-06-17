namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface IChangeGroupHandler : ISubscriber
{
	void HandleChangeGroup(string unitUniqueId);
}
