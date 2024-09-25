using Kingmaker.Controllers.Dialog;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDialogCueHandler : ISubscriber
{
	void HandleOnCueShow(CueShowData cueShowData);
}
