using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class SpawnedUnitData : AbstractUnitData<SpawnedUnitData>
{
	public SceneEntitiesState State { get; private set; }

	public SpawnedUnitData Setup([NotNull] AbstractUnitEntity unit, [NotNull] SceneEntitiesState state)
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
