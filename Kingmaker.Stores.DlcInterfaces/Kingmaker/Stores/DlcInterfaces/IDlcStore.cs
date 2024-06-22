namespace Kingmaker.Stores.DlcInterfaces;

public interface IDlcStore
{
	bool IsSuitable { get; }

	bool AllowsPurchase { get; }

	bool ComingSoon { get; }

	IDLCStatus GetStatus();

	bool OpenShop();

	bool Mount();
}
