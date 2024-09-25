using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("2c2b7a22639a4908b623ba0a902e04c1")]
public class EtudeBracketLockCharacters : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	private BlueprintUnitReference[] m_Companions;

	protected override void OnEnter()
	{
		foreach (BaseUnitEntity item in from character in Game.Instance.Player.AllCharacters
			from companion in m_Companions
			where character.Blueprint == companion.Get()
			select character)
		{
			if (!CheckCharacter(item))
			{
				continue;
			}
			UnitPartCompanion optional = item.GetOptional<UnitPartCompanion>();
			if (optional == null || optional.State != CompanionState.InParty)
			{
				item.GetOptional<UnitPartCompanion>().SetState(CompanionState.InParty);
				EventBus.RaiseEvent((IBaseUnitEntity)item, (Action<IPartyHandler>)delegate(IPartyHandler h)
				{
					h.HandleAddCompanion();
				}, isCheckRuntime: true);
			}
			item.GetOrCreate<PartPartyLock>().Lock();
		}
	}

	protected override void OnExit()
	{
		foreach (BaseUnitEntity item in from character in Game.Instance.Player.AllCharacters
			from companion in m_Companions
			where character.Blueprint == companion.Get()
			select character)
		{
			if (CheckCharacter(item))
			{
				item.GetOrCreate<PartPartyLock>().Unlock();
			}
		}
	}

	private bool CheckCharacter(IBaseUnitEntity character)
	{
		Player player = Game.Instance.Player;
		if (!player.ActiveCompanions.Contains(character) && !player.RemoteCompanions.Contains(character))
		{
			return player.PartyAndPetsDetached.Contains(character);
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
