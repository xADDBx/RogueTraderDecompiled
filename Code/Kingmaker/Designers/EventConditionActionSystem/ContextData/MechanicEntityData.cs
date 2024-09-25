using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class MechanicEntityData : ContextData<MechanicEntityData>
{
	private MechanicEntity m_Entity;

	[CanBeNull]
	public static BaseUnitEntity CurrentBaseUnit => ContextData<MechanicEntityData>.Current?.m_Entity as BaseUnitEntity;

	[CanBeNull]
	public static UnitEntity CurrentUnit => ContextData<MechanicEntityData>.Current?.m_Entity as UnitEntity;

	[CanBeNull]
	public static StarshipEntity CurrentStarship => ContextData<MechanicEntityData>.Current?.m_Entity as StarshipEntity;

	[CanBeNull]
	public static MapObjectEntity CurrentMapObject => ContextData<MechanicEntityData>.Current?.m_Entity as MapObjectEntity;

	[CanBeNull]
	public static MechanicEntity CurrentEntity => ContextData<MechanicEntityData>.Current?.m_Entity;

	public MechanicEntityData Setup([NotNull] MechanicEntity entity)
	{
		m_Entity = entity;
		return this;
	}

	protected override void Reset()
	{
		m_Entity = null;
	}
}
