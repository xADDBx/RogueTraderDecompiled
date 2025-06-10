using System.Collections.Generic;
using Kingmaker.QA.Arbiter.GameCore.DynamicsChecker;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter.DynamicsChecker;

public class DynamicsReportingTask : ArbiterTask
{
	private readonly ArbiterStartupParameters m_Arguments;

	private DynamicsCheckerComponent m_DynamicsCheckerComponent;

	public DynamicsReportingTask(DynamicsCheckerComponent dynamicsCheckerComponent, ArbiterStartupParameters arguments)
	{
		m_Arguments = arguments;
		m_DynamicsCheckerComponent = dynamicsCheckerComponent;
		base.Status = "Loading " + dynamicsCheckerComponent.OwnerBlueprint.name;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		if (!m_DynamicsCheckerComponent.DebugStart)
		{
			yield return new StartNewGameFromPresetTask(this, m_DynamicsCheckerComponent.StartPreset);
			yield return new SetScreenResolutionTask(m_Arguments, this);
		}
		GeneralProbeData probeData = ArbiterService.Instance.CreateGeneralProbeData("DynamicTest");
		yield return new CommandsRunnerTask(this, m_DynamicsCheckerComponent.CommandList, probeData);
		ArbiterService.Instance.SendToServer(probeData);
	}
}
