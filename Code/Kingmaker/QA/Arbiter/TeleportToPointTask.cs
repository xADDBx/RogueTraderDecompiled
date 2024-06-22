using System.Collections;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.QA.Arbiter;

public class TeleportToPointTask : ArbiterTask
{
	private readonly BlueprintAreaEnterPointReference m_BlueprintAreaEnterPointReference;

	public TeleportToPointTask(ArbiterTask parent, BlueprintAreaEnterPointReference blueprintAreaEnterPointReference)
		: base(parent)
	{
		m_BlueprintAreaEnterPointReference = blueprintAreaEnterPointReference;
	}

	protected override IEnumerator Routine()
	{
		using (ArbiterClientMeasurements.StartTimer("TeleportToPoint"))
		{
			ArbiterClientIntegration.TeleportToEnterPoint(m_BlueprintAreaEnterPointReference);
			yield return null;
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			yield return new WaitForCutsceneTask(this);
		}
	}
}
