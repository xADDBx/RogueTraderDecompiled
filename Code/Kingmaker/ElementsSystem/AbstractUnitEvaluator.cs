using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.ElementsSystem;

[Serializable]
public abstract class AbstractUnitEvaluator : MechanicEntityEvaluator, IEvaluator<AbstractUnitEntity>
{
	public new AbstractUnitEntity GetValue()
	{
		return (base.GetValue() as AbstractUnitEntity) ?? throw new FailToEvaluateException(this);
	}

	public bool TryGetValue(out AbstractUnitEntity value)
	{
		if (TryGetValue(out MechanicEntity value2))
		{
			value = value2 as AbstractUnitEntity;
			return value != null;
		}
		value = null;
		return false;
	}

	protected sealed override Entity GetValueInternal()
	{
		return GetAbstractUnitEntityInternal();
	}

	[CanBeNull]
	protected abstract AbstractUnitEntity GetAbstractUnitEntityInternal();

	public bool Is(MechanicEntity value)
	{
		try
		{
			return GetValueInternal() == value;
		}
		catch
		{
			return false;
		}
	}
}
