using System;
using Kingmaker.AI;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class SurfaceCombatFollowTasksProvider : IDisposable, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler, IWarhammerAttackHandler, IUnitCommandStartHandler, IUnitCommandEndHandler, IUnitDeathHandler, IUnitCommandActHandler, ICameraFocusTargetHandler
{
	private readonly Action<ICameraFollowTask, bool, float> m_AddTask;

	private bool m_IsCommandInAction;

	private uint m_AttackCooldownHandle;

	private BlueprintCameraFollowSettings CameraFollowSettings => BlueprintRoot.Instance.CameraFollowSettings;

	private bool CameraFollowUnit => SettingsRoot.Game.TurnBased.CameraFollowUnit.GetValue();

	private bool CameraScrollToCurrentUnit => SettingsRoot.Game.TurnBased.CameraScrollToCurrentUnit.GetValue();

	private bool IsAttackCooldown => m_AttackCooldownHandle != 0;

	private bool IsTurnBased => Game.Instance.TurnController.TurnBasedModeActive;

	private Rect SafeRect => CameraFollowTasksSceneHelper.Instance.SafeRect;

	public SurfaceCombatFollowTasksProvider(Action<ICameraFollowTask, bool, float> addTaskAction)
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
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleUnitStartTurnInternal();
	}

	private void HandleUnitStartTurnInternal()
	{
		if (!IsTurnBased)
		{
			return;
		}
		Game.Instance.CameraController?.Follower?.Release();
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
		cameraFollowTaskContext.Owner = mechanicEntity;
		CameraFollowTaskContext cameraFollowTaskContext2 = cameraFollowTaskContext;
		if (CameraScrollToCurrentUnit && mechanicEntity.IsInPlayerParty && mechanicEntity is BaseUnitEntity baseUnitEntity && !mechanicEntity.IsDead && baseUnitEntity.LifeState.IsConscious)
		{
			UIAccess.SelectionManager.SelectUnit(baseUnitEntity.View);
			cameraFollowTaskContext2.Params = CameraFollowSettings.NewTurn;
			AddTask(CameraFollowTaskFactory.GetScrollToTask(cameraFollowTaskContext2), checkOnScreen: false);
		}
		else if (CameraFollowUnit && mechanicEntity is UnitSquad squad)
		{
			BaseUnitEntity baseUnitEntity2 = squad.SelectLeader();
			if (baseUnitEntity2 != null && baseUnitEntity2.IsRevealed && !baseUnitEntity2.IsInvisible())
			{
				cameraFollowTaskContext = default(CameraFollowTaskContext);
				cameraFollowTaskContext.Owner = baseUnitEntity2;
				cameraFollowTaskContext2 = cameraFollowTaskContext;
				cameraFollowTaskContext2.Params = CameraFollowSettings.NewTurn;
				AddTask(CameraFollowTaskFactory.GetFollowTask(cameraFollowTaskContext2), checkOnScreen: true);
			}
		}
		else
		{
			AddTask(HandleStartNonPartyMemberTurn(mechanicEntity, cameraFollowTaskContext2), checkOnScreen: true);
		}
	}

	private ICameraFollowTask HandleStartNonPartyMemberTurn(MechanicEntity entity, CameraFollowTaskContext ctx)
	{
		if (entity.GetOptional<PartFocusCameraOnEntity>()?.Entity != null)
		{
			return null;
		}
		if (CameraFollowUnit && entity is UnitSquad squad)
		{
			BaseUnitEntity baseUnitEntity = squad.SelectLeader();
			if (baseUnitEntity != null && baseUnitEntity.IsRevealed && !baseUnitEntity.IsInvisible())
			{
				CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
				cameraFollowTaskContext.Owner = baseUnitEntity;
				ctx = cameraFollowTaskContext;
				ctx.Params = CameraFollowSettings.NewTurn;
				return CameraFollowTaskFactory.GetFollowTask(ctx);
			}
		}
		else if (CameraFollowUnit && !entity.IsInPlayerParty && !entity.IsInSquad && entity.IsRevealed && !entity.IsInvisible())
		{
			ctx.Params = CameraFollowSettings.NewTurn;
			return CameraFollowTaskFactory.GetFollowTask(ctx);
		}
		return null;
	}

	public void HandleAttack(RulePerformAttack attackRule)
	{
		if (IsAttackCooldown || !CameraScrollToCurrentUnit)
		{
			return;
		}
		if (CheckUnit((MechanicEntity)attackRule.Target))
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = (MechanicEntity)attackRule.Target;
			cameraFollowTaskContext.Params = CameraFollowSettings.Attacked;
			cameraFollowTaskContext.IsMelee = attackRule.IsMelee;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddUnitsObserveTask(context, attackRule.Initiator.Position, attackRule.Target.Position);
		}
		CameraFollowTaskParamsEntry taskParamsEntry = CameraFollowHelper.GetTaskParamsEntry(CameraFollowSettings.Attacked, attackRule.IsMelee);
		Game.Instance.CustomCallbackController.Cancel(m_AttackCooldownHandle);
		m_AttackCooldownHandle = Game.Instance.CustomCallbackController.InvokeInTime(delegate
		{
			if (!m_IsCommandInAction)
			{
				TryAddReturnToUnitTask((MechanicEntity)attackRule.Initiator);
			}
			m_AttackCooldownHandle = 0u;
		}, taskParamsEntry.CameraObserveTime);
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Params.FromCutscene || !CheckUnitCommand(command) || !CameraScrollToCurrentUnit || !CheckUnit(command.Executor))
		{
			return;
		}
		m_IsCommandInAction = true;
		if (!(command is UnitUseAbility unitUseAbility))
		{
			if (command is UnitAttackOfOpportunity unitAttackOfOpportunity)
			{
				HandleUnitAttackOfOpportunity(unitAttackOfOpportunity);
			}
		}
		else
		{
			HandleUnitUseAbility(unitUseAbility);
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (!command.Params.FromCutscene && command is UnitUseAbility unitUseAbility && !(unitUseAbility.Target == null) && CameraScrollToCurrentUnit)
		{
			bool isMelee = unitUseAbility.Ability.Weapon?.Blueprint.IsMelee ?? false;
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = unitUseAbility.Target;
			cameraFollowTaskContext.Params = CameraFollowSettings.Targeting;
			cameraFollowTaskContext.IsMelee = isMelee;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddUnitsObserveTask(context, unitUseAbility.Executor.Position, unitUseAbility.Target.Point);
		}
	}

	private void HandleUnitUseAbility(UnitUseAbility unitUseAbility)
	{
		if (!unitUseAbility.Params.DisableCameraFollow && CameraScrollToCurrentUnit)
		{
			bool isMelee = unitUseAbility.Ability.Weapon?.Blueprint.IsMelee ?? false;
			CameraFollowTaskParams readyToAttack = CameraFollowSettings.ReadyToAttack;
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = unitUseAbility.Executor;
			cameraFollowTaskContext.Params = readyToAttack;
			cameraFollowTaskContext.IsMelee = isMelee;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
		}
	}

	private void HandleUnitAttackOfOpportunity(UnitAttackOfOpportunity unitAttackOfOpportunity)
	{
		CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
		cameraFollowTaskContext.Owner = unitAttackOfOpportunity.Executor;
		cameraFollowTaskContext.Params = CameraFollowSettings.AttackOfOpportunity;
		cameraFollowTaskContext.Priority = 1;
		CameraFollowTaskContext context = cameraFollowTaskContext;
		AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (!command.Params.FromCutscene && command is UnitUseAbility && CameraScrollToCurrentUnit)
		{
			m_IsCommandInAction = false;
			if (!IsAttackCooldown)
			{
				TryAddReturnToUnitTask(command.Executor);
			}
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (CameraScrollToCurrentUnit && m_IsCommandInAction && unitEntity is BaseUnitEntity baseUnitEntity)
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = baseUnitEntity;
			cameraFollowTaskContext.Params = CameraFollowSettings.UnitDeath;
			cameraFollowTaskContext.Priority = 1;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
		}
	}

	private void AddUnitsObserveTask(CameraFollowTaskContext context, Vector3 startPos, Vector3 endPos)
	{
		if (!context.IsMelee || !context.Params.HasMeleeParams)
		{
			_ = context.Params.DefaultParams;
		}
		else
		{
			_ = context.Params.MeleeParams;
		}
		if (!(startPos == default(Vector3)) && !(endPos == default(Vector3)))
		{
			context.Position = endPos;
			AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
		}
	}

	private void TryAddReturnToUnitTask(MechanicEntity unitEntity)
	{
		if (CheckUnit(unitEntity) && unitEntity.IsInPlayerParty && unitEntity == Game.Instance.TurnController.CurrentUnit)
		{
			CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
			cameraFollowTaskContext.Owner = unitEntity;
			cameraFollowTaskContext.Params = CameraFollowSettings.CommandDidEnd;
			CameraFollowTaskContext context = cameraFollowTaskContext;
			AddTask(CameraFollowTaskFactory.GetScrollToTask(context), checkOnScreen: true);
		}
	}

	private bool IsPointOnScreen(Vector3 point)
	{
		Vector3 point2 = CameraRig.Instance.Camera.Or(null)?.WorldToViewportPoint(point) ?? Vector3.positiveInfinity;
		return SafeRect.Contains(point2);
	}

	private bool CheckUnit(TargetWrapper targetWrapper)
	{
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

	private bool CheckUnitCommand(AbstractUnitCommand command)
	{
		if (command is UnitUseAbility { Params: var @params })
		{
			if (@params != null && !@params.DisableCameraFollow)
			{
				goto IL_0026;
			}
		}
		else if (command is UnitAttackOfOpportunity)
		{
			goto IL_0026;
		}
		return false;
		IL_0026:
		return true;
	}

	private void AddTask(ICameraFollowTask task, bool checkOnScreen)
	{
		if (task == null)
		{
			return;
		}
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

	public void HandleCameraRetain(MechanicEntity cameraFrom)
	{
		if (!IsTurnBased)
		{
			return;
		}
		Game.Instance.CameraController?.Follower?.Release();
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		ICameraFocusTarget cameraFocusTarget = mechanicEntity as ICameraFocusTarget;
		CameraFollowTaskContext cameraFollowTaskContext = default(CameraFollowTaskContext);
		cameraFollowTaskContext.Owner = mechanicEntity;
		CameraFollowTaskContext cameraFollowTaskContext2 = cameraFollowTaskContext;
		if (mechanicEntity != null)
		{
			if (cameraFrom is AbstractUnitEntity abstractUnitEntity)
			{
				cameraFollowTaskContext = default(CameraFollowTaskContext);
				cameraFollowTaskContext.Owner = abstractUnitEntity;
				CameraFollowTaskContext ctx = cameraFollowTaskContext;
				ctx.Params = CameraFollowSettings.CommandDidEnd;
				m_AddTask(HandleStartNonPartyMemberTurn(abstractUnitEntity, ctx), arg2: true, cameraFocusTarget?.TimeToFocus ?? 0f);
				abstractUnitEntity.GetOrCreate<PartFocusCameraOnEntity>().Entity = mechanicEntity;
			}
			cameraFollowTaskContext2.Params = CameraFollowSettings.NewTurn;
			CameraScrollToTask task = new CameraScrollToTask(CameraFollowHelper.GetTaskParamsEntry(cameraFollowTaskContext2.Params, cameraFollowTaskContext2.IsMelee), mechanicEntity, mechanicEntity.Position, 100);
			AddTask(task, checkOnScreen: false);
		}
	}
}
