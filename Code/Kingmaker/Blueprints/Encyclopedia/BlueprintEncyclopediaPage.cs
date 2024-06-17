using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("687801e1e573169469c7b258da67b5b4")]
public class BlueprintEncyclopediaPage : BlueprintEncyclopediaNode, IPage, INode
{
	[SerializeField]
	private BlueprintEncyclopediaNodeReference m_GlossaryEntry;

	[SerializeField]
	[FormerlySerializedAs("ParentAsset")]
	private BlueprintEncyclopediaNodeReference m_ParentAsset;

	[SerializeField]
	public List<BlueprintEncyclopediaBlock> Blocks = new List<BlueprintEncyclopediaBlock>();

	[SerializeField]
	private List<SpriteLink> m_Images = new List<SpriteLink>();

	public BlueprintEncyclopediaNode GlossaryEntry => m_GlossaryEntry;

	public BlueprintEncyclopediaNode ParentAsset
	{
		get
		{
			return m_ParentAsset?.Get();
		}
		set
		{
			m_ParentAsset = value.ToReference<BlueprintEncyclopediaNodeReference>();
		}
	}

	public BlueprintEncyclopediaNode Parent => ParentAsset;

	public List<INode> GetRootBranch()
	{
		List<INode> list = new List<INode>();
		for (BlueprintEncyclopediaNode blueprintEncyclopediaNode = ParentAsset; blueprintEncyclopediaNode != null; blueprintEncyclopediaNode = (blueprintEncyclopediaNode as BlueprintEncyclopediaPage)?.ParentAsset ?? null)
		{
			list.Add(blueprintEncyclopediaNode);
		}
		list.Reverse();
		return list;
	}

	public virtual List<IBlock> GetBlocks()
	{
		return new List<IBlock>(Blocks);
	}

	public virtual List<SpriteLink> GetImages()
	{
		return new List<SpriteLink>(m_Images);
	}
}
