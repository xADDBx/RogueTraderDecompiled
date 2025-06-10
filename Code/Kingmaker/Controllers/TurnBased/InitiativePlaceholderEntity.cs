using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class InitiativePlaceholderEntity : MechanicEntity, IPartyCombatHandler, ISubscriber, IUnitDeathHandler, ITurnEndHandler, ISubscriber<IMechanicEntity>, IHashable
{
	private static readonly List<InitiativePlaceholderEntity> AllList = new List<InitiativePlaceholderEntity>();

	[JsonProperty]
	public readonly MechanicEntity Delegate;

	[JsonProperty]
	public MechanicEntity CorrespondingEnemy;

	[JsonProperty]
	public readonly int Index;

	[JsonProperty]
	public bool MarkedForDisposeOnTurnEnd;

	public override bool NeedsView => false;

	public override bool IsInCombat => Delegate.IsInCombat;

	public static ReadonlyList<InitiativePlaceholderEntity> All => AllList;

	[NotNull]
	public static InitiativePlaceholderEntity Ensure(MechanicEntity entity, int index)
	{
		return All.FirstItem((InitiativePlaceholderEntity i) => i.Delegate == entity && i.Index == index) ?? Entity.Initialize(new InitiativePlaceholderEntity(entity, index));
	}

	[JsonConstructor]
	private InitiativePlaceholderEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public InitiativePlaceholderEntity(MechanicEntity entity, int index)
		: base(ConstructUniqueId(entity, index), entity.IsInGame, entity.Blueprint)
	{
		Delegate = entity;
		Index = index;
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		AllList.Add(this);
		base.Initiative.WasPreparedForRound = Game.Instance.TurnController.CombatRound - 1;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		AllList.Remove(this);
	}

	private static string ConstructUniqueId(MechanicEntity entity, int index)
	{
		return entity.UniqueId + "_ip" + index;
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return null;
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			Dispose();
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity == CorrespondingEnemy)
		{
			MarkedForDisposeOnTurnEnd = true;
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (MarkedForDisposeOnTurnEnd)
		{
			Dispose();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicEntity>.GetHash128(Delegate);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<MechanicEntity>.GetHash128(CorrespondingEnemy);
		result.Append(ref val3);
		int val4 = Index;
		result.Append(ref val4);
		result.Append(ref MarkedForDisposeOnTurnEnd);
		return result;
	}
}
