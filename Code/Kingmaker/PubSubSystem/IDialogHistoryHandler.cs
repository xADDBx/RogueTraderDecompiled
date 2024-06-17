using Kingmaker.Controllers.Dialog;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogHistoryHandler : ISubscriber
{
	void HandleOnDialogHistory(IDialogShowData showData);
}
