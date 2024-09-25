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
	private readonly bool m_ShowRemoteCompanionsAnyway;

	private bool RemoteCompanionsAvailable
	{
		get
		{
			if (!IsCapital && !Game.Instance.CurrentlyLoadedArea.IsShipArea)
			{
				return m_ShowRemoteCompanionsAnyway;
			}
			return true;
		}
	}

	private bool AreAllRequiredWithUs => !base.RemoteCharacter.Any((GroupChangerCharacterVM v) => v.IsLock.Value);

	public GroupChangerCommonVM(Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital = false, bool sameFinishActions = false, bool canCancel = true, bool showRemoteCompanions = false)
		: base(go, close, lastUnits, requiredUnits, isCapital, sameFinishActions, canCancel)
	{
		m_ShowRemoteCompanionsAnyway = showRemoteCompanions;
		List<GroupChangerCharacterVM> list = (from u in LastUnits.Concat(Game.Instance.Player.PartyCharacters).Concat(Game.Instance.Player.RemoteCompanions.Where(ShouldShowRemote).Select(UnitReference.FromIAbstractUnitEntity)).Distinct()
			orderby u == Game.Instance.Player.MainCharacterEntity descending
			select new GroupChangerCharacterVM(u, MustBeInParty((BaseUnitEntity)u.ToIBaseUnitEntity()))).ToList();
		List<GroupChangerCharacterVM> list2 = list.Where((GroupChangerCharacterVM u) => MustBeInParty((BaseUnitEntity)u.UnitRef.ToIBaseUnitEntity())).ToList();
		List<GroupChangerCharacterVM> list3 = list.Except(list2).ToList();
		int num = 0;
		foreach (GroupChangerCharacterVM characterVm in list2)
		{
			GroupChangerCharacterVM groupChangerCharacterVM = list.FirstOrDefault((GroupChangerCharacterVM c) => c == characterVm);
			if (groupChangerCharacterVM != null)
			{
				groupChangerCharacterVM.SetIsInParty(num < 6);
				num++;
			}
		}
		foreach (GroupChangerCharacterVM characterVm in list3)
		{
			GroupChangerCharacterVM groupChangerCharacterVM2 = list.FirstOrDefault((GroupChangerCharacterVM c) => c == characterVm);
			if (groupChangerCharacterVM2 != null)
			{
				groupChangerCharacterVM2.SetIsInParty(num < 6);
				num++;
			}
		}
		foreach (GroupChangerCharacterVM item in list)
		{
			if (item.IsInParty.Value)
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
		}
	}

	private bool MustBeInParty(BaseUnitEntity character)
	{
		if (character != Game.Instance.Player.MainCharacterEntity && character.Blueprint.GetComponent<LockedCompanionComponent>() == null && !RequiredUnits.Contains(character.Blueprint))
		{
			return PartPartyLock.IsLocked(character);
		}
		return true;
	}

	private bool ShouldShowRemote(BaseUnitEntity c)
	{
		if (c.IsInGame || RemoteCompanionsAvailable || UnitIsRequired(c))
		{
			return !c.IsPet;
		}
		return false;
	}

	private bool UnitIsRequired(BaseUnitEntity c)
	{
		return RequiredUnits.Contains(c.Blueprint);
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
