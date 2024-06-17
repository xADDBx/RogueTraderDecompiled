using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

public class AreaEffectContextData : ContextData<AreaEffectContextData>
{
	public AreaEffectEntity Entity { get; private set; }

	public AreaEffectContextData Setup(AreaEffectEntity entity)
	{
		Entity = entity;
		return this;
	}

	protected override void Reset()
	{
		Entity = null;
	}
}
