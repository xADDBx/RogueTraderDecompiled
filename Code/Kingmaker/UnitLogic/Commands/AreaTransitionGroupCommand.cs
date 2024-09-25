using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Commands;

public sealed class AreaTransitionGroupCommand : GroupCommand
{
	private const int MaxDistanceFromAreaTransitionToIncludeAllParty = 36;

	[NotNull]
	public Entity TransitionEntity => Target.Entity;

	[NotNull]
	public AreaTransitionPart TransitionPart => Target.Entity.GetRequired<AreaTransitionPart>();

	public override bool CanStart
	{
		get
		{
			if (!base.CanStart)
			{
				if (Units.All((BaseUnitEntity u) => GeometryUtils.SqrMechanicsDistance(u.Position, TransitionPart.Owner.Position) <= 36f))
				{
					return Units.All((BaseUnitEntity u) => u.Commands.GroupCommand != null);
				}
				return false;
			}
			return true;
		}
	}

	public override bool ReadyToAct
	{
		get
		{
			if (Units.Any((BaseUnitEntity u) => GeometryUtils.MechanicsDistance(u.Position, TransitionPart.View.ViewTransform.position) <= TransitionPart.ProximityDistance))
			{
				return Units.All((BaseUnitEntity u) => u.Commands.GroupCommand != null);
			}
			return false;
		}
	}

	public AreaTransitionGroupCommand(Guid groupGuid, [NotNull] IEnumerable<EntityRef<BaseUnitEntity>> units, [NotNull] AreaTransitionPart areaTransition)
		: base(groupGuid, units, (MechanicEntity)areaTransition.Owner)
	{
	}

	protected override AbstractUnitCommand.ResultType OnAction()
	{
		if (MassLootHelper.CanLootZone() && TransitionPart.AreaEnterPoint?.Area != Game.Instance.CurrentlyLoadedArea && !TransitionPart.Settings.SuppressLoot)
		{
			EventBus.RaiseEvent(Game.Instance.Player.MainCharacter.ToIBaseUnitEntity(), delegate(ILootInteractionHandler e)
			{
				e.HandleZoneLootInteraction(TransitionPart);
			});
		}
		else
		{
			ExecuteTransition(TransitionPart, isPlayerCommand: false, (Units.Length != 0) ? Units[0] : null);
		}
		return AbstractUnitCommand.ResultType.Success;
	}

	public static void ExecuteTransition([NotNull] AreaTransitionPart areaTransition, bool isPlayerCommand, BaseUnitEntity executorEntity)
	{
		Game.Instance.GameCommandQueue.AreaTransition(areaTransition, isPlayerCommand, executorEntity);
	}
}
