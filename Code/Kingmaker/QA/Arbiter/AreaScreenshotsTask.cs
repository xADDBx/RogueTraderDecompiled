using System;
using System.Collections;
using System.Linq;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;

namespace Kingmaker.QA.Arbiter;

public class AreaScreenshotsTask : ArbiterTask
{
	private AreaCheckerComponent m_AreaCheckerComponent;

	private GeneralProbeData m_ProbeData;

	public AreaScreenshotsTask(ArbiterTask parent, AreaCheckerComponent areaCheckerComponent, GeneralProbeData probeData)
		: base(parent)
	{
		m_AreaCheckerComponent = areaCheckerComponent;
		m_ProbeData = probeData;
	}

	protected override IEnumerator Routine()
	{
		ArbiterClientIntegration.AddCameraRig();
		ArbiterClientIntegration.EnableEnterPoints(m_AreaCheckerComponent.AreaParts.ElementList.Select((ArbiterElement x) => (x as AreaCheckerComponentPart)?.EnterPoint));
		foreach (ArbiterElement element in m_AreaCheckerComponent.AreaParts.ElementList)
		{
			if (!(element is AreaCheckerComponentPart areaPartTest))
			{
				PFLog.Arbiter.Error("AreaPartElement with name '" + element.name + "' is not AreaCheckerComponentPart");
				continue;
			}
			yield return null;
			yield return new GameLoadingWaitTask(this);
			yield return new WaitForCutsceneTask(this);
			int oldVSync = ArbiterClientIntegration.SetVSync(0);
			ArbiterClientIntegration.HideUi();
			InclemencyType oldWeather = ArbiterClientIntegration.SetWeather();
			ArbiterClientIntegration.DisableClouds();
			ArbiterClientIntegration.DisableWind();
			ArbiterClientIntegration.DisableFog();
			ArbiterClientIntegration.DisableFow();
			ArbiterClientIntegration.DisableFx();
			yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
			yield return new TeleportToPointTask(this, areaPartTest.EnterPoint);
			bool oldGamePauseValue = ArbiterClientIntegration.SetGamePause(value: true);
			foreach (ArbiterPoint point in areaPartTest.Points)
			{
				ArbiterClientIntegration.MoveCameraToPoint(point);
				base.Status = $"Screenshot at point {m_AreaCheckerComponent.GetSampleId(point)}";
				PFLog.Arbiter.Log(base.Status);
				ArbiterClientIntegration.HideUnits();
				yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
				ArbiterClientIntegration.TakeScreenshot(m_ProbeData.DataFolder, m_AreaCheckerComponent.GetSampleId(point));
				ArbiterClientIntegration.ShowUnits();
			}
			ArbiterClientIntegration.SetGamePause(oldGamePauseValue);
			yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
			ArbiterClientIntegration.EnableFx();
			ArbiterClientIntegration.EnableFow();
			ArbiterClientIntegration.EnableFog();
			ArbiterClientIntegration.EnableWind();
			ArbiterClientIntegration.EnableClouds();
			ArbiterClientIntegration.SetWeather(oldWeather);
			ArbiterClientIntegration.ShowUi();
			ArbiterClientIntegration.SetVSync(oldVSync);
		}
		ArbiterClientIntegration.RemoveCameraRig();
	}
}
