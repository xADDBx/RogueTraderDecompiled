using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

public class ViewPartDestructionStagesActions : ViewBasedPart<DestructionStagesActionsSettings>, IDestructionStagesManager, IHashable
{
	[CanBeNull]
	public ActionsHolder OnBecameDamaged => base.Settings?.OnBecameDamaged;

	[CanBeNull]
	public ActionsHolder OnBecameDestroyed => base.Settings?.OnBecameDestroyed;

	public string name => ((AbstractEntityPartComponent)base.Source).Or(null)?.name ?? "<uninitialized-view-based-part>";

	public IEnumerable<DestructionStage> Stages
	{
		get
		{
			ActionsHolder onBecameDamaged = OnBecameDamaged;
			if (onBecameDamaged != null && onBecameDamaged.HasActions)
			{
				yield return DestructionStage.Damaged;
			}
			onBecameDamaged = OnBecameDestroyed;
			if (onBecameDamaged != null && onBecameDamaged.HasActions)
			{
				yield return DestructionStage.Destroyed;
			}
		}
	}

	public void ChangeStage(DestructionStage stage, bool onLoad)
	{
		if (onLoad)
		{
			return;
		}
		ActionsHolder actions = GetActions(stage);
		if (actions == null || !actions.HasActions)
		{
			return;
		}
		MechanicEntity mechanicEntity = base.Owner as MechanicEntity;
		MechanicsContext mechanicsContext = (base.Owner as MechanicEntity)?.MainFact.MaybeContext;
		if (mechanicsContext == null && mechanicEntity != null)
		{
			mechanicsContext = new MechanicsContext(mechanicEntity, mechanicEntity, mechanicEntity.Blueprint);
		}
		using ((mechanicEntity != null) ? ContextData<MechanicEntityData>.Request().Setup(mechanicEntity) : null)
		{
			using (mechanicsContext?.GetDataScope())
			{
				actions.Run();
			}
		}
	}

	[CanBeNull]
	private ActionsHolder GetActions(DestructionStage stage)
	{
		return stage switch
		{
			DestructionStage.Whole => null, 
			DestructionStage.Damaged => OnBecameDamaged, 
			DestructionStage.Destroyed => OnBecameDestroyed, 
			_ => throw new ArgumentOutOfRangeException("stage", stage, null), 
		};
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
