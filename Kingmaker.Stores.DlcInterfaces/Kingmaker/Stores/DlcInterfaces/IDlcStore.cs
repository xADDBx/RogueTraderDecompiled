namespace Kingmaker.Stores.DlcInterfaces;

public interface IDlcStore
{
	bool IsSuitable { get; }

	bool AllowsPurchase { get; }

	bool AllowsInstalling { get; }

	bool AllowsDeleting { get; }

	bool ComingSoon { get; }

	bool TryGetStatus(out IDLCStatus value);

	bool OpenShop();

	bool Mount();

	bool Install();

	bool Delete();
}
