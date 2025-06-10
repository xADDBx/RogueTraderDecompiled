using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateGlossary : TooltipBaseTemplate
{
	public readonly BlueprintEncyclopediaGlossaryEntry GlossaryEntry;

	private readonly bool m_IsHistory;

	private readonly bool m_IsEncyclopedia;

	public TooltipTemplateGlossary(string key, bool isHistory = false, bool isEncyclopedia = false)
	{
		GlossaryEntry = UIUtility.GetGlossaryEntry(key);
		m_IsHistory = isHistory;
		m_IsEncyclopedia = isEncyclopedia;
	}

	public TooltipTemplateGlossary(BlueprintEncyclopediaGlossaryEntry glossaryEntry, bool isHistory = false, bool isEncyclopedia = false)
	{
		GlossaryEntry = glossaryEntry;
		m_IsHistory = isHistory;
		m_IsEncyclopedia = isEncyclopedia;
	}

	public TooltipTemplateGlossary(IEnumerable<string> keys, bool isHistory = false, bool isEncyclopedia = false)
	{
		foreach (string key in keys)
		{
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtility.GetGlossaryEntry(key);
			if (glossaryEntry != null)
			{
				GlossaryEntry = glossaryEntry;
				break;
			}
		}
		m_IsHistory = isHistory;
		m_IsEncyclopedia = isEncyclopedia;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Info && m_IsHistory)
		{
			yield return new TooltipBrickHistoryManagement(GlossaryEntry);
		}
		else
		{
			yield return new TooltipBrickTitle(GlossaryEntry?.Title);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (!string.IsNullOrEmpty(GlossaryEntry?.GetDescription()))
		{
			TooltipTextType flags = TooltipTextType.Paragraph | TooltipTextType.BlackColor;
			if ((type != TooltipTemplateType.Info || !m_IsHistory) && m_IsEncyclopedia)
			{
				flags |= TooltipTextType.GlossarySize;
			}
			string description = UIUtilityTexts.UpdateDescriptionWithUIProperties(GlossaryEntry?.GetDescription(), null);
			yield return new TooltipBrickSeparator();
			yield return new TooltipBrickText(description, flags);
		}
		yield return null;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		RootUIContext rootUiContext = Game.Instance.RootUiContext;
		FullScreenUIType fullScreenUIType = rootUiContext.FullScreenUIType;
		bool flag = fullScreenUIType == FullScreenUIType.Settings || fullScreenUIType == FullScreenUIType.NewGame || fullScreenUIType == FullScreenUIType.DlcModManager || fullScreenUIType == FullScreenUIType.FirstLaunchSettings;
		bool flag2 = rootUiContext.FullScreenUIType == FullScreenUIType.Chargen;
		if (type == TooltipTemplateType.Info && m_IsHistory && !flag && !flag2 && !GlossaryEntry.HideInEncyclopedia)
		{
			yield return new TooltipBrickButton(EncyclopediaCallback, UIStrings.Instance.EncyclopediaTexts.EncyclopediaGlossaryButton);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHint(TooltipTemplateType type)
	{
		if (m_IsEncyclopedia)
		{
			yield return new TooltipBrickHint(UIStrings.Instance.EncyclopediaTexts.TooltipOpenEncyclopedia);
		}
	}

	public void EncyclopediaCallback()
	{
		TutorialVM obj = Game.Instance.RootUiContext.CommonVM?.TutorialVM;
		obj?.BigWindowVM?.Value?.Hide();
		obj?.SmallWindowVM?.Value?.Hide();
		EventBus.RaiseEvent(delegate(IEncyclopediaGlossaryModeHandler h)
		{
			h.HandleGlossaryMode(state: false);
		});
		TooltipHelper.CloseGlossaryInfoWindow();
		TooltipHelper.HideInfo();
		UIUtility.EntityLinkActions.ShowEncyclopediaPage(GlossaryEntry.Key);
	}
}
