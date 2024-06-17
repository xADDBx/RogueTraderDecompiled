using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateClass : TooltipBaseTemplate
{
	private readonly string m_Name;

	private readonly string m_Desc;

	public TooltipTemplateClass(ClassData classData)
	{
		BlueprintCharacterClass characterClass = classData.CharacterClass;
		m_Name = characterClass.Name;
		m_Desc = characterClass.Description;
		BlueprintArchetype blueprintArchetype = classData.Archetypes.FirstOrDefault();
		if (blueprintArchetype != null)
		{
			m_Name = blueprintArchetype.Name;
			m_Desc = blueprintArchetype.Description;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(m_Desc, TooltipTextType.Paragraph);
	}
}
