using System;
using Kingmaker.AreaLogic.TimeSurvival;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.Settings;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class SpaceCombatFollowTasksProvider : IDisposable, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITimeSurvivalSpawnHandler, ISubscriber<IBaseUnitEntity>, IStarshipAttackHandler, IUnitCommandStartHandler, IUnitCommandEndHandler, IUnitDeathHandler
{
	private readonly Action<ICameraFollowTask, bool, float> m_AddTask;

	private bool m_IsCommandInAction;

	private uint m_AttackCooldownHandle;

	private BlueprintCameraFollowSettings SpaceCameraFollowSettings => BlueprintRoot.Instance.SpaceCameraFollowSettings;

	private bool CameraFollowUnit => SettingsRoot.Game.TurnBased.CameraFollowUnit.GetValue();

	private bool CameraScrollToCurrentUnit => SettingsRoot.Game.TurnBased.CameraScrollToCurrentUnit.GetValue();

	private bool IsAttackCooldown => m_AttackCooldownHandle != 0;

	private Rect SafeRect => CameraFollowTasksSceneHelper.Instance.SafeRect;

	public SpaceCombatFollowTasksProvider(Action<ICameraFollowTask, bool, float> addTaskAction)
	{
		m_AddTask = addTaskAction;
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		Game.Instance.CustomCallbackController.Cancel(m_AttackCooldownHandle);
		m_AttackCooldownHandle = 0u;
		EventBus.Unsubscribe(this);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			Game.Instance.CameraController?.Follower?.Release();
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (CameraScrollToCurrentUnit && mechanicEntity.IsInPlayerParty && mechanicEntity is BaseUnitEntity baseUnitEntity && !mechanicEntity.IsDead && baseUnitEntity.LifeState.IsConscious)
			{
				UIAccess.SelectionManager.SelectUnit(baseUnitEntity.View);
				CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
				cameraFollowTaskContext.Owner = mechanicEntity;
				cameraFollowTaskContext.Params = SpaceCameraFollowSettings.NewTurn;
				CameraFollowTaskContext context = cameraFollowTaskContext;
				AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: false);
			}
			else if (CameraFollowUnit && !mechanicEntity.IsInPlayerParty && !mechanicEntity.IsInFogOfWar && !mechanicEntity.IsInvisible())
			{
				CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
				cameraFollowTaskContext.Owner = mechanicEntity;
				cameraFollowTaskContext.Params = SpaceCameraFollowSettings.NewTurn;
				CameraFollowTaskContext context2 = cameraFollowTaskContext;
				AddTask(CameraFollowTaskFactory.GetFollowTask(context2), checkOnScreen: true);
			}
		}
	}

	public void HandleAttack(RuleStarshipPerformAttack starshipAttack)
	{
		if (IsAttackCooldown)
		{
			return;
		}
		if (CheckUnit(starshipAttack.Target))
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = starshipAttack.Target;
			cameraFollowTaskContext.Params = SpaceCameraFollowSettings.Attacked;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddUnitsObserveTask(context, starshipAttack.Initiator.Position, starshipAttack.Target.Position);
		}
		Game.Instance.CustomCallbackController.Cancel(m_AttackCooldownHandle);
		m_AttackCooldownHandle = Game.Instance.CustomCallbackController.InvokeInTime(delegate
		{
			if (!m_IsCommandInAction)
			{
				TryAddReturnToUnitTask(starshipAttack.Initiator);
			}
			m_AttackCooldownHandle = 0u;
		}, SpaceCameraFollowSettings.Attacked.DefaultParams.CameraObserveTime);
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		m_IsCommandInAction = true;
		if (CheckActionCamera(command))
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Caster = command.Executor;
			cameraFollowTaskContext.Target = command.TargetUnit;
			cameraFollowTaskContext.Priority = 3;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddTask(CameraFollowTaskFactory.GetActionCameraTask(context), checkOnScreen: false);
			EventBus.RaiseEvent(delegate(IHideUIWhileActionCameraHandler h)
			{
				h.HandleHideUI();
			});
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		m_IsCommandInAction = false;
		if (command is UnitUseAbility)
		{
			if (!IsAttackCooldown)
			{
				TryAddReturnToUnitTask(command.Executor);
			}
			EventBus.RaiseEvent(delegate(IHideUIWhileActionCameraHandler h)
			{
				h.HandleShowUI();
			});
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (m_IsCommandInAction && unitEntity is BaseUnitEntity baseUnitEntity)
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = baseUnitEntity;
			cameraFollowTaskContext.Params = SpaceCameraFollowSettings.UnitDeath;
			cameraFollowTaskContext.Priority = 1;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
		}
	}

	private void AddUnitsObserveTask(CameraFollowTaskContext context, Vector3 startPos, Vector3 endPos)
	{
		if (IsPointOnScreen(startPos) && IsPointOnScreen(endPos))
		{
			context.Position = (startPos + endPos) / 2f;
		}
		else
		{
			context.Position = endPos;
		}
		AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
	}

	private bool IsPointOnScreen(Vector3 point)
	{
		Vector3 point2 = ObjectExtensions.Or(CameraRig.Instance.Camera, null)?.WorldToViewportPoint(point) ?? Vector3.positiveInfinity;
		return SafeRect.Contains(point2);
	}

	private void TryAddReturnToUnitTask(MechanicEntity unitEntity)
	{
		if (CameraScrollToCurrentUnit && CheckUnit(unitEntity) && unitEntity.IsInPlayerParty && unitEntity == Game.Instance.TurnController.CurrentUnit)
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = unitEntity;
			cameraFollowTaskContext.Params = SpaceCameraFollowSettings.CommandDidEnd;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
		}
	}

	private bool CheckUnit(TargetWrapper targetWrapper)
	{
		if (!CameraScrollToCurrentUnit)
		{
			return false;
		}
		if (targetWrapper == null)
		{
			return false;
		}
		MechanicEntity entity = targetWrapper.Entity;
		if (entity == null || entity.IsDead)
		{
			return false;
		}
		MechanicEntity entity2 = targetWrapper.Entity;
		if (entity2 == null || !entity2.IsInCombat)
		{
			return false;
		}
		if (targetWrapper.Entity is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity.LifeState.IsConscious;
		}
		return true;
	}

	private void AddTask(ICameraFollowTask task, bool checkOnScreen)
	{
		bool num = task.TaskParams?.SkipIfOnScreen ?? false;
		bool flag = task.TaskParams?.ForceTimescale ?? false;
		if (num && checkOnScreen && IsPointOnScreen(task.Position))
		{
			if (!flag)
			{
				return;
			}
			task = CameraFollowTaskFactory.GetDoNothingWaitTask(task);
		}
		m_AddTask?.Invoke(task, arg2: false, 0f);
	}

	public bool CheckActionCamera(AbstractUnitCommand command)
	{
		if ((bool)SettingsRoot.Game.TurnBased.DisableActionCamera)
		{
			return false;
		}
		if (!(command is UnitUseAbility unitUseAbility))
		{
			return false;
		}
		AbilityActionCamera abilityActionCamera = unitUseAbility?.Ability.Blueprint.GetComponent<AbilityActionCamera>();
		if (abilityActionCamera == null)
		{
			return false;
		}
		AbilityActionCameraSettings settings = abilityActionCamera.GetSettings(unitUseAbility);
		if (!settings.IsValid)
		{
			return false;
		}
		if (settings.TriggerActionCameraChance > PFStatefulRandom.Camera.Range(0, 100))
		{
			return true;
		}
		return false;
	}

	public void HandleStarshipSpawnStarted()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		TimeSurvival component = Game.Instance.CurrentlyLoadedArea.GetComponent<TimeSurvival>();
		if (CameraFollowUnit && component != null && mechanicEntity != null && !mechanicEntity.IsInPlayerParty && !mechanicEntity.IsInFogOfWar && !mechanicEntity.IsInvisible())
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = mechanicEntity;
			cameraFollowTaskContext.Params = SpaceCameraFollowSettings.NewTurn;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddTask(CameraFollowTaskFactory.GetFollowTask(context), checkOnScreen: true);
		}
	}
}
