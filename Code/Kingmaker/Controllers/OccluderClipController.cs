using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;

namespace Kingmaker.Controllers;

public sealed class OccluderClipController : IControllerTick, IController, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IUnitFactionHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IViewAttachedHandler, ISubscriber<IEntity>, IControllerStart
{
	private const string InvalidateEntityViewProfilingScopeName = "OccluderClipController.InvalidateEntityView";

	private const string UpdateEntitiesProfilingScopeName = "OccluderClipController.UpdateEntities";

	private readonly HashSet<UnitEntityView> m_InvalidatedViews = new HashSet<UnitEntityView>();

	void IControllerStart.OnStart()
	{
		InvalidateAllEntityViews();
	}

	void IViewAttachedHandler.OnViewAttached(IEntityViewBase view)
	{
		InvalidateEntityView(((EntityViewBase)view) as UnitEntityView);
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		InvalidateEntityView(EventInvokerExtensions.BaseUnitEntity.View);
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		InvalidateEntityView(EventInvokerExtensions.BaseUnitEntity.View);
	}

	void IUnitFactionHandler.HandleFactionChanged()
	{
		InvalidateEntityView(EventInvokerExtensions.BaseUnitEntity.View);
	}

	void IUnitLifeStateChanged.HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			InvalidateEntityView(baseUnitEntity.View);
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		UpdateEntities();
	}

	private void InvalidateAllEntityViews()
	{
		EntityPool<BaseUnitEntity> allBaseUnits = Game.Instance.State.AllBaseUnits;
		if (allBaseUnits == null)
		{
			return;
		}
		foreach (BaseUnitEntity item in allBaseUnits)
		{
			InvalidateEntityView(item.View);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void InvalidateEntityView(UnitEntityView view)
	{
		using (ProfileScope.New("OccluderClipController.InvalidateEntityView"))
		{
			if (view != null)
			{
				m_InvalidatedViews.Add(view);
			}
		}
	}

	private void UpdateEntities()
	{
		using (ProfileScope.New("OccluderClipController.UpdateEntities"))
		{
			try
			{
				foreach (UnitEntityView invalidatedView in m_InvalidatedViews)
				{
					if (!(invalidatedView == null))
					{
						BaseUnitEntity entityData = invalidatedView.EntityData;
						if (entityData != null)
						{
							invalidatedView.SetOccluderClippingEnabled(CalculateOccluderClippingState(entityData));
						}
					}
				}
			}
			finally
			{
				m_InvalidatedViews.Clear();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CalculateOccluderClippingState(BaseUnitEntity entity)
	{
		PartLifeState lifeStateOptional = entity.GetLifeStateOptional();
		if (lifeStateOptional != null && lifeStateOptional.State == UnitLifeState.Dead)
		{
			return false;
		}
		UnitPartCompanion optional = entity.GetOptional<UnitPartCompanion>();
		if (optional != null && optional.State == CompanionState.InParty)
		{
			return true;
		}
		PartUnitCombatState optional2 = entity.GetOptional<PartUnitCombatState>();
		if (optional2 != null && optional2.IsInCombat)
		{
			return true;
		}
		return false;
	}
}
