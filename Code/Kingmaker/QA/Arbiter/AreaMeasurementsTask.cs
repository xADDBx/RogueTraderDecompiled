using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Controllers;
using Kingmaker.QA.Arbiter.Profiling;
using Owlcat.Runtime.Core.ProfilingCounters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.QA.Arbiter;

public class AreaMeasurementsTask : ArbiterTask
{
	private const int FramesToWarmUp = 15;

	private const int FramesForFPS = 120;

	private readonly AreaCheckerComponent m_AreaCheckerComponent;

	private readonly GeneralProbeData m_ProbeData;

	private readonly Dictionary<string, string> m_OnAreaLoadMeasurements;

	private GameObject m_Revealer;

	public AreaMeasurementsTask(ArbiterTask parent, AreaCheckerComponent areaCheckerComponent, GeneralProbeData probeData)
		: base(parent)
	{
		m_AreaCheckerComponent = areaCheckerComponent;
		m_ProbeData = probeData;
	}

	protected override IEnumerator Routine()
	{
		EnsureRevealer();
		ArbiterClientIntegration.AddCameraRig();
		ArbiterClientIntegration.EnableEnterPoints(m_AreaCheckerComponent.AreaParts.ElementList.Select((ArbiterElement x) => (x as AreaCheckerComponentPart)?.EnterPoint));
		int oldVSync = ArbiterClientIntegration.SetVSync(0);
		foreach (ArbiterElement element in m_AreaCheckerComponent.AreaParts.ElementList)
		{
			if (!(element is AreaCheckerComponentPart areaPartTest))
			{
				PFLog.Arbiter.Error("AreaPartElement with name '" + element.name + "' is not AreaCheckerComponentPart");
				continue;
			}
			yield return new TeleportToPointTask(this, areaPartTest.EnterPoint);
			bool oldGamePauseValue = ArbiterClientIntegration.SetGamePause(value: false);
			yield return 5f;
			ArbiterClientIntegration.DisableFow();
			foreach (ArbiterPoint point in areaPartTest.Points)
			{
				ArbiterClientIntegration.MoveCameraToPoint(point);
				m_Revealer.transform.position = point.Position;
				FogOfWarControllerData.AddRevealer(m_Revealer.transform);
				base.Status = $"Profiling at point {m_AreaCheckerComponent.GetSampleId(point)}";
				PFLog.Arbiter.Log(base.Status);
				int i;
				for (i = 0; i < 15; i++)
				{
					yield return null;
				}
				AreaProbeData sampleData = new AreaProbeData
				{
					Area = m_AreaCheckerComponent.Area.Get().name,
					AreaPart = areaPartTest.AreaPartName,
					ActiveScene = SceneManager.GetActiveScene().name,
					AreaPartLightScenes = areaPartTest.AreaPartLightScenes,
					Sample = m_AreaCheckerComponent.GetSampleId(point).ToString()
				};
				PFLog.Arbiter.Log("Sample data created");
				ArbiterMeasurements.StartEveryFrameMeasurements();
				i = 0;
				while (i < 120)
				{
					yield return null;
					ArbiterMeasurements.TickEveryFrameMeasurements();
					int num = i + 1;
					i = num;
				}
				ArbiterMeasurements.StopEveryFrameMeasurements();
				sampleData.StaticScene = areaPartTest.StaticSceneName;
				sampleData.CustomMeasurements = ArbiterMeasurements.GetAreaPointMeasurements();
				Kingmaker.QA.Arbiter.Profiling.Counters.All.ForEach(delegate(Counter x)
				{
					sampleData.CustomMeasurements.Add(x.Name, x.GetMedian().ToString("0.00", CultureInfo.InvariantCulture));
				});
				PFLog.Arbiter.Log("Custom measurements added");
				m_ProbeData.ProbeDataList.Add(sampleData);
				FogOfWarControllerData.RemoveRevealer(m_Revealer.transform);
			}
			ArbiterClientIntegration.EnableFow();
			yield return 5f;
			ArbiterClientIntegration.SetGamePause(oldGamePauseValue);
		}
		ArbiterClientIntegration.SetVSync(oldVSync);
		ArbiterClientIntegration.RemoveCameraRig();
	}

	private void EnsureRevealer()
	{
		if (m_Revealer == null)
		{
			m_Revealer = new GameObject("ArbiterRevealer");
			Object.DontDestroyOnLoad(m_Revealer);
		}
	}
}
