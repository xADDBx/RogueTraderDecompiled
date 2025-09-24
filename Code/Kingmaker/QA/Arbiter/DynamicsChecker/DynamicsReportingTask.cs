using System.Collections.Generic;
using Arbiter.Runtime.Tasks;
using Kingmaker.QA.Arbiter.GameCore.DynamicsChecker;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter.DynamicsChecker;

public class DynamicsReportingTask : ArbiterCheckerTask
{
	private readonly ArbiterStartupParameters m_Arguments;

	private DynamicsCheckerComponent m_DynamicsCheckerComponent;

	public override string CheckerType => "DynamicTest";

	public DynamicsReportingTask(DynamicsCheckerComponent dynamicsCheckerComponent, ArbiterStartupParameters arguments)
	{
		m_Arguments = arguments;
		m_DynamicsCheckerComponent = dynamicsCheckerComponent;
		base.Status = "Loading " + dynamicsCheckerComponent.OwnerBlueprint.name;
	}

	protected override IEnumerable<ArbiterTask> CheckerRoutine(GeneralProbeData probeData)
	{
		if (!m_DynamicsCheckerComponent.DebugStart)
		{
			yield return new StartNewGameFromPresetTask(this, m_DynamicsCheckerComponent.StartPreset);
			yield return new SetScreenResolutionTask(m_Arguments, this);
		}
		yield return new CommandsRunnerTask(this, m_DynamicsCheckerComponent.CommandList, probeData);
	}
}
