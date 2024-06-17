using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UnitLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSpellTableVM : TooltipBaseBrickVM
{
	public List<string> Values = new List<string>();

	public TooltipBrickSpellTableVM(BlueprintSpellbook spellbook, int characterLevel)
	{
		List<int> spellNumberTable = UIUtilityUnit.GetSpellNumberTable(spellbook, characterLevel);
		if (spellbook.SpellList.SpellsByLevel[0].Spells.Any())
		{
			Values.Add("<size=150%><sprite name=Infinity></size>");
		}
		else
		{
			Values.Add(null);
		}
		for (int i = 1; i <= spellbook.MaxSpellLevel; i++)
		{
			Values.Add(spellNumberTable[i].ToString());
		}
	}

	public TooltipBrickSpellTableVM(Spellbook spellbook, bool ignoreScores, BaseUnitEntity currentUnit)
	{
		List<int> list = (ignoreScores ? UIUtilityUnit.GetSpellNumberBaseTable(spellbook) : UIUtilityUnit.GetSpellNumberTable(spellbook));
		if (currentUnit.Spellbooks.FirstOrDefault((Spellbook cl) => cl.Blueprint == spellbook.Blueprint) != null)
		{
			if (spellbook.Blueprint.SpellList.SpellsByLevel[0].Spells.Any())
			{
				Values.Add("<size=150%><sprite name=Infinity></size>");
			}
			else
			{
				Values.Add(null);
			}
		}
		for (int i = 1; i <= spellbook.LastSpellbookLevel; i++)
		{
			if (i < spellbook.FirstSpellbookLevel)
			{
				Values.Add(null);
			}
			else
			{
				Values.Add(list[i].ToString());
			}
		}
	}
}
