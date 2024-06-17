using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Encyclopedia.Blocks;

namespace Kingmaker.UI.Common;

public static class UIUtilityCreateEncyclopediaTooltipDescription
{
	public static string CreateSettingsTooltipDescription(BlueprintEncyclopediaPage blueprintPage)
	{
		string text = "";
		if (blueprintPage.GlossaryEntry != null)
		{
			string text2 = (blueprintPage.GlossaryEntry as BlueprintEncyclopediaGlossaryEntry)?.GetDescription();
			if (!string.IsNullOrWhiteSpace(text2))
			{
				text = text + text2 + "<br><br>";
			}
		}
		foreach (IBlock block in blueprintPage.GetBlocks())
		{
			if (block is GlossaryEntryBlock glossaryEntryBlock)
			{
				if (!string.IsNullOrWhiteSpace(glossaryEntryBlock.Entry.GetDescription()))
				{
					text = string.Concat(text, glossaryEntryBlock.Entry.GetDescription(), "<br><br>");
				}
			}
			else if (block is BlueprintEncyclopediaBlockText blueprintEncyclopediaBlockText && !string.IsNullOrWhiteSpace(blueprintEncyclopediaBlockText.GetText()))
			{
				text = text + blueprintEncyclopediaBlockText.GetText() + "<br><br>";
			}
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			PFLog.System.Log("Description as Encyclopedia description is empty");
		}
		return text;
	}
}
