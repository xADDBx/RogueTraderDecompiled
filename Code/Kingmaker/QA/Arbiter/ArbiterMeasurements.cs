using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using Kingmaker.Cheats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA.Arbiter.Attributes;
using Kingmaker.Settings;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps;
using Newtonsoft.Json.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker.QA.Arbiter;

public class ArbiterMeasurements
{
	public interface IArbiterMeasurementTimer : IDisposable
	{
		double Elapsed { get; }

		void Start();

		void Stop();
	}

	private class ArbiterMeasurementTimer : IArbiterMeasurementTimer, IDisposable
	{
		private Stopwatch timer = new Stopwatch();

		public double Elapsed => timer.IsRunning ? 0f : ((float)timer.ElapsedMilliseconds / 2000f);

		public void Dispose()
		{
			StopTimer(this);
		}

		public void Start()
		{
			timer.Restart();
		}

		public void Stop()
		{
			timer.Stop();
		}
	}

	public static string s_MemoryToolUrl = "http://localhost:8000/memory";

	public static bool s_IsMemoryToolAvailable = true;

	private static int s_MemoryFactor = 1048576;

	private static List<MeasurementCounter> m_Counters;

	private static bool m_Started = false;

	private static ProfilerRecorder s_SystemUsedMemoryRecorder;

	private static ProfilerRecorder s_TotalUsedMemoryRecorder;

	private static ProfilerRecorder s_TotalReservedMemoryRecorder;

	private static ProfilerRecorder s_GCUsedMemoryRecorder;

	private static ProfilerRecorder s_GCReservedMemoryRecorder;

	private static ProfilerRecorder s_ProfilerUsedMemoryRecorder;

	private static ProfilerRecorder s_ProfilerReservedMemoryRecorder;

	private static ProfilerRecorder s_AudioUsedMemoryRecorder;

	private static ProfilerRecorder s_AudioReservedMemoryRecorder;

	private static ProfilerRecorder s_VideoUsedMemoryRecorder;

	private static ProfilerRecorder s_VideoReservedMemoryRecorder;

	private static ProfilerRecorder s_RenderTexturesCountRecorder;

	private static ProfilerRecorder s_RenderTexturesBytesRecorder;

	private static Dictionary<string, IArbiterMeasurementTimer> ArbiterMeasurementTimers = new Dictionary<string, IArbiterMeasurementTimer>();

	private static MeasurementCounter SystemUsedMemoryCounter { get; } = new MeasurementCounter(() => SystemUsedMemory, "SystemUsedMemory");


	private static MeasurementCounter TotalUsedMemoryCounter { get; } = new MeasurementCounter(() => TotalUsedMemory, "TotalUsedMemory");


	private static MeasurementCounter TotalReservedMemoryCounter { get; } = new MeasurementCounter(() => TotalReservedMemory, "TotalReservedMemory");


	private static MeasurementCounter GCUsedMemoryCounter { get; } = new MeasurementCounter(() => GCUsedMemory, "GCUsedMemory");


	private static MeasurementCounter GCReservedMemoryCounter { get; } = new MeasurementCounter(() => GCReservedMemory, "GCReservedMemory");


	private static MeasurementCounter ProfilerUsedMemoryCounter { get; } = new MeasurementCounter(() => ProfilerUsedMemory, "ProfilerUsedMemory");


	private static MeasurementCounter ProfilerReservedMemoryCounter { get; } = new MeasurementCounter(() => ProfilerReservedMemory, "ProfilerReservedMemory");


	private static MeasurementCounter AudioUsedMemoryCounter { get; } = new MeasurementCounter(() => AudioUsedMemory, "AudioUsedMemory");


	private static MeasurementCounter AudioReservedMemoryCounter { get; } = new MeasurementCounter(() => AudioReservedMemory, "AudioReservedMemory");


	private static MeasurementCounter VideoUsedMemoryCounter { get; } = new MeasurementCounter(() => VideoUsedMemory, "VideoUsedMemory");


	private static MeasurementCounter VideoReservedMemoryCounter { get; } = new MeasurementCounter(() => VideoReservedMemory, "VideoReservedMemory");


	private static MeasurementCounter RenderTexturesCountCounter { get; } = new MeasurementCounter(() => RenderTexturesCount, "RenderTexturesCount");


	private static MeasurementCounter RenderTexturesBytesCounter { get; } = new MeasurementCounter(() => RenderTexturesBytes, "RenderTexturesBytes");


	[Measurement]
	private static int FPS => Mathf.RoundToInt(1f / DeltaTimeCounter.GetAvgValue(95));

	[Measurement]
	private static int AwakeUnits => Game.Instance.State.AllAwakeUnits.Count;

	[Measurement]
	private static int TotalUnits => Game.Instance.State.AllUnits.All.Count;

	[Measurement]
	private static int UniqueUnitPrefabs => Game.Instance.State.AllUnits.All.Select((AbstractUnitEntity u) => u.ViewSettings.PrefabGuid).Distinct().Count();

	[Measurement]
	private static int Corpses => Game.Instance.State.AllUnits.All.Count((AbstractUnitEntity u) => u.LifeState.IsDead);

	[Measurement]
	private static int MapObjects => Game.Instance.State.MapObjects.Count();

	[Measurement]
	private static int InteractiveObjects => Game.Instance.State.MapObjects.Count((MapObjectEntity m) => m.View.GetComponent<IInteractionComponent>() != null);

	[Measurement]
	private static int Traps => Game.Instance.State.MapObjects.OfType<TrapObjectData>().Count();

	[Measurement]
	private static int Cutscenes => Game.Instance.State.Cutscenes.Count();

	[Measurement]
	private static float DeltaTime => DeltaTimeCounter.GetAvgValue(95);

	[Measurement]
	private static int AllocatedMemory => (int)(Profiler.GetTotalAllocatedMemoryLong() / s_MemoryFactor);

	[Measurement]
	private static int ReservedMemory => (int)(Profiler.GetTotalReservedMemoryLong() / s_MemoryFactor);

	[Measurement]
	private static int TotalMemory
	{
		get
		{
			if (!s_SystemUsedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_SystemUsedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int SystemUsedMemory
	{
		get
		{
			if (!s_SystemUsedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_SystemUsedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int TotalUsedMemory
	{
		get
		{
			if (!s_TotalUsedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_TotalUsedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int TotalReservedMemory
	{
		get
		{
			if (!s_TotalReservedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_TotalReservedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int GCUsedMemory
	{
		get
		{
			if (!s_GCUsedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_GCUsedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int GCReservedMemory
	{
		get
		{
			if (!s_GCReservedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_GCReservedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int ProfilerUsedMemory
	{
		get
		{
			if (!s_ProfilerUsedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_ProfilerUsedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int ProfilerReservedMemory
	{
		get
		{
			if (!s_ProfilerReservedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_ProfilerReservedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int AudioUsedMemory
	{
		get
		{
			if (!s_AudioUsedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_AudioUsedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int AudioReservedMemory
	{
		get
		{
			if (!s_AudioReservedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_AudioReservedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int VideoUsedMemory
	{
		get
		{
			if (!s_VideoUsedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_VideoUsedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int VideoReservedMemory
	{
		get
		{
			if (!s_VideoReservedMemoryRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_VideoReservedMemoryRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int RenderTexturesCount
	{
		get
		{
			if (!s_RenderTexturesCountRecorder.Valid)
			{
				return 0;
			}
			return (int)s_RenderTexturesCountRecorder.LastValue;
		}
	}

	[Measurement]
	private static int RenderTexturesBytes
	{
		get
		{
			if (!s_RenderTexturesBytesRecorder.Valid)
			{
				return 0;
			}
			return (int)(s_RenderTexturesBytesRecorder.LastValue / s_MemoryFactor);
		}
	}

	[Measurement]
	private static int MonoUsedSize => (int)(Profiler.GetMonoUsedSizeLong() / s_MemoryFactor);

	[Measurement]
	private static int MonoHeapSize => (int)(Profiler.GetMonoHeapSizeLong() / s_MemoryFactor);

	[Measurement]
	private static int GpuMemory => (int)(Profiler.GetAllocatedMemoryForGraphicsDriver() / s_MemoryFactor);

	private static MeasurementCounter DeltaTimeCounter { get; } = new MeasurementCounter(() => Time.unscaledDeltaTime, "DeltaTime");


	private static MeasurementCounter AllocatedMemoryCounter { get; } = new MeasurementCounter(() => AllocatedMemory, "AllocatedMemory");


	private static MeasurementCounter ReservedMemoryCounter { get; } = new MeasurementCounter(() => ReservedMemory, "ReservedMemory");


	private static MeasurementCounter TotalMemoryCounter { get; } = new MeasurementCounter(() => TotalMemory, "TotalMemory");


	private static MeasurementCounter MonoUsedSizeCounter { get; } = new MeasurementCounter(() => MonoUsedSize, "MonoUsedSize");


	private static MeasurementCounter MonoHeapSizeCounter { get; } = new MeasurementCounter(() => MonoHeapSize, "MonoHeapSize");


	private static MeasurementCounter GpuMemoryCounter { get; } = new MeasurementCounter(() => GpuMemory, "GpuMemory");


	private static MeasurementCounter AwakeUnitsCounter { get; } = new MeasurementCounter(() => AwakeUnits, "AwakeUnits");


	private static MeasurementCounter TotalUnitsCounter { get; } = new MeasurementCounter(() => TotalUnits, "TotalUnits");


	private static MeasurementCounter MapObjectsCounter { get; } = new MeasurementCounter(() => MapObjects, "MapObjects");


	public static void StartProfilerRecorders()
	{
		if (s_SystemUsedMemoryRecorder.Valid)
		{
			if (!s_SystemUsedMemoryRecorder.IsRunning)
			{
				s_SystemUsedMemoryRecorder.Start();
			}
			return;
		}
		s_SystemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_TotalUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_TotalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_GCUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_GCReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_ProfilerUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Profiler Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_ProfilerReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Profiler Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_AudioUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_AudioReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_VideoUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Video Used Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_VideoReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Video Reserved Memory", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_RenderTexturesCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Render Textures Count", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
		s_RenderTexturesBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Render Textures Bytes", 1, ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
	}

	public static void StopProfilerRecorders()
	{
		if (s_SystemUsedMemoryRecorder.Valid && s_SystemUsedMemoryRecorder.IsRunning)
		{
			s_SystemUsedMemoryRecorder.Stop();
		}
	}

	public static Dictionary<string, string> GetAreaLoadMeasurements()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		try
		{
			CheatsDebug.UITexturesCountForArbiter uITexturesCountForArbiter = CheatsDebug.GetUITexturesCountForArbiter();
			long num = uITexturesCountForArbiter.UI_Atlases.Size / 1048576;
			long num2 = uITexturesCountForArbiter.UI_NoAtlases.Size / 1048576;
			dictionary.Add("UITexturesAtlas", num.ToString());
			dictionary.Add("UITexturesNonAtlas", num2.ToString());
			PFLog.Arbiter.Log("Graphics settings: " + $"\n\tQuality preset: {SettingsRoot.Graphics.GraphicsQuality.GetValue()}" + $"\n\tGamma Correction: {SettingsRoot.Display.GammaCorrection.GetValue()}" + $"\n\tBrightness: {SettingsRoot.Display.Brightness.GetValue()}" + $"\n\tContrast: {SettingsRoot.Display.Contrast.GetValue()}" + $"\n\tDeuteranopia: {SettingsRoot.Accessiability.Deuteranopia.GetValue()}" + $"\n\tProtanopia: {SettingsRoot.Accessiability.Protanopia.GetValue()}" + $"\n\tTritanopia: {SettingsRoot.Accessiability.Tritanopia.GetValue()}");
		}
		catch (Exception ex)
		{
			PFLog.Arbiter.Exception(ex);
			dictionary.Add("UITexturesAtlas", "unknown");
			dictionary.Add("UITexturesNonAtlas", "unknown");
		}
		return dictionary;
	}

	public static Dictionary<string, string> GetMemoryMeasurementSnapshot()
	{
		return new Dictionary<string, string>
		{
			{
				"Memory.TotalUsed",
				TotalUsedMemory.ToString()
			},
			{
				"Memory.TotalReserved",
				TotalReservedMemory.ToString()
			},
			{
				"Memory.SystemUsed",
				SystemUsedMemory.ToString()
			},
			{
				"Memory.GCUsed",
				GCUsedMemory.ToString()
			},
			{
				"Memory.GCReserved",
				GCReservedMemory.ToString()
			},
			{
				"Memory.VideoUsed",
				VideoUsedMemory.ToString()
			},
			{
				"Memory.VideoReserved",
				VideoReservedMemory.ToString()
			}
		};
	}

	public static Dictionary<string, string> GetAreaPointMeasurements()
	{
		Dictionary<string, string> results = new Dictionary<string, string>();
		foreach (PropertyInfo item in from prop in typeof(ArbiterMeasurements).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where Attribute.IsDefined(prop, typeof(MeasurementAttribute))
			select prop)
		{
			try
			{
				string value = Convert.ToString(item.GetValue(null), CultureInfo.InvariantCulture);
				results.Add(item.Name, value);
			}
			catch (Exception)
			{
				results.Add(item.Name, "unknown");
			}
		}
		AddDataFromMemoryTool(ref results);
		AddArbiterMeasurementTimers(ref results);
		return results;
	}

	private static void AddArbiterMeasurementTimers(ref Dictionary<string, string> results)
	{
		foreach (var (key, arbiterMeasurementTimer2) in ArbiterMeasurementTimers)
		{
			results.Add(key, arbiterMeasurementTimer2.Elapsed.ToString("0.00", CultureInfo.InvariantCulture));
		}
	}

	public static Dictionary<string, string> GetEveryFrameMeasurements()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (PropertyInfo item in from p in typeof(ArbiterMeasurements).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList()
			where p.PropertyType == typeof(MeasurementCounter)
			select p)
		{
			MeasurementCounter measurementCounter = (MeasurementCounter)item.GetValue(null);
			dictionary.Add((measurementCounter.Name ?? item.Name) + "_Min", measurementCounter.GetMinValue().ToString("0.00", CultureInfo.InvariantCulture));
			dictionary.Add((measurementCounter.Name ?? item.Name) + "_Max", measurementCounter.GetMaxValue().ToString("0.00", CultureInfo.InvariantCulture));
			dictionary.Add((measurementCounter.Name ?? item.Name) + "_Avg", measurementCounter.GetAvgValue().ToString("0.00", CultureInfo.InvariantCulture));
		}
		dictionary.Add("FPS_Min", (1f / DeltaTimeCounter.GetMaxValue()).ToString("0.00", CultureInfo.InvariantCulture));
		dictionary.Add("FPS_Avg", (1f / DeltaTimeCounter.GetAvgValue()).ToString("0.00", CultureInfo.InvariantCulture));
		dictionary.Add("FPS_Max", (1f / DeltaTimeCounter.GetMinValue()).ToString("0.00", CultureInfo.InvariantCulture));
		dictionary.Add("FPS_95Avg", (1f / DeltaTimeCounter.GetAvgValue(95)).ToString("0.00", CultureInfo.InvariantCulture));
		dictionary.Add("FPS_1ExclMin", (1f / DeltaTimeCounter.GetMinExcludedValue(99)).ToString("0.00", CultureInfo.InvariantCulture));
		return dictionary;
	}

	private static List<MeasurementCounter> GetEveryFrameMeasurementCounters()
	{
		return (from p in typeof(ArbiterMeasurements).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where p.PropertyType == typeof(MeasurementCounter)
			select (MeasurementCounter)p.GetValue(null)).ToList();
	}

	public static void StartEveryFrameMeasurements()
	{
		if (m_Counters == null)
		{
			m_Counters = GetEveryFrameMeasurementCounters();
		}
		m_Counters.ForEach(delegate(MeasurementCounter c)
		{
			c.Reset();
		});
		m_Started = true;
	}

	public static void StopEveryFrameMeasurements()
	{
		m_Started = false;
	}

	public static void TickEveryFrameMeasurements()
	{
		if (m_Started)
		{
			m_Counters?.ForEach(delegate(MeasurementCounter x)
			{
				x.Measure();
			});
		}
	}

	private static void AddDataFromMemoryTool(ref Dictionary<string, string> results)
	{
		if (!s_IsMemoryToolAvailable)
		{
			return;
		}
		try
		{
			JObject jObject = JObject.Parse(new WebClient().DownloadString(s_MemoryToolUrl));
			results["ProcessVirtualMemory"] = (string?)jObject.SelectToken("vms");
			results["ProcessPhysicalMemory"] = (string?)jObject.SelectToken("rss");
			results["ProcessGpuMemory"] = (string?)jObject.SelectToken("gpu");
		}
		catch (WebException arg)
		{
			PFLog.Arbiter.Error($"Failed to connect to memory info tool: {arg}");
			s_IsMemoryToolAvailable = false;
		}
		catch (Exception arg2)
		{
			PFLog.Arbiter.Error($"Failed to parse data from memory info tool: {arg2}");
		}
	}

	public static IArbiterMeasurementTimer StartTimer(string name)
	{
		if (!ArbiterMeasurementTimers.TryGetValue(name, out var value))
		{
			value = new ArbiterMeasurementTimer();
			ArbiterMeasurementTimers.Add(name, value);
		}
		value.Start();
		return value;
	}

	private static void StopTimer(IArbiterMeasurementTimer timer)
	{
		timer.Stop();
	}
}
