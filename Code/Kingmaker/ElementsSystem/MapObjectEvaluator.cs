using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.ElementsSystem;

[Serializable]
public abstract class MapObjectEvaluator : MechanicEntityEvaluator, IEvaluator<MapObjectEntity>
{
	public new MapObjectEntity GetValue()
	{
		return (MapObjectEntity)base.GetValue();
	}

	public bool TryGetValue(out MapObjectEntity value)
	{
		if (TryGetValue(out MechanicEntity value2))
		{
			value = value2 as MapObjectEntity;
			return value != null;
		}
		value = null;
		return false;
	}

	protected sealed override Entity GetValueInternal()
	{
		return GetMapObjectInternal();
	}

	[CanBeNull]
	protected abstract MapObjectEntity GetMapObjectInternal();
}
