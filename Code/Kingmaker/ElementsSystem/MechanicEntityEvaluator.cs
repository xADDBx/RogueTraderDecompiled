using System;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.ElementsSystem;

[Serializable]
public abstract class MechanicEntityEvaluator : EntityEvaluator, IEvaluator<MechanicEntity>
{
	public new MechanicEntity GetValue()
	{
		return (MechanicEntity)base.GetValue();
	}

	public bool TryGetValue(out MechanicEntity value)
	{
		if (TryGetValue(out Entity value2))
		{
			value = value2 as MechanicEntity;
			return value != null;
		}
		value = null;
		return false;
	}
}
