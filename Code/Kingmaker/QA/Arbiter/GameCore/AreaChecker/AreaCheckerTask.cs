using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Tasks;
using Unity.Profiling.Memory;

namespace Kingmaker.QA.Arbiter.GameCore.AreaChecker;

public class AreaCheckerTask : ArbiterTask
{
	private readonly ArbiterStartupParameters m_Arguments;

	private readonly AreaCheckerComponent m_AreaCheckerComponent;

	public AreaCheckerTask(AreaCheckerComponent areaCheckerComponent, ArbiterStartupParameters arguments)
	{
		m_Arguments = arguments;
		m_AreaCheckerComponent = areaCheckerComponent;
		base.Status = "Loading " + areaCheckerComponent.OwnerBlueprint.name;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		ArbiterService.Instance.MeasureProvider.StartProfilerRecorders();
		yield return new SetScreenResolutionTask(m_Arguments, this);
		yield return new ResetToMainMenuTask(this);
		if (m_Arguments.ArbiterTakeMemorySnapshots && m_Arguments.ArbiterRestart)
		{
			TakeMemorySnapshot();
		}
		Dictionary<string, string> beforeLoadPreset = GetMemoryMeasurementSnapshot().ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key.Replace("Memory.", "Memory.BeforeLoadPreset."), (KeyValuePair<string, string> kvp) => kvp.Value);
		yield return new LoadPresetTask(this, m_AreaCheckerComponent.Preset);
		Dictionary<string, string> afterLoadPreset = GetMemoryMeasurementSnapshot().ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key.Replace("Memory.", "Memory.AfterLoadPreset."), (KeyValuePair<string, string> kvp) => kvp.Value);
		yield return new SetTimeOfDayTask(this, m_AreaCheckerComponent);
		GeneralProbeData probeData = ArbiterService.Instance.CreateGeneralProbeData("AreaTest");
		yield return new AreaPartHopperTask(this, m_AreaCheckerComponent, probeData);
		yield return new AreaMeasurementsTask(this, m_AreaCheckerComponent, probeData);
		yield return new ResetToMainMenuTask(this);
		if (m_Arguments.ArbiterTakeMemorySnapshots && m_Arguments.ArbiterRestart)
		{
			TakeMemorySnapshot();
		}
		Dictionary<string, string> measurements = GetMemoryMeasurementSnapshot().ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key.Replace("Memory.", "Memory.AfterUnloadPreset."), (KeyValuePair<string, string> kvp) => kvp.Value);
		ArbiterService.Instance.MeasureProvider.StopProfilerRecorders();
		foreach (ISampleData sampleData in probeData.SampleDataList)
		{
			ScreenshotSampleData obj = sampleData as ScreenshotSampleData;
			obj.AddCustomMeasurements(beforeLoadPreset);
			obj.AddCustomMeasurements(afterLoadPreset);
			obj.AddCustomMeasurements(measurements);
		}
		ArbiterService.Instance.SendToServer(probeData);
	}

	private Dictionary<string, string> GetMemoryMeasurementSnapshot()
	{
		string[] desiredMeasure = new string[7] { "Memory.TotalUsed", "Memory.TotalReserved", "Memory.SystemUsed", "Memory.GCUsed", "Memory.GCReserved", "Memory.VideoUsed", "Memory.VideoReserved" };
		return (from kvp in ArbiterService.Instance.MeasureProvider.GetMeasures()
			where desiredMeasure.Contains(kvp.Key)
			select kvp).ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key, (KeyValuePair<string, string> kvp) => kvp.Value);
	}

	private void TakeMemorySnapshot()
	{
		bool num = !File.Exists(ArbiterService.SnapshotNameFirst);
		if (File.Exists(ArbiterService.SnapshotNameTemp))
		{
			File.Delete(ArbiterService.SnapshotNameTemp);
		}
		MemoryProfiler.TakeSnapshot(ArbiterService.SnapshotNameTemp, delegate
		{
		}, CaptureFlags.ManagedObjects | CaptureFlags.NativeObjects | CaptureFlags.NativeAllocations);
		string text = (num ? ArbiterService.SnapshotNameFirst : ArbiterService.SnapshotNameLast);
		if (!num && File.Exists(text))
		{
			File.Delete(text);
		}
		File.Move(ArbiterService.SnapshotNameTemp, text);
	}
}
