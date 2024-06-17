using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using StateHasher.Core;
using UniRx;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("31d6da0fb0a880f4d85844cea65b02b0")]
public class DropLootAndDestroyOnDeactivate : UnitBuffComponentDelegate, IHashable
{
	private IDisposable m_Coroutine;

	protected override void OnActivate()
	{
		Buff buff = base.Buff;
		m_Coroutine = Observable.Timer(base.Buff.ExpirationInRounds.Rounds().Seconds).Subscribe(delegate
		{
			buff.Remove();
		});
		if (!base.Owner.Faction.IsPlayer)
		{
			base.Owner.Inventory.DropLootToGround(dismember: true);
		}
	}

	protected override void OnDeactivate()
	{
		m_Coroutine?.Dispose();
		m_Coroutine = null;
		if ((bool)base.Owner.GetOptional<UnitPartSummonedMonster>())
		{
			Game.Instance.EntityDestroyer.Destroy(base.Owner);
		}
		else if (!base.Owner.Faction.IsPlayer)
		{
			base.Owner.IsInGame = false;
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
