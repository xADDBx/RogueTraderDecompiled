using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.GroupChanger;

public class GroupChangerCommonVM : GroupChangerVM
{
	private bool RemoteCompanionsAvailable
	{
		get
		{
			if (!IsCapital)
			{
				return Game.Instance.CurrentlyLoadedArea.IsShipArea;
			}
			return true;
		}
	}

	private bool AreAllRequiredWithUs => !base.RemoteCharacter.Any((GroupChangerCharacterVM v) => v.IsLock.Value);

	public GroupChangerCommonVM(Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital = false)
		: base(go, close, lastUnits, requiredUnits, isCapital)
	{
		IEnumerable<GroupChangerCharacterVM> enumerable = from v in m_LastUnits.Concat(Game.Instance.Player.PartyCharacters).Concat(from v in Game.Instance.Player.RemoteCompanions.Where(ShouldShowRemote)
				select UnitReference.FromIAbstractUnitEntity(v)).Distinct()
			orderby MustBeInParty((BaseUnitEntity)v.ToIBaseUnitEntity()) descending
			select v into u
			select new GroupChangerCharacterVM(u, MustBeInParty((BaseUnitEntity)u.ToIBaseUnitEntity()));
		int num = 0;
		foreach (GroupChangerCharacterVM item in enumerable)
		{
			item.SetIsInParty(num < 6);
			if (num < 6)
			{
				m_PartyCharacter.Add(item);
			}
			else
			{
				m_RemoteCharacter.Add(item);
			}
			AddDisposable(item.Click.Subscribe(delegate(GroupChangerCharacterVM value)
			{
				MoveCharacter(value.UnitRef);
			}));
			num++;
		}
	}

	private bool MustBeInParty(BaseUnitEntity character)
	{
		if (character != Game.Instance.Player.MainCharacterEntity && character.Blueprint.GetComponent<LockedCompanionComponent>() == null && !m_RequiredUnits.Contains(character.Blueprint))
		{
			return PartPartyLock.IsLocked(character);
		}
		return true;
	}

	private bool ShouldShowRemote(BaseUnitEntity c)
	{
		if (c.IsInGame || RemoteCompanionsAvailable)
		{
			return !c.IsPet;
		}
		return false;
	}

	public override bool CloseCondition()
	{
		if (!IsCapital)
		{
			return AreAllRequiredWithUs;
		}
		return true;
	}
}
