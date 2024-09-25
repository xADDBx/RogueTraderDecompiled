using Newtonsoft.Json;

namespace Kingmaker.UnitLogic;

public interface IFactWithRanks
{
	[JsonProperty]
	int Rank { get; }

	void AddRank(int count = 1);

	void RemoveRank(int count = 1);
}
