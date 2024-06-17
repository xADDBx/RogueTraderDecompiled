using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.EntitySystem.Interfaces;

public static class EntityInterfacesHelper
{
	public static readonly Type UnitEntityInterface = typeof(IUnitEntity);

	public static readonly Type MapObjectEntityInterface = typeof(IMapObjectEntity);

	public static readonly Type AreaEffectEntityInterface = typeof(IAreaEffectEntity);

	public static readonly Type SectorMapEntityInterface = typeof(ISectorMapObjectEntity);

	public static readonly Type SectorMapPassageEntityInterface = typeof(ISectorMapPassageEntity);

	public static readonly Type EntityInterface = typeof(IEntity);

	public static readonly Type StarshipEntityEntityInterface = typeof(IStarshipEntity);

	public static readonly Type ItemEntityInterface = typeof(IItemEntity);

	public static readonly Type BaseUnitEntityInterface = typeof(IBaseUnitEntity);

	public static readonly Type MechanicEntityInterface = typeof(IMechanicEntity);

	public static readonly Type StarSystemObjectEntityInterface = typeof(IStarSystemObjectEntity);

	public static readonly Dictionary<Type, Type> InterfacesCache = new Dictionary<Type, Type>();

	public static Type GetEntityInterfaceType<T>() where T : IEntity
	{
		Type typeFromHandle = typeof(T);
		if (InterfacesCache.TryGetValue(typeFromHandle, out var value))
		{
			return value;
		}
		Type[] interfaces = typeFromHandle.GetInterfaces();
		if (interfaces.Contains(StarSystemObjectEntityInterface))
		{
			InterfacesCache[typeFromHandle] = StarSystemObjectEntityInterface;
			return StarSystemObjectEntityInterface;
		}
		if (interfaces.Contains(MapObjectEntityInterface))
		{
			InterfacesCache[typeFromHandle] = MapObjectEntityInterface;
			return MapObjectEntityInterface;
		}
		if (interfaces.Contains(AreaEffectEntityInterface))
		{
			InterfacesCache[typeFromHandle] = AreaEffectEntityInterface;
			return AreaEffectEntityInterface;
		}
		if (interfaces.Contains(SectorMapEntityInterface))
		{
			InterfacesCache[typeFromHandle] = SectorMapEntityInterface;
			return SectorMapEntityInterface;
		}
		if (interfaces.Contains(SectorMapPassageEntityInterface))
		{
			InterfacesCache[typeFromHandle] = SectorMapPassageEntityInterface;
			return SectorMapPassageEntityInterface;
		}
		if (interfaces.Contains(StarshipEntityEntityInterface))
		{
			InterfacesCache[typeFromHandle] = StarshipEntityEntityInterface;
			return StarshipEntityEntityInterface;
		}
		if (interfaces.Contains(ItemEntityInterface))
		{
			InterfacesCache[typeFromHandle] = ItemEntityInterface;
			return ItemEntityInterface;
		}
		if (interfaces.Contains(UnitEntityInterface))
		{
			InterfacesCache[typeFromHandle] = UnitEntityInterface;
			return UnitEntityInterface;
		}
		if (interfaces.Contains(BaseUnitEntityInterface))
		{
			InterfacesCache[typeFromHandle] = BaseUnitEntityInterface;
			return BaseUnitEntityInterface;
		}
		if (interfaces.Contains(MechanicEntityInterface))
		{
			InterfacesCache[typeFromHandle] = MechanicEntityInterface;
			return MechanicEntityInterface;
		}
		InterfacesCache[typeFromHandle] = EntityInterface;
		return EntityInterface;
	}
}
