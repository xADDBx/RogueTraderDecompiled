using System.Collections;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.QA.Clockwork;

public class TaskAttack : ClockworkRunnerTask
{
	private BaseUnitEntity m_Unit;

	public TaskAttack(ClockworkRunner runner, BaseUnitEntity unit = null)
		: base(runner)
	{
		m_Unit = unit;
	}

	protected override IEnumerator Routine()
	{
		if (m_Unit != null)
		{
			yield return new TaskMovePartyToPoint(Runner, m_Unit.Position);
			new ClickUnitHandler().OnClick(m_Unit.View.gameObject, m_Unit.Position, 0);
		}
	}

	public override string ToString()
	{
		return $"Attack: {m_Unit}";
	}
}
