using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

public interface IInterchapterHandler : ISubscriber
{
	void StartInterchapter(InterchapterData data);

	void StopInterchapter(InterchapterData data);
}
