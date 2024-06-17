using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

public class StarSystemData : IHashable
{
	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
