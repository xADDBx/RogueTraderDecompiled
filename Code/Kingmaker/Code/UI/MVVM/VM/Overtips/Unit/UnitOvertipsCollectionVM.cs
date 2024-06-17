using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;

public class UnitOvertipsCollectionVM : OvertipsCollectionVM<OvertipEntityUnitVM>, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IGameModeHandler, IReloadMechanicsHandler, IAreaActivationHandler, IAreaHandler, ISurroundingInteractableObjectsCountHandler, IEntitySuppressedHandler, ISubscriber<IEntity>
{
	private static readonly float DeathEntityRemoveDelay = 4f;

	public readonly ReactiveProperty<bool> TBMEnabled = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsCutscene = new ReactiveProperty<bool>(initialValue: false);

	private readonly Dictionary<Entity, IDisposable> m_DelayedRemoveHandlers = new Dictionary<Entity, IDisposable>();

	protected override IEnumerable<Entity> Entities => Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity e) => e.IsInGame && !e.Suppressed);

	protected override Func<OvertipEntityUnitVM, Entity, bool> OvertipGetter => (OvertipEntityUnitVM vm, Entity entity) => vm.Unit == entity as MechanicEntity || vm.UnitUIWrapper.UniqueId == entity.UniqueId;

	public UnitOvertipsCollectionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		if (TurnController.IsInTurnBasedCombat())
		{
			HandleTurnBasedModeResumed();
		}
		RescanEntities();
	}

	protected override void Clear()
	{
		m_DelayedRemoveHandlers.Values.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_DelayedRemoveHandlers.Clear();
		base.Clear();
	}

	protected override void RemoveEntity(Entity entityData)
	{
		TryCancelDelayedRemoveHandler(entityData);
		OvertipEntityUnitVM overtip = GetOvertip(entityData);
		if (overtip == null || !overtip.UnitState.HasLoot.Value)
		{
			base.RemoveEntity(entityData);
		}
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		if (UIUtility.IsGlobalMap() || UIUtility.IsShipArea())
		{
			return entityData.Parts.GetOptional<PartStarship>() != null;
		}
		return true;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TBMEnabled.Value = isTurnBased;
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !(abstractUnitEntity is LightweightUnitEntity))
		{
			AddEntity(abstractUnitEntity);
		}
	}

	public void HandleEntitySuppressionChanged(IEntity entity, bool suppressed)
	{
		if (entity is AbstractUnitEntity entityData && !(entity is LightweightUnitEntity))
		{
			if (suppressed)
			{
				TryImmediatelyRemoveEntity(entityData);
			}
			else
			{
				AddEntity(entityData);
			}
		}
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !(abstractUnitEntity is LightweightUnitEntity))
		{
			RemoveEntity(abstractUnitEntity);
		}
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !(abstractUnitEntity is LightweightUnitEntity))
		{
			UnitStatesHolderVM.Instance.GetOrCreateUnitState(abstractUnitEntity).UpdateProperties();
			TryDelayedRemoveEntity(abstractUnitEntity);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsCutscene.Value = gameMode == GameModeType.Cutscene;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		Clear();
	}

	public void OnAreaDidLoad()
	{
		RescanEntities();
	}

	public void OnAreaActivated()
	{
		RescanEntities();
	}

	void IReloadMechanicsHandler.OnMechanicsReloaded()
	{
		RescanEntities();
	}

	void IReloadMechanicsHandler.OnBeforeMechanicsReload()
	{
		Clear();
	}

	public void ShowBark(AbstractUnitEntity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(AbstractUnitEntity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	private void TryDelayedRemoveEntity(Entity entityData)
	{
		TryCancelDelayedRemoveHandler(entityData);
		float time = GetOvertip(entityData)?.DeathDelay ?? DeathEntityRemoveDelay;
		m_DelayedRemoveHandlers[entityData] = DelayedInvoker.InvokeInTime(delegate
		{
			RemoveEntity(entityData);
		}, time);
	}

	private void TryImmediatelyRemoveEntity(Entity entityData)
	{
		GetOvertip(entityData)?.SetDestroyViewImmediately();
		RemoveEntity(entityData);
	}

	private void TryCancelDelayedRemoveHandler(Entity entityData)
	{
		if (m_DelayedRemoveHandlers.TryGetValue(entityData, out var _))
		{
			m_DelayedRemoveHandlers.Remove(entityData);
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		TBMEnabled.Value = true;
	}

	public void HandleSurroundingInteractableObjectsCountChanged(EntityViewBase entity, bool isInNavigation, bool isChosen)
	{
		GetOvertip(entity.Data.ToEntity())?.HandleSurroundingObjectsChanged(isInNavigation, isChosen);
	}
}
