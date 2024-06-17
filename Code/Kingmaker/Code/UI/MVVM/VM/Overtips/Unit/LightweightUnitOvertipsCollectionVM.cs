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
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;

public class LightweightUnitOvertipsCollectionVM : OvertipsCollectionVM<LightweightUnitOvertipVM>, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IGameModeHandler, IReloadMechanicsHandler, IAreaActivationHandler, IAreaHandler, ISurroundingInteractableObjectsCountHandler, IEntitySuppressedHandler, ISubscriber<IEntity>
{
	private static readonly float DeathEntityRemoveDelay = 4f;

	public readonly ReactiveProperty<bool> TBMEnabled = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsCutscene = new ReactiveProperty<bool>(initialValue: false);

	private readonly Dictionary<Entity, IDisposable> m_DelayedRemoveHandlers = new Dictionary<Entity, IDisposable>();

	protected override IEnumerable<Entity> Entities => Game.Instance.State.AllUnits.All.Where((AbstractUnitEntity e) => e.IsInGame && e is LightweightUnitEntity);

	protected override Func<LightweightUnitOvertipVM, Entity, bool> OvertipGetter => (LightweightUnitOvertipVM vm, Entity entity) => vm.Unit == entity as MechanicEntity || vm.UnitUIWrapper.UniqueId == entity.UniqueId;

	public LightweightUnitOvertipsCollectionVM()
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
		base.RemoveEntity(entityData);
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		return true;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TBMEnabled.Value = isTurnBased;
		if (!isTurnBased)
		{
			RescanEntities();
		}
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity is LightweightUnitEntity)
		{
			UnitStatesHolderVM.Instance.GetOrCreateUnitState(abstractUnitEntity).UpdateProperties();
			AddEntity(abstractUnitEntity);
		}
	}

	public void HandleEntitySuppressionChanged(IEntity entity, bool suppressed)
	{
		if (entity is LightweightUnitEntity entityData)
		{
			if (suppressed)
			{
				DelayedRemoveEntity(entityData);
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
		if (abstractUnitEntity is LightweightUnitEntity)
		{
			RemoveEntity(abstractUnitEntity);
		}
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity is LightweightUnitEntity)
		{
			DelayedRemoveEntity(abstractUnitEntity);
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

	public void ShowBark(LightweightUnitEntity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(LightweightUnitEntity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	private void DelayedRemoveEntity(Entity entityData)
	{
		TryCancelDelayedRemoveHandler(entityData);
		float time = GetOvertip(entityData)?.DeathDelay ?? DeathEntityRemoveDelay;
		m_DelayedRemoveHandlers[entityData] = DelayedInvoker.InvokeInTime(delegate
		{
			RemoveEntity(entityData);
		}, time);
	}

	private void TryCancelDelayedRemoveHandler(Entity entityData)
	{
		if (m_DelayedRemoveHandlers.ContainsKey(entityData))
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
