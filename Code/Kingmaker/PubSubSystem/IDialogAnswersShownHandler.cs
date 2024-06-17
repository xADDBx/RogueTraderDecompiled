using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogAnswersShownHandler : ISubscriber
{
	void HandleAnswersShown();
}
