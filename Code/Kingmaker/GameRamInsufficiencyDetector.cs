using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA.Analytics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker;

public class GameRamInsufficiencyDetector : IAreaLoadingStagesHandler, ISubscriber
{
	private static GameRamInsufficiencyDetector s_Instance;

	private long m_MemoryRedLimit = 7516192768L;

	private long m_MemoryYellowLimit = 7516192768L;

	private readonly TimeSpan m_Delay = TimeSpan.FromSeconds(1.0);

	private string CurrentArea = string.Empty;

	private DateTimeOffset CurrentAreaEnterTime = DateTimeOffset.Now;

	private bool YellowSent;

	private bool RedSent;

	private readonly IList<byte[]> m_Mbs = new List<byte[]>();

	private static GameRamInsufficiencyDetector Instance => s_Instance ?? (s_Instance = new GameRamInsufficiencyDetector());

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init()
	{
		EventBus.Subscribe(Instance);
		Instance.RamInsufficiencyDetectingAsync(Application.exitCancellationToken);
	}

	private async Task RamInsufficiencyDetectingAsync(CancellationToken token)
	{
		while (!Application.exitCancellationToken.IsCancellationRequested)
		{
			await Task.Delay(m_Delay, token);
			try
			{
				long monoHeapSizeLong = Profiler.GetMonoHeapSizeLong();
				if (!RedSent && monoHeapSizeLong > m_MemoryRedLimit)
				{
					OwlcatAnalytics.Instance.SendRamInsufficiency(m_MemoryRedLimit, monoHeapSizeLong, Game.Instance?.CurrentMode ?? GameModeType.None, CurrentArea ?? "none", GetPartyCenter(), DateTimeOffset.Now - CurrentAreaEnterTime);
					RedSent = true;
				}
				if (!YellowSent && monoHeapSizeLong > m_MemoryYellowLimit)
				{
					OwlcatAnalytics.Instance.SendRamInsufficiency(m_MemoryYellowLimit, monoHeapSizeLong, Game.Instance?.CurrentMode, CurrentArea ?? "none", GetPartyCenter(), DateTimeOffset.Now - CurrentAreaEnterTime);
					YellowSent = true;
				}
			}
			catch
			{
			}
		}
	}

	private static Vector3 GetPartyCenter()
	{
		Vector3 result = Vector3.zero;
		List<Vector3> list = (from UnitEntity unit in Game.Instance?.Player.Party.Where((BaseUnitEntity unit) => unit is UnitEntity)
			select unit.Position).ToList();
		if (list.Count > 0)
		{
			result = list.Aggregate((Vector3 sum, Vector3 vec) => sum + vec) / list.Count;
		}
		return result;
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		CurrentAreaEnterTime = DateTimeOffset.Now;
		CurrentArea = Game.Instance.CurrentlyLoadedArea.name;
		YellowSent = false;
		RedSent = false;
	}

	[Cheat(Name = "debug_ram_insufficiency")]
	public static void DebugRamInsufficiency(long sizeInMbs)
	{
		byte[] item = new byte[sizeInMbs * 1024 * 1024];
		Instance.m_Mbs.Add(item);
	}
}
