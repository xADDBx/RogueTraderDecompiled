using System.Collections.Generic;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using TMPro;

namespace Kingmaker.Blueprints.Encyclopedia.Blocks;

public class BlueprintEncyclopediaBlockText : BlueprintEncyclopediaBlock, IBlockText, IBlockLink
{
	public LocalizedString Text;

	public bool HasConsoleText;

	[ShowIf("HasConsoleText")]
	public LocalizedString ConsoleText;

	public TextAlignmentOptions TextAlignment;

	public List<BlueprintEncyclopediaPageReference> m_Resource = new List<BlueprintEncyclopediaPageReference>();

	public BlueprintEncyclopediaPage GetResource(string key)
	{
		return ChapterList.GetPage(key) ?? ((BlueprintEncyclopediaPage)m_Resource.Find((BlueprintEncyclopediaPageReference x) => x.Get().name == key));
	}

	public string GetText()
	{
		return (!Game.Instance.IsControllerMouse && HasConsoleText && !string.IsNullOrWhiteSpace(ConsoleText)) ? ConsoleText : Text;
	}

	public override string ToString()
	{
		return "Text";
	}
}
