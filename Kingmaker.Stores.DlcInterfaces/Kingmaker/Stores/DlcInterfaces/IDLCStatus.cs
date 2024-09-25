namespace Kingmaker.Stores.DlcInterfaces;

public interface IDLCStatus
{
	bool Purchased { get; set; }

	bool IsMounted { get; set; }

	DownloadState DownloadState { get; set; }
}
