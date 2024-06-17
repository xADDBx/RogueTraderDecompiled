using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogFinishHandler : ISubscriber
{
	void HandleDialogFinished(BlueprintDialog dialog, bool success);
}
