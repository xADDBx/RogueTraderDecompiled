using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitLootUnit : UnitCommand
{
	public new MechanicEntity Target => base.Target?.Entity;

	public override bool IsMoveUnit => false;

	public UnitLootUnit([NotNull] UnitLootUnitParams @params)
		: base(@params)
	{
	}

	protected override ResultType OnAction()
	{
		EntityViewBase[] objectsWithLoot = MassLootHelper.GetObjectsWithLoot(Target.View).ToArray();
		EventBus.RaiseEvent((IBaseUnitEntity)base.Executor, (Action<ILootInteractionHandler>)delegate(ILootInteractionHandler l)
		{
			l.HandleLootInteraction(objectsWithLoot, LootContainerType.Unit, null);
		}, isCheckRuntime: true);
		EntityViewBase[] array = objectsWithLoot;
		foreach (EntityViewBase entityViewBase in array)
		{
			if (entityViewBase.InteractionComponent is InteractionLootPart interactionLootPart)
			{
				interactionLootPart.IsViewed = true;
			}
			else if (entityViewBase.Data is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.MarkLootViewed();
			}
		}
		return ResultType.Success;
	}

	protected override Vector3 GetTargetPoint()
	{
		return ((UnitLootUnitParams)base.Params).OverrideApproachPoint ?? base.GetTargetPoint();
	}
}
