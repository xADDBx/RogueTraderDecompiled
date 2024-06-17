using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("49dabfbcf10e464f992d7bdbcc314fdf")]
public class BlueprintEncyclopediaGlossaryEntry : BlueprintEncyclopediaPage
{
	public bool HideInEncyclopedia;

	public LocalizedString Description;

	public bool HasConsoleDescription;

	[ShowIf("HasConsoleDescription")]
	public LocalizedString ConsoleDescription;

	private List<IBlock> m_Blocks;

	public string Key => name;

	public override List<IBlock> GetBlocks()
	{
		List<IBlock> list = m_Blocks;
		if (list == null)
		{
			List<IBlock> obj = new List<IBlock>
			{
				new GlossaryEntryBlock(this)
			};
			List<IBlock> list2 = obj;
			m_Blocks = obj;
			list = list2;
		}
		return list;
	}

	public LocalizedString GetDescription()
	{
		if (Game.Instance.IsControllerMouse || !HasConsoleDescription || string.IsNullOrWhiteSpace(ConsoleDescription))
		{
			return Description;
		}
		return ConsoleDescription;
	}
}
