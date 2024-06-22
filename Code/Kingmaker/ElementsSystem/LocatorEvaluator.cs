using System;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;

namespace Kingmaker.ElementsSystem;

[Serializable]
public abstract class LocatorEvaluator : EntityEvaluator, IEvaluator<LocatorEntity>
{
	public new LocatorEntity GetValue()
	{
		return (LocatorEntity)base.GetValue();
	}

	public bool TryGetValue(out LocatorEntity value)
	{
		if (TryGetValue(out Entity value2))
		{
			value = value2 as LocatorEntity;
			return value != null;
		}
		value = null;
		return false;
	}
}
