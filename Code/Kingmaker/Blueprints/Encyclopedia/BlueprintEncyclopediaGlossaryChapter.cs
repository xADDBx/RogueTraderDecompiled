using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("fdfbacd144d74286bf268f2a522d64d9")]
public class BlueprintEncyclopediaGlossaryChapter : BlueprintEncyclopediaChapter
{
	private List<IPage> m_ChildLetterIndexPages;

	private Dictionary<string, GlossaryLetterIndexPage> m_IndexPages;

	private bool m_IsInitialized;

	private void Initialize()
	{
		if (m_IsInitialized)
		{
			return;
		}
		m_IndexPages = new Dictionary<string, GlossaryLetterIndexPage>();
		foreach (BlueprintEncyclopediaGlossaryEntry item in ChildPages.Dereference())
		{
			if (item == null)
			{
				UberDebug.LogError("Empty or wrong type in " + name + " chapter!");
			}
			else
			{
				if (item.HideInEncyclopedia)
				{
					continue;
				}
				string title = item.GetTitle();
				if (string.IsNullOrEmpty(title))
				{
					UberDebug.LogError(item.name + " has no title!");
					continue;
				}
				string text = title.Substring(0, 1);
				if (m_IndexPages.TryGetValue(text, out var value))
				{
					value.AddChild(item);
					continue;
				}
				GlossaryLetterIndexPage glossaryLetterIndexPage = new GlossaryLetterIndexPage(this, text);
				glossaryLetterIndexPage.AddChild(item);
				m_IndexPages[text] = glossaryLetterIndexPage;
			}
		}
		m_ChildLetterIndexPages = new List<IPage>(m_IndexPages.Values.OrderBy((GlossaryLetterIndexPage page) => page.SortIndex).ToList());
	}

	public override List<IPage> GetChilds()
	{
		Initialize();
		return m_ChildLetterIndexPages;
	}

	public IPage GetLetterIndexPage(string key)
	{
		Initialize();
		return m_IndexPages[key];
	}
}
