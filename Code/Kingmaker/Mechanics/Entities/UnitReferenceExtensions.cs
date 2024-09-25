using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Mechanics.Entities;

public static class UnitReferenceExtensions
{
	public static AbstractUnitEntity ToAbstractUnitEntity(this UnitReference r)
	{
		if (!(r == null))
		{
			return (AbstractUnitEntity)r.Entity;
		}
		return null;
	}

	public static IBaseUnitEntity ToIBaseUnitEntity(this UnitReference r)
	{
		if (!(r == null))
		{
			return (IBaseUnitEntity)r.Entity;
		}
		return null;
	}

	public static BaseUnitEntity ToBaseUnitEntity(this UnitReference r)
	{
		if (!(r == null))
		{
			return (BaseUnitEntity)r.Entity;
		}
		return null;
	}

	public static UnitReference FromAbstractUnitEntity(this AbstractUnitEntity unit)
	{
		if (unit == null)
		{
			return UnitReference.NullUnitReference;
		}
		return new UnitReference(unit.UniqueId);
	}

	public static UnitReference FromBaseUnitEntity(this BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return UnitReference.NullUnitReference;
		}
		return new UnitReference(unit.UniqueId);
	}
}
