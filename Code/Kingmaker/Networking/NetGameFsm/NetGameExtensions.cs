namespace Kingmaker.Networking.NetGameFsm;

public static class NetGameExtensions
{
	public static bool CanSendGameMessages(this NetGame netGame)
	{
		NetGame.State currentState = netGame.CurrentState;
		return currentState == NetGame.State.Playing || currentState == NetGame.State.DownloadSaveAndLoading || currentState == NetGame.State.UploadSaveAndStartLoading;
	}
}
