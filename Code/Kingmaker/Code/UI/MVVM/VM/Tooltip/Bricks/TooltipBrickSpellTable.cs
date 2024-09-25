using Kingmaker.Code.UnitLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSpellTable : ITooltipBrick
{
	private readonly TooltipBrickSpellTableVM m_SpellTableVM;

	public TooltipBrickSpellTable(BlueprintSpellbook spellbook, int characterLevel)
	{
		m_SpellTableVM = new TooltipBrickSpellTableVM(spellbook, characterLevel);
	}

	public TooltipBrickSpellTable(Spellbook spellbook, bool isCharGen, BaseUnitEntity currentUnit)
	{
		m_SpellTableVM = new TooltipBrickSpellTableVM(spellbook, isCharGen, currentUnit);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return m_SpellTableVM;
	}
}
