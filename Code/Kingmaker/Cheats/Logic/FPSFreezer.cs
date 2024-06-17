using System.Threading;
using UnityEngine;

namespace Kingmaker.Cheats.Logic;

public class FPSFreezer : MonoBehaviour
{
	public int MinSleepMs = 10;

	public int MaxSleepMs = 10;

	private void Update()
	{
		Thread.Sleep(Random.Range(MinSleepMs, MaxSleepMs));
	}
}
