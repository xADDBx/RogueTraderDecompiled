using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogNavigationCreatedHandler : ISubscriber
{
	void HandleDialogNavigationBuildStarted();

	void HandleDialogNavigationBuildFinished();
}
