using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.Controllers;

public class EntityVisibilityForPlayerController : IControllerTick, IController
{
	private int m_VisibleEntityRevealingDisabledCounter;

	public void EnableVisibleEntityRevealing()
	{
		m_VisibleEntityRevealingDisabledCounter--;
	}

	public void DisableVisibleEntityRevealing()
	{
		m_VisibleEntityRevealingDisabledCounter++;
	}

	public bool IsVisibleEntityRevealingEnabled()
	{
		return m_VisibleEntityRevealingDisabledCounter <= 0;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		bool revealVisible = IsVisibleEntityRevealingEnabled();
		foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
		{
			Update(mechanicEntity, revealVisible);
		}
	}

	private static void Update(MechanicEntity entity, bool revealVisible)
	{
		BaseUnitEntity baseUnitEntity = entity as BaseUnitEntity;
		if (!(entity.View == null) && (baseUnitEntity == null || !baseUnitEntity.IsSleeping || (baseUnitEntity.LifeState.IsFinallyDead && !baseUnitEntity.LifeState.IsDeathRevealed)))
		{
			bool visible = ((baseUnitEntity != null) ? IsVisible(baseUnitEntity) : IsVisible(entity));
			entity.View.SetVisible(visible, force: false, revealVisible);
		}
	}

	private static bool IsVisible(BaseUnitEntity unit)
	{
		if ((bool)unit.Features.Hidden)
		{
			return false;
		}
		if (unit.LifeState.IsHiddenBecauseDead)
		{
			return false;
		}
		UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
		if (optional != null && optional.State == CompanionState.InParty)
		{
			return true;
		}
		if (unit.IsInvisible)
		{
			return false;
		}
		if (unit.IsInFogOfWar)
		{
			return false;
		}
		if (unit.Stealth.Active)
		{
			foreach (BaseUnitEntity item in unit.Stealth.SpottedBy)
			{
				if (item.Faction.IsPlayer)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private static bool IsVisible(MechanicEntity entity)
	{
		if (entity is MapObjectEntity { IsAwarenessCheckPassed: false } || (!entity.IsRevealed && entity.IsInFogOfWar))
		{
			return false;
		}
		UnitPartFamiliar optional = entity.GetOptional<UnitPartFamiliar>();
		if (optional != null)
		{
			return optional.IsVisible;
		}
		if (entity is TrapObjectData trapObjectData)
		{
			return trapObjectData.TrapActive;
		}
		return true;
	}
}
