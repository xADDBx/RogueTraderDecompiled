using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetSaveUploadDownloadErrorHandler : ISubscriber
{
	void HandleSaveSourceDisconnectedError();

	void HandleSaveReceiveError();

	void HandleSaveNotFoundError();

	void HandleSendMessageFailError();

	void HandleUnknownException();
}
