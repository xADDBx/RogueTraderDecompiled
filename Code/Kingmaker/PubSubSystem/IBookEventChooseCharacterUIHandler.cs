using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBookEventChooseCharacterUIHandler : ISubscriber
{
	void HandleSelect(int index);

	void HandleHighlight(int index);
}
