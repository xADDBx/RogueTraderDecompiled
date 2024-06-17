using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetPingDialogAnswer : ISubscriber
{
	void HandleDialogAnswerHover(string answer, bool hover);

	void HandleDialogAnswerVote(NetPlayer player, string answer);
}
