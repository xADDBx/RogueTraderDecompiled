using System;
using System.Collections.Generic;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.QA.Arbiter.GameCore.AreaChecker;

public class AreaMeasurementsTask : ArbiterTask
{
	private const int FramesToWarmUp = 15;

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

	protected override IEnumerator<ArbiterTask> Routine()
	{
		EnsureRevealer();
		ArbiterIntegration.AddCameraRig();
		VSyncConfig oldVSync = ArbiterUtils.SetVSync(VSyncConfig.Off);
		foreach (ArbiterElement element in m_AreaCheckerComponent.AreaParts.ElementList)
		{
			if (!(element is AreaCheckerComponentPart areaPartTest))
			{
				ArbiterService.Logger.Error("AreaPartElement with name '" + element.name + "' is not AreaCheckerComponentPart");
				continue;
			}
			yield return new TeleportToPointTask(this, areaPartTest.EnterPoint);
			bool oldGamePauseValue = ArbiterIntegration.SetGamePause(value: false);
			yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
			ArbiterIntegration.DisableFow();
			foreach (ArbiterPointElement point in areaPartTest.Points)
			{
				ArbiterService.Instance.Integration.MoveCameraToPoint(point.Position, point.Rotation, point.Zoom);
				m_Revealer.transform.position = point.Position;
				ArbiterIntegration.AddRevealer(m_Revealer.transform);
				base.Status = $"Profiling at point {m_AreaCheckerComponent.GetSampleId(point)}";
				ArbiterService.Logger.Log(base.Status);
				for (int i = 0; i < 15; i++)
				{
					yield return null;
				}
				ScreenshotSampleData sampleData = new ScreenshotSampleData
				{
					Area = m_AreaCheckerComponent.Area.Get().name,
					AreaPart = areaPartTest.AreaPartName,
					ActiveScene = SceneManager.GetActiveScene().name,
					AreaPartLightScenes = areaPartTest.AreaPartLightScenes,
					Sample = m_AreaCheckerComponent.GetSampleId(point).ToString()
				};
				ArbiterService.Logger.Log("Sample data created");
				Dictionary<string, string> data;
				yield return new CollectMeasuresTask(this, new string[1] { "FPS" }, out data);
				sampleData.StaticScene = areaPartTest.StaticSceneName;
				sampleData.Data = data;
				ArbiterService.Logger.Log("Custom measurements added");
				m_ProbeData.SampleDataList.Add(sampleData);
				ArbiterIntegration.RemoveRevealer(m_Revealer.transform);
				data = null;
			}
			ArbiterIntegration.EnableFow();
			yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
			ArbiterIntegration.SetGamePause(oldGamePauseValue);
		}
		ArbiterUtils.SetVSync(oldVSync);
		ArbiterIntegration.RemoveCameraRig();
	}

	private void EnsureRevealer()
	{
		if (m_Revealer == null)
		{
			m_Revealer = new GameObject("ArbiterRevealer");
			UnityEngine.Object.DontDestroyOnLoad(m_Revealer);
		}
	}
}
