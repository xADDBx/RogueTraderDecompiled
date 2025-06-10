using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.QA.Arbiter.GameCore.StaticPerformanceChecker;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Service.StaticPerformanceTest;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter.StaticPerformanceChecker;

public class StaticPerformanceCheckerTask : ArbiterTask
{
	private readonly BlueprintAreaPreset m_StartPreset;

	private readonly int m_Step;

	public StaticPerformanceCheckerTask(StaticPerformanceComponent component, int step)
	{
		m_StartPreset = component.Preset;
		m_Step = step;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		yield return new LoadPresetTask(this, m_StartPreset);
		base.Status = "Collect static performance for " + m_StartPreset.Area.AreaName.Text;
		yield return new StaticPerformanceCollectTask(m_Step, 0, true, ArbiterService.Instance.SceneBoundary, ArbiterService.Instance.Integration);
	}
}
