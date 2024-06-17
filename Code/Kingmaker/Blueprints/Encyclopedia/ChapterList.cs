using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Blueprints.Encyclopedia;

public class ChapterList : ScriptableObject, IEnumerable<BlueprintEncyclopediaChapter>, IEnumerable
{
	[SerializeField]
	protected List<BlueprintEncyclopediaChapterReference> m_List = new List<BlueprintEncyclopediaChapterReference>();

	private static Dictionary<string, BlueprintEncyclopediaPage> m_AllPages;

	public BlueprintEncyclopediaChapter this[int i]
	{
		get
		{
			return m_List[i].Get();
		}
		set
		{
			m_List[i] = value.ToReference<BlueprintEncyclopediaChapterReference>();
		}
	}

	public IEnumerator<BlueprintEncyclopediaChapter> GetEnumerator()
	{
		return m_List.Select((BlueprintEncyclopediaChapterReference r) => r.Get()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_List.GetEnumerator();
	}

	public static BlueprintEncyclopediaPage GetPage(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		UIConfig.Instance.ChapterList.RefreshChapters();
		if (m_AllPages.ContainsKey(key))
		{
			return m_AllPages[key];
		}
		string text = m_AllPages?.FirstOrDefault((KeyValuePair<string, BlueprintEncyclopediaPage> p) => p.Value?.GlossaryEntry?.ToString() == key).Key;
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		if (!m_AllPages.ContainsKey(text))
		{
			return null;
		}
		return m_AllPages[text];
	}

	public void RefreshChapters()
	{
		Initialize();
		InitializeAdditionalChapter(UIConfig.Instance.BookEventsChapter.Get());
		InitializeAdditionalChapter(UIConfig.Instance.AstropathBriefsChapter.Get());
	}

	public void Initialize()
	{
		if (m_AllPages != null)
		{
			return;
		}
		m_AllPages = new Dictionary<string, BlueprintEncyclopediaPage>();
		foreach (BlueprintEncyclopediaChapterReference item in m_List)
		{
			PrepareNode(item.Get());
		}
	}

	private void InitializeAdditionalChapter(BlueprintEncyclopediaChapter chapter)
	{
		if (chapter != null && chapter.ChildPages.Dereference().Any((BlueprintEncyclopediaPage page) => page is IEncyclopediaPageWithAvailability encyclopediaPageWithAvailability && encyclopediaPageWithAvailability.IsAvailable))
		{
			PrepareNode(chapter);
		}
	}

	private static void PrepareNode(BlueprintEncyclopediaNode node)
	{
		if (node == null)
		{
			return;
		}
		if (!m_AllPages.ContainsKey(node.name) && node is BlueprintEncyclopediaPage blueprintEncyclopediaPage)
		{
			m_AllPages.Add(blueprintEncyclopediaPage.name, blueprintEncyclopediaPage);
		}
		foreach (BlueprintEncyclopediaPageReference childPage in node.ChildPages)
		{
			if (childPage != null)
			{
				BlueprintEncyclopediaPage blueprintEncyclopediaPage2 = childPage.Get();
				if (blueprintEncyclopediaPage2 == null)
				{
					UberDebug.LogError("Error: BlueprintEncyclopediaNode [" + node.name + "] has empty links, please delete them");
					continue;
				}
				blueprintEncyclopediaPage2.ParentAsset = node;
				PrepareNode(childPage.Get());
			}
		}
	}
}
