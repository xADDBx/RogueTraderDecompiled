using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class ScriptZoneTriggerData : SingleUnitData<ScriptZoneTriggerData>
{
	public SceneEntitiesState State { get; private set; }

	public ScriptZoneTriggerData Setup([NotNull] BaseUnitEntity unit, [CanBeNull] SceneEntitiesState state)
	{
		Setup(unit);
		State = state;
		return this;
	}

	protected override void Reset()
	{
		State = null;
		base.Reset();
	}
}
