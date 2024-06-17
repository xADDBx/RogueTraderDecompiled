namespace Kingmaker.Stores.DlcInterfaces;

public interface IDlcStore
{
	bool IsSuitable { get; }

	bool AllowsPurchase { get; }

	IDLCStatus GetStatus();

	bool OpenShop();

	bool Mount();
}
