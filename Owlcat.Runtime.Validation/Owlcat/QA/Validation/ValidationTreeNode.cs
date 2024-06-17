using System.Collections.Generic;
using System.Linq;

namespace Owlcat.QA.Validation;

internal readonly struct ValidationTreeNode
{
	private readonly ValidationTree m_Tree;

	private readonly ValidationNodeType m_Type;

	private readonly int m_Idx;

	private readonly int m_Parent;

	private readonly string m_Name;

	private readonly bool m_Active;

	public static readonly ValidationTreeNode Root = new ValidationTreeNode(null, -1, -1, "", ValidationNodeType.Object, active: true);

	private ValidationTreeNode? Parent
	{
		get
		{
			if (m_Parent < 0)
			{
				return null;
			}
			return m_Tree[m_Parent];
		}
	}

	public int Index => m_Idx;

	public bool IsActive => m_Active;

	public string Name => m_Name;

	public ValidationTreeNode(ValidationTree tree, int idx, int parent, string name, ValidationNodeType type, bool active)
	{
		m_Idx = idx;
		m_Parent = parent;
		m_Tree = tree;
		m_Name = name;
		m_Type = type;
		m_Active = active;
	}

	internal ValidationTreeNode CreateChild(string name, ValidationNodeType type, bool active)
	{
		if (m_Tree != null)
		{
			return m_Tree.CreateNew(m_Idx, name, type, active);
		}
		return new ValidationTree().CreateNew(-1, name, type, active);
	}

	public IEnumerable<ValidationTreeNode> ParentChain()
	{
		ValidationTreeNode? node = this;
		while (node.HasValue)
		{
			yield return node.Value;
			node = node.Value.Parent;
		}
	}

	public void CopyTo(ValidationTree tree)
	{
		tree.CreateNew(m_Parent, m_Name, m_Type, m_Active);
	}

	public override string ToString()
	{
		return string.Join("", from v in ParentChain().Reverse()
			select v.GetSeparator() + v.m_Name);
	}

	private string GetSeparator()
	{
		if (m_Idx == 0)
		{
			return "";
		}
		return m_Type switch
		{
			ValidationNodeType.Component => ":", 
			ValidationNodeType.Field => "->", 
			_ => ".", 
		};
	}
}
