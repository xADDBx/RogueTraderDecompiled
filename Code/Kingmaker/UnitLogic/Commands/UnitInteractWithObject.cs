using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.StarSystem;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Interaction;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Covers;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitInteractWithObject : UnitCommand<UnitInteractWithObjectParams>
{
	private InteractionPart m_Interaction;

	[JsonProperty]
	public Vector3? OverrideTarget { get; set; }

	public InteractionPart Interaction => m_Interaction ?? (m_Interaction = base.Params.Interaction);

	private MapObjectEntity TargetObject => Interaction.Owner;

	public override bool ShouldBeInterrupted => !Interaction.CanInteract();

	public override bool IsMoveUnit => false;

	public override bool IsBlockingCommand => true;

	public override bool IsUnitEnoughClose
	{
		get
		{
			if (!Interaction.IsEnoughCloseForInteraction(base.Executor))
			{
				return false;
			}
			if (base.NeedLoS)
			{
				return (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(base.Executor, base.ApproachPoint, default(IntRect)) != LosCalculations.CoverType.Invisible;
			}
			return true;
		}
	}

	public UnitInteractWithObject([NotNull] UnitInteractWithObjectParams @params)
		: base(@params)
	{
	}

	public static void ApproachAndInteract(BaseUnitEntity unit, InteractionPart interaction, IInteractionVariantActor variantActor = null)
	{
		if (unit == null || !interaction.HasEnoughActionPoints(unit))
		{
			return;
		}
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			PathfindingService.Instance.FindPathRT(unit.MovementAgent, interaction.Owner.Position, interaction.ApproachRadius, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else if (unit.IsMovementLockedByGameModeOrCombat())
				{
					PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
				}
				else
				{
					MoveCommandSettings moveCommandSettings = default(MoveCommandSettings);
					moveCommandSettings.Destination = interaction.Owner.Position;
					moveCommandSettings.DisableApproachRadius = true;
					MoveCommandSettings settings = moveCommandSettings;
					UnitMoveToParams unitMoveToParams = UnitHelper.CreateMoveCommandParamsRT(unit, settings, path);
					if (Game.Instance.CurrentMode == GameModeType.StarSystem)
					{
						float num = (unit.Blueprint as BlueprintStarship)?.SpeedOnStarSystemMap ?? 1f;
						unitMoveToParams.OverrideSpeed = num / StarSystemTimeController.TimeMultiplier;
					}
					unit.Commands.Run(unitMoveToParams);
					unit.Commands.AddToQueueFirst(new UnitInteractWithObjectParams(interaction, variantActor)
					{
						IsSynchronized = true
					});
				}
			});
		}
		else if (interaction.IsEnoughCloseForInteraction(unit))
		{
			unit.Commands.AddToQueueFirst(new UnitInteractWithObjectParams(interaction, variantActor)
			{
				IsSynchronized = true
			});
		}
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		UpdateTarget();
	}

	protected override Vector3 GetTargetPoint()
	{
		return OverrideTarget ?? TargetObject.View.ViewTransform.position;
	}

	private void UpdateTarget()
	{
		if (!OverrideTarget.HasValue && TargetObject is TrapObjectData trapObjectData && (bool)trapObjectData.View.Settings.ScriptZoneTrigger)
		{
			Vector3 normalized = (trapObjectData.View.ViewTransform.position - base.Executor.Position).normalized;
			Vector3 point = base.Executor.Position + normalized * (base.Executor.Corpulence + 0.25f);
			if (trapObjectData.View.Settings.ScriptZoneTrigger.Data.ContainsPosition(point))
			{
				OverrideTarget = base.Executor.Position + normalized * 0.01f;
			}
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		Interaction.PlayStartSound(base.Executor);
		if (base.Executor.IsInCombat)
		{
			base.Executor.CombatState.SpendActionPoints(Interaction.ActionPointsCost);
			EventBus.RaiseEvent(delegate(IUnitActionPointsHandler h)
			{
				h.HandleActionPointsSpent(base.Executor);
			});
		}
	}

	protected override void TriggerAnimation()
	{
		base.TriggerAnimation();
		if (Interaction.UseAnimationState != 0)
		{
			StartAnimation(UnitAnimationType.Interact, delegate(UnitAnimationActionHandle h)
			{
				h.InteractionType = Interaction.UseAnimationState;
			});
		}
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		if (Interaction?.Settings?.InteractionStopSound != null && Interaction.Settings.InteractionStopSound != "")
		{
			SoundEventsManager.PostEvent(Interaction.Settings.InteractionStopSound, Interaction.View.Or(null)?.gameObject);
		}
	}

	protected override ResultType OnAction()
	{
		if (Interaction.Settings.NotInCombat && base.Executor.IsInCombat)
		{
			return ResultType.Fail;
		}
		if (!Interaction.CanInteract())
		{
			return ResultType.Fail;
		}
		IInteractionVariantActor interactionVariantActor = base.Params.VariantActor as IInteractionVariantActor;
		if (interactionVariantActor == null && Interaction is IHasInteractionVariantActors { InteractThroughVariants: not false })
		{
			interactionVariantActor = Interaction.View.Data.Parts.GetAll<IInteractionVariantActor>().FirstOrDefault((IInteractionVariantActor x) => x is KeyRestrictionPart && x.CanInteract(base.Executor));
		}
		using (ContextData<InteractionVariantData>.Request().Setup(interactionVariantActor))
		{
			Interaction.Interact(base.Executor);
		}
		return ResultType.Success;
	}
}
