using System.Collections.Generic;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.QA.Arbiter.GameCore.AreaChecker;

namespace Kingmaker.QA.Arbiter.Tasks;

public class SetTimeOfDayTask : ArbiterTask
{
	private readonly AreaCheckerComponent m_AreaCheckerComponent;

	public SetTimeOfDayTask(ArbiterTask parent, AreaCheckerComponent areaCheckerComponent)
		: base(parent)
	{
		m_AreaCheckerComponent = areaCheckerComponent;
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		if (m_AreaCheckerComponent.OverrideTimeOfDay)
		{
			TimeOfDay timeOfDay = m_AreaCheckerComponent.TimeOfDay;
			Game.Instance.TimeController.SkipGameTime(timeOfDay);
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
		}
	}
}
