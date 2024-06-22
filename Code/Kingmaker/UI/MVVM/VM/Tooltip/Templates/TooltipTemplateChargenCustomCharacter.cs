using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.CharGen;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateChargenCustomCharacter : TooltipBaseTemplate
{
	private readonly CharGenConfig.CharGenMode m_Mode;

	private readonly CharGenConfig.CharGenCompanionType m_CompanionType;

	public TooltipTemplateChargenCustomCharacter(CharGenConfig.CharGenMode mode, CharGenConfig.CharGenCompanionType companionType)
	{
		m_Mode = mode;
		m_CompanionType = companionType;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Mode == CharGenConfig.CharGenMode.NewCompanion)
		{
			LocalizedString localizedString = ((m_CompanionType == CharGenConfig.CharGenCompanionType.Navigator) ? UIStrings.Instance.CharGen.CreateNewNavigator : UIStrings.Instance.CharGen.CreateNewCompanion);
			list.Add(new TooltipBrickTitle(localizedString));
		}
		else
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.NewGameWin.CreateNewCharacter));
		}
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Mode == CharGenConfig.CharGenMode.NewCompanion)
		{
			LocalizedString localizedString = ((m_CompanionType == CharGenConfig.CharGenCompanionType.Navigator) ? UIStrings.Instance.CharGen.CreateNewNavigatorDescription : UIStrings.Instance.CharGen.CreateNewCompanionDescription);
			list.Add(new TooltipBrickText(localizedString));
		}
		else
		{
			list.Add(new TooltipBrickText(UIStrings.Instance.NewGameWin.CreateNewCharacterDescription));
		}
		return list;
	}
}
