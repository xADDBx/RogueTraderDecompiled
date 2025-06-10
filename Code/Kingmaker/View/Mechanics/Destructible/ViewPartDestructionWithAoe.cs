using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

public class ViewPartDestructionWithAoe : ViewBasedPart<DestructibleWithAoeSettings>, IDestructionStagesManager, IAreaLoadingStagesHandler, ISubscriber, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, IEventTag<IInGameHandler, EntitySubscriber>, IHashable
{
	public string name => ((AbstractEntityPartComponent)base.Source).Or(null)?.name ?? "<uninitialized-view-based-part>";

	public IEnumerable<DestructionStage> Stages
	{
		get
		{
			yield break;
		}
	}

	public void ChangeStage(DestructionStage stage, bool onLoad)
	{
		if (!onLoad)
		{
			base.Settings.AreaEffectView.Data.IsInGame = GetAOETargetState(stage, base.Owner.IsInGame);
		}
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		base.Settings.AreaEffectView.Data.IsInGame = GetAOETargetState(base.Owner.GetRequired<PartDestructionStagesManager>().Stage, base.Owner.IsInGame);
	}

	private bool GetAOETargetState(DestructionStage stage, bool isInGame)
	{
		if (isInGame)
		{
			return stage != DestructionStage.Destroyed;
		}
		return false;
	}

	public void HandleObjectInGameChanged()
	{
		base.Settings.AreaEffectView.Data.IsInGame = GetAOETargetState(base.Owner.GetRequired<PartDestructionStagesManager>().Stage, base.Owner.IsInGame);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
