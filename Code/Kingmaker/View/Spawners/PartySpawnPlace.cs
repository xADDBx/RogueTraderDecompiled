using UnityEngine;

namespace Kingmaker.View.Spawners;

internal class PartySpawnPlace : MonoBehaviour
{
	private void OnEnable()
	{
		PFLog.Default.Error("PartySpawnPlace is obsolete, use AreaEnterPoint instead");
	}
}
