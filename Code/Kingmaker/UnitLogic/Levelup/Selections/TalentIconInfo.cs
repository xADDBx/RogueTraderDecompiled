using System;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Base;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Serializable]
public class TalentIconInfo
{
	[EnumFlagsAsButtons]
	public TalentGroup AllGroups;

	[SingleEnumOptions(typeof(TalentGroup), "AllGroups")]
	public TalentGroup MainGroup;

	public bool HasGroups => AllGroups > (TalentGroup)0;
}
