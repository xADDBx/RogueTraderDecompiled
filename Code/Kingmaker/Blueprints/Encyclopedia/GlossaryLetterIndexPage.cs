using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Blueprints.Encyclopedia;

public class GlossaryLetterIndexPage : IPage, INode
{
	private readonly string m_Letter;

	private readonly List<IPage> m_Childs = new List<IPage>();

	private readonly SortedList<string, IBlock> m_Blocks = new SortedList<string, IBlock>();

	public BlueprintEncyclopediaNode Parent { get; }

	public string SortIndex => m_Letter;

	public bool FirstExpanded => false;

	public GlossaryLetterIndexPage(BlueprintEncyclopediaNode parent, string letter)
	{
		Parent = parent;
		m_Letter = letter;
	}

	public void AddChild(BlueprintEncyclopediaGlossaryEntry child)
	{
		m_Childs.Add(child);
		string title = child.GetTitle();
		if (string.IsNullOrEmpty(title))
		{
			UberDebug.LogError("Empty title in " + child.name + " glossary entry!");
		}
		else if (m_Blocks.ContainsKey(title))
		{
			UberDebug.LogError("Duplicated glossary entry: " + child.name);
		}
		else
		{
			m_Blocks[title] = new GlossaryEntryBlock(child);
		}
	}

	public string GetTitle()
	{
		return m_Letter;
	}

	public bool IsChilds()
	{
		return m_Childs.Any();
	}

	public List<IPage> GetChilds()
	{
		return m_Childs;
	}

	public List<INode> GetRootBranch()
	{
		return new List<INode>();
	}

	public List<IBlock> GetBlocks()
	{
		return m_Blocks.Values.ToList();
	}

	public List<SpriteLink> GetImages()
	{
		return new List<SpriteLink>();
	}
}
