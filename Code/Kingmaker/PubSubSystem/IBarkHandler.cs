using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBarkHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleOnShowBark(string text);

	void HandleOnShowLinkedBark(string text, string encyclopediaLink);

	void HandleOnHideBark();
}
