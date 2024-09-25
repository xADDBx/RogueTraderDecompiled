namespace Kingmaker.Networking;

public readonly struct DataTransferProgressInfo
{
	public readonly int ProgressChange;

	public readonly int CurrentProgress;

	public readonly int FullProgress;

	public DataTransferProgressInfo(int progressChange, int currentProgress, int fullProgress)
	{
		ProgressChange = progressChange;
		CurrentProgress = currentProgress;
		FullProgress = fullProgress;
	}
}
