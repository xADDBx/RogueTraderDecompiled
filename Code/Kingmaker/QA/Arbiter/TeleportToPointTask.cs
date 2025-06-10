using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.QA.Arbiter.GameCore;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter;

public class TeleportToPointTask : ArbiterTask
{
	private readonly BlueprintAreaEnterPointReference m_BlueprintAreaEnterPointReference;

	public TeleportToPointTask(ArbiterTask parent, BlueprintAreaEnterPointReference blueprintAreaEnterPointReference)
		: base(parent)
	{
		m_BlueprintAreaEnterPointReference = blueprintAreaEnterPointReference;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		using (ArbiterMeasurementTimer.StartTimer("TeleportToPoint"))
		{
			ArbiterIntegration.TeleportToEnterPoint(m_BlueprintAreaEnterPointReference);
			yield return null;
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			yield return new WaitForDefaultModeTask(this);
		}
	}
}
