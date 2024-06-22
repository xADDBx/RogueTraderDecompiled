using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Profiling.Memory;

namespace Kingmaker.QA.Arbiter;

public class AreaCheckerTask : ArbiterTask
{
	private readonly ArbiterStartupParameters m_Arguments;

	private AreaCheckerComponent m_AreaCheckerComponent;

	public AreaCheckerTask(AreaCheckerComponent areaCheckerComponent, ArbiterStartupParameters arguments)
	{
		m_Arguments = arguments;
		m_AreaCheckerComponent = areaCheckerComponent;
		base.Status = "Loading " + areaCheckerComponent.OwnerBlueprint.name;
	}

	protected override IEnumerator Routine()
	{
		Arbiter.Instance.Reporter.ReportInstructionStarted(Arbiter.Instruction.name);
		PFLog.Arbiter.Log($"Processing {Arbiter.Instruction} ({m_AreaCheckerComponent.Area.Get().AreaDisplayName})");
		ArbiterClientMeasurements.StartProfilerRecorders();
		yield return new SetScreenResolutionTask(m_Arguments, this);
		yield return new ResetToMainMenuTask(this);
		if (m_Arguments.ArbiterTakeMemorySnapshots && m_Arguments.ArbiterRestart)
		{
			TakeMemorySnapshot();
		}
		Dictionary<string, string> beforeLoadPreset = ArbiterClientMeasurements.GetMemoryMeasurementSnapshot().ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key.Replace("Memory.", "Memory.BeforeLoadPreset."), (KeyValuePair<string, string> kvp) => kvp.Value);
		yield return new LoadPresetTask(this, m_AreaCheckerComponent.Preset);
		Dictionary<string, string> afterLoadPreset = ArbiterClientMeasurements.GetMemoryMeasurementSnapshot().ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key.Replace("Memory.", "Memory.AfterLoadPreset."), (KeyValuePair<string, string> kvp) => kvp.Value);
		yield return new SetTimeOfDayTask(this, m_AreaCheckerComponent);
		GeneralProbeData probeData = new GeneralProbeData(Arbiter.Instruction.name, "AreaTest");
		yield return new AreaScreenshotsTask(this, m_AreaCheckerComponent, probeData);
		Dictionary<string, string> areaLoadMeasurements = ArbiterClientMeasurements.GetAreaLoadMeasurements();
		yield return new AreaMeasurementsTask(this, m_AreaCheckerComponent, probeData);
		yield return new ResetToMainMenuTask(this);
		if (m_Arguments.ArbiterTakeMemorySnapshots && m_Arguments.ArbiterRestart)
		{
			TakeMemorySnapshot();
		}
		Dictionary<string, string> measurements = ArbiterClientMeasurements.GetMemoryMeasurementSnapshot().ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key.Replace("Memory.", "Memory.AfterUnloadPreset."), (KeyValuePair<string, string> kvp) => kvp.Value);
		ArbiterClientMeasurements.StopProfilerRecorders();
		foreach (ISpecificProbeData probeData2 in probeData.ProbeDataList)
		{
			AreaProbeData obj = probeData2 as AreaProbeData;
			obj.AddCustomMeasurements(beforeLoadPreset);
			obj.AddCustomMeasurements(afterLoadPreset);
			obj.AddCustomMeasurements(measurements);
			obj.AddCustomMeasurements(areaLoadMeasurements);
		}
		try
		{
			probeData.SendToServer();
			probeData.CleanupData();
			if (m_AreaCheckerComponent.MakeMapScreenshot)
			{
				MakeAndSendMapScreenshot();
			}
			Arbiter.Instance.Reporter.ReportInstructionPassed(Arbiter.Instruction.name, base.ElapsedTestTime);
		}
		catch (Exception ex)
		{
			Arbiter.Instance.Reporter.ReportInstructionError(Arbiter.Instruction.name, ex.Message, base.ElapsedTestTime);
		}
	}

	private void TakeMemorySnapshot()
	{
		bool num = !File.Exists(Arbiter.SnapshotNameFirst);
		if (File.Exists(Arbiter.SnapshotNameTemp))
		{
			File.Delete(Arbiter.SnapshotNameTemp);
		}
		MemoryProfiler.TakeSnapshot(Arbiter.SnapshotNameTemp, delegate
		{
		}, CaptureFlags.ManagedObjects | CaptureFlags.NativeObjects | CaptureFlags.NativeAllocations);
		string text = (num ? Arbiter.SnapshotNameFirst : Arbiter.SnapshotNameLast);
		if (!num && File.Exists(text))
		{
			File.Delete(text);
		}
		File.Move(Arbiter.SnapshotNameTemp, text);
	}

	private void MakeAndSendMapScreenshot()
	{
		ArbiterClientIntegration.DisableFow();
		GeneralProbeData generalProbeData = new GeneralProbeData(Arbiter.Instruction.name, "AreaMapTest");
		ArbiterUtils.SaveAreaMapImageToFile(Path.Combine(generalProbeData.DataFolder, "0.png"));
		generalProbeData.ProbeDataList.Add(new AreaMapProbeData(m_AreaCheckerComponent.Area.Get().name));
		generalProbeData.SendToServer();
		generalProbeData.CleanupData();
		ArbiterClientIntegration.EnableFow();
	}
}
