using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Encyclopedia;

[TypeId("6ff07c7ff7009af4c9c3a4f74c792963")]
public class BlueprintEncyclopediaSkillPage : BlueprintEncyclopediaPage
{
	public class PageArchytype : IPage, INode
	{
		public BlueprintArchetype Archytype;

		public BlueprintEncyclopediaNode Parent { get; set; }

		public bool FirstExpanded => false;

		public PageArchytype(BlueprintArchetype archytype, BlueprintEncyclopediaNode parent)
		{
			Parent = parent;
			Archytype = archytype;
		}

		public List<IBlock> GetBlocks()
		{
			BlueprintCharacterClass parentClass = Archytype.GetParentClass();
			return new List<IBlock>
			{
				new SkillTable(parentClass, Archytype)
			};
		}

		public List<SpriteLink> GetImages()
		{
			return new List<SpriteLink>();
		}

		public bool IsChilds()
		{
			return false;
		}

		public List<IPage> GetChilds()
		{
			return null;
		}

		public Object GetInnerResource(string name)
		{
			return null;
		}

		public List<INode> GetRootBranch()
		{
			List<INode> list = new List<INode>();
			for (BlueprintEncyclopediaNode blueprintEncyclopediaNode = Parent; blueprintEncyclopediaNode != null; blueprintEncyclopediaNode = (blueprintEncyclopediaNode as BlueprintEncyclopediaPage)?.Parent ?? null)
			{
				list.Add(blueprintEncyclopediaNode);
			}
			list.Reverse();
			return list;
		}

		public string GetTitle()
		{
			return Archytype.Name;
		}
	}

	public class SkillTable : IBlockSkillTable, IBlock, IBlockText
	{
		public BlueprintCharacterClass CharacterClass;

		public BlueprintArchetype CharacterArchetype;

		public SkillTable(BlueprintCharacterClass characterClass)
		{
			CharacterClass = characterClass;
		}

		public SkillTable(BlueprintCharacterClass characterClass, BlueprintArchetype characterArchetype)
		{
			CharacterClass = characterClass;
			CharacterArchetype = characterArchetype;
		}

		public string GetText()
		{
			if (!CharacterArchetype)
			{
				return CharacterClass.Description;
			}
			return CharacterArchetype.Description;
		}
	}

	[SerializeField]
	[FormerlySerializedAs("Class")]
	private BlueprintCharacterClassReference m_Class;

	public BlueprintCharacterClass Class => m_Class?.Get();

	public override List<IBlock> GetBlocks()
	{
		return new List<IBlock>(base.GetBlocks())
		{
			new SkillTable(Class)
		};
	}

	public override string GetTitle()
	{
		return Class.Name;
	}

	public override List<IPage> GetChilds()
	{
		List<IPage> list = new List<IPage>();
		list.AddRange(base.GetChilds());
		foreach (BlueprintArchetype archetype in Class.Archetypes)
		{
			list.Add(new PageArchytype(archetype, this));
		}
		return list;
	}
}
