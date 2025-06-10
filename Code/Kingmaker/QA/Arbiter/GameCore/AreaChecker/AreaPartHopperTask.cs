using System.Collections.Generic;
using System.Linq;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Service.Tests.ScreenshotTest;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter.GameCore.AreaChecker;

public class AreaPartHopperTask : ArbiterTask
{
	private AreaCheckerComponent m_AreaCheckerComponent;

	private GeneralProbeData m_ProbeData;

	public AreaPartHopperTask(ArbiterTask parent, AreaCheckerComponent areaCheckerComponent, GeneralProbeData probeData)
		: base(parent)
	{
		m_AreaCheckerComponent = areaCheckerComponent;
		m_ProbeData = probeData;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		ArbiterIntegration.AddCameraRig();
		m_AreaCheckerComponent.AreaParts.ElementList.Where((ArbiterElement ap) => ap is AreaCheckerComponentPart).Cast<AreaCheckerComponentPart>().SelectMany((AreaCheckerComponentPart ap) => ap.Points)
			.Count();
		foreach (ArbiterElement areaPartElement in m_AreaCheckerComponent.AreaParts.ElementList)
		{
			if (!(areaPartElement is AreaCheckerComponentPart areaPartTest))
			{
				ArbiterService.Logger.Error("AreaPartElement with name '" + areaPartElement.name + "' is not AreaCheckerComponentPart");
			}
			else if (ArbiterIntegration.EnableEnterPoint(((AreaCheckerComponentPart)areaPartElement).EnterPoint))
			{
				yield return new TeleportToPointTask(this, areaPartTest.EnterPoint);
				yield return new GameLoadingWaitTask(this);
				base.Status = $"Screenshot {areaPartTest.Points.Count()} points in area '{areaPartElement.name}' with enter point '{areaPartTest.EnterPoint}'";
				yield return new ArbiterScreenerTask(this, m_ProbeData, areaPartTest.Points.Select((ArbiterPointElement ap) => ap.GetArbiterPoint()));
			}
		}
		ArbiterIntegration.RemoveCameraRig();
	}
}
