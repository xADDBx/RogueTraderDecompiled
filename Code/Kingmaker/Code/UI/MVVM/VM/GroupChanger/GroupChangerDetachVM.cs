using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Code.UI.MVVM.VM.GroupChanger;

public class GroupChangerDetachVM : GroupChangerVM
{
	public GroupChangerDetachVM(Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital = false)
		: base(go, close, lastUnits, requiredUnits, isCapital)
	{
		Player player = Game.Instance.Player;
		foreach (UnitReference partyCharacter in player.PartyCharacters)
		{
			m_PartyCharacter.Add(new GroupChangerCharacterVM(partyCharacter, isLock: false));
		}
		foreach (BaseUnitEntity item in player.PartyAndPetsDetached)
		{
			UnitReference unit = UnitReference.FromIAbstractUnitEntity(item);
			m_RemoteCharacter.Add(new GroupChangerCharacterVM(unit, isLock: false));
		}
		m_CloseEnabled.Value = false;
	}
}
