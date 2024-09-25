using System.Collections;
using System.Linq;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects.SriptZones;

namespace Kingmaker.QA.Clockwork;

public class TaskInteractWithScriptZone : ClockworkRunnerTask
{
	private Entity m_ZoneEntity;

	public TaskInteractWithScriptZone(ClockworkRunner runner, Entity entity)
		: base(runner)
	{
		m_ZoneEntity = entity;
	}

	protected override IEnumerator Routine()
	{
		ScriptZone scriptZone = m_ZoneEntity.View as ScriptZone;
		yield return new TaskMovePartyToPoint(Runner, scriptZone.Shapes.First().Center());
		Runner.MarkAsInteracted(m_ZoneEntity.UniqueId);
	}

	public override string ToString()
	{
		return "Interact with script zone";
	}
}
