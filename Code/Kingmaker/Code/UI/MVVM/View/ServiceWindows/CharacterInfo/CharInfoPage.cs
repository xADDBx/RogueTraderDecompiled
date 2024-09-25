using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public class CharInfoPage
{
	public List<CharInfoComponentType> ComponentsForAll;

	public List<CharInfoComponentType> ComponentsForMainCharacter;

	public List<CharInfoComponentType> ComponentsForCompanions;

	public List<CharInfoComponentType> ComponentsForPets;

	public List<CharInfoComponentType> GetComponentsListForUnitType(UnitType type)
	{
		List<CharInfoComponentType> list = ComponentsForAll.EmptyIfNull().ToList();
		switch (type)
		{
		case UnitType.MainCharacter:
			list.AddRange(ComponentsForMainCharacter.EmptyIfNull());
			break;
		case UnitType.Companion:
			list.AddRange(ComponentsForCompanions.EmptyIfNull());
			break;
		case UnitType.Pet:
			list.AddRange(ComponentsForPets.EmptyIfNull());
			break;
		}
		return list;
	}
}
