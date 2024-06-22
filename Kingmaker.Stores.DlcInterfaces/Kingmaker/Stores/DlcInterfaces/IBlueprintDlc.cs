using System.Collections.Generic;

namespace Kingmaker.Stores.DlcInterfaces;

public interface IBlueprintDlc
{
	string DlcDisplayName { get; }

	string DlcDescription { get; }

	string Id { get; }

	IEnumerable<IBlueprintDlcReward> Rewards { get; }

	bool IsAvailable { get; }

	bool IsPurchased { get; }

	bool IsActive { get; }

	DlcTypeEnum DlcType { get; }

	IEnumerable<IDlcStore> GetDlcStores();
}
