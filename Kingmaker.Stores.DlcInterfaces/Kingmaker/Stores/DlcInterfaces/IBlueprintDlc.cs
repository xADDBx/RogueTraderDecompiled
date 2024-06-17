using System.Collections.Generic;

namespace Kingmaker.Stores.DlcInterfaces;

public interface IBlueprintDlc
{
	string DlcDisplayName { get; }

	string DlcDescription { get; }

	string Id { get; }

	IEnumerable<IBlueprintDlcReward> Rewards { get; }

	bool IsAvailable { get; }

	IEnumerable<IDlcStore> GetDlcStores();
}
