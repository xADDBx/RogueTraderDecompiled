using Kingmaker.Stores.DlcInterfaces;

namespace Kingmaker.Stores;

public class DLCStatus : IDLCStatus
{
	public static readonly DLCStatus Available = new DLCStatus
	{
		Purchased = true,
		DownloadState = DownloadState.Loaded,
		IsMounted = true,
		IsEnabled = true
	};

	public bool Purchased { get; set; }

	public bool IsMounted { get; set; }

	public DownloadState DownloadState { get; set; }

	public bool IsEnabled { get; set; }

	public bool Equals(IDLCStatus other)
	{
		if (other == null)
		{
			return false;
		}
		if (Purchased == other.Purchased && IsMounted == other.IsMounted && DownloadState == other.DownloadState)
		{
			return IsEnabled == other.IsEnabled;
		}
		return false;
	}
}
