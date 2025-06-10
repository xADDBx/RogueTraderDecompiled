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
				string firstVisibleLetter = GetFirstVisibleLetter(title);
				if (m_IndexPages.TryGetValue(firstVisibleLetter, out var value))
				{
					value.AddChild(item);
					continue;
				}
				GlossaryLetterIndexPage glossaryLetterIndexPage = new GlossaryLetterIndexPage(this, firstVisibleLetter);
				glossaryLetterIndexPage.AddChild(item);
				m_IndexPages[firstVisibleLetter] = glossaryLetterIndexPage;
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

	private string GetFirstVisibleLetter(string text)
	{
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '<':
				flag = true;
				continue;
			case '>':
				flag = false;
				continue;
			}
			if (!flag && !char.IsWhiteSpace(c))
			{
				return c.ToString();
			}
		}
		return string.Empty;
	}
}
