using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateEncumbranceCharacter : TooltipTemplateEncumbrance
{
	private readonly BaseUnitEntity m_Unit;

	private readonly string m_Name;

	public TooltipTemplateEncumbranceCharacter(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			m_Unit = unit;
			m_Name = unit.CharacterName;
			Capacity = EncumbranceHelper.GetCarryingCapacity(unit);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_Name != null)
		{
			string title = string.Format(UIStrings.Instance.Tooltips.PersonalEncumbrance, m_Name);
			yield return new TooltipBrickTitle(title);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Unit == null)
		{
			return list;
		}
		list.Add(new TooltipBrickEncumbrance(Capacity));
		AddPenalty(list);
		list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
		list.Add(new TooltipBrickText(UIUtility.GetGlossaryEntryName("CEInfo")));
		return list;
	}

	protected override void AddPenalty(List<ITooltipBrick> bricks)
	{
		Encumbrance encumbrance = Capacity.GetEncumbrance();
		if (encumbrance != 0)
		{
			TooltipTemplateEncumbrance.AddTitle(bricks, UIUtility.GetGlossaryEntryName("PersonalPenalty"));
			TooltipTemplateEncumbrance.AddIconValueSmall(bricks, UIUtility.GetGlossaryEntryName("CEArmorCheckPenalty"), UnitPartEncumbrance.GetArmorCheckPenalty(m_Unit, encumbrance).ToString());
			TooltipTemplateEncumbrance.AddIconValueSmall(bricks, UIUtility.GetGlossaryEntryName("CEMaxDexterityBonus"), UnitPartEncumbrance.GetArmorCheckPenalty(m_Unit, encumbrance).ToString());
		}
	}
}
