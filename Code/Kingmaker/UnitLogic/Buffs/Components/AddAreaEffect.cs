using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[TypeId("25b073dd90738ed46939db4777aafe17")]
public class AddAreaEffect : UnitFactComponentDelegate, IAreaHandler, ISubscriber, IUnitSpawnHandler<EntitySubscriber>, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IEventTag<IUnitSpawnHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_AreaEffect;

	public BlueprintAbilityAreaEffect AreaEffect => m_AreaEffect?.Get();

	protected override void OnActivate()
	{
		SpawnAreaEffect();
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.RemoveAndEnd(base.Fact, this);
	}

	public void OnAreaBeginUnloading()
	{
		base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.RemoveAndEnd(base.Fact, this);
	}

	public void OnAreaDidLoad()
	{
		if (!base.Owner.IsInState)
		{
			PFLog.Default.Error($"Area effect from wrong unit: {AreaEffect.NameSafe()} on {base.Owner}");
		}
		else
		{
			SpawnAreaEffect();
		}
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		SpawnAreaEffect();
	}

	private void SpawnAreaEffect()
	{
		if (base.Owner.IsInGame && Game.Instance.CurrentlyLoadedArea != null && base.Owner.IsInState && base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.Get(base.Fact, this) == null && !ContextData<UnitHelper.PreviewUnit>.Current && !base.Owner.IsPreviewUnit)
		{
			AreaEffectEntity areaEffectEntity = AreaEffectsController.SpawnAttachedToTarget(base.Fact.MaybeContext, AreaEffect, base.Owner, null);
			areaEffectEntity.SourceFact = base.Fact;
			base.Owner.GetOrCreate<UnitPartSpawnedAreaEffects>().Add(base.Fact, this, areaEffectEntity);
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (!base.Owner.IsInGame)
		{
			base.Owner.GetOptional<UnitPartSpawnedAreaEffects>()?.RemoveAndEnd(base.Fact, this);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
