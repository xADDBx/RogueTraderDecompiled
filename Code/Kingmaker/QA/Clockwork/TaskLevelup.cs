using System;
using System.Collections;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.QA.Clockwork;

[Obsolete]
public class TaskLevelup : ClockworkRunnerTask
{
	private BaseUnitEntity m_Unit;

	public TaskLevelup(ClockworkRunner runner, BaseUnitEntity unit)
		: base(runner)
	{
		m_Unit = unit;
	}

	protected override IEnumerator Routine()
	{
		yield break;
	}

	public override string ToString()
	{
		return "Level up " + m_Unit.CharacterName;
	}
}
