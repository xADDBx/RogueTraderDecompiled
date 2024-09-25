using System.Collections;

namespace Kingmaker.QA.Arbiter.DynamicsChecker;

public class DynamicsCheckerTask : ArbiterTask
{
	private readonly ArbiterStartupParameters m_Arguments;

	private DynamicsCheckerComponent m_DynamicsCheckerComponent;

	public DynamicsCheckerTask(DynamicsCheckerComponent dynamicsCheckerComponent, ArbiterStartupParameters arguments)
	{
		m_Arguments = arguments;
		m_DynamicsCheckerComponent = dynamicsCheckerComponent;
		base.Status = "Loading " + dynamicsCheckerComponent.OwnerBlueprint.name;
	}

	protected override IEnumerator Routine()
	{
		Arbiter.Instance.Reporter.ReportInstructionStarted(Arbiter.Instruction.name);
		PFLog.Arbiter.Log($"Processing {Arbiter.Instruction}");
		if (!m_DynamicsCheckerComponent.DebugStart)
		{
			yield return new StartNewGameFromPresetTask(this, m_DynamicsCheckerComponent.StartPreset);
			yield return new SetScreenResolutionTask(m_Arguments, this);
		}
		GeneralProbeData probeData = new GeneralProbeData(Arbiter.Instruction.name, "DynamicTest");
		yield return new CommandsRunnerTask(this, m_DynamicsCheckerComponent.CommandList, probeData);
		probeData.SendToServer();
		probeData.CleanupData();
		Arbiter.Instance.Reporter.ReportInstructionPassed(Arbiter.Instruction.name, base.ElapsedTestTime);
	}
}
