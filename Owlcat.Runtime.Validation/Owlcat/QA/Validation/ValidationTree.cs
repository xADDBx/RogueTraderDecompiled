using System.Collections.Generic;
using System.Linq;

namespace Owlcat.QA.Validation;

internal class ValidationTree
{
	private readonly List<ValidationTreeNode> m_Nodes = new List<ValidationTreeNode>();

	public IEnumerable<ValidationTreeNode> Nodes => m_Nodes;

	public ValidationTreeNode this[int idx] => m_Nodes[idx];

	public ValidationTreeNode CreateNew(int parent, string name, ValidationNodeType type, bool active)
	{
		m_Nodes.Add(new ValidationTreeNode(this, m_Nodes.Count, parent, name, type, active));
		return m_Nodes.Last();
	}

	public void Copy(ValidationTree tree)
	{
		m_Nodes.Clear();
		foreach (ValidationTreeNode node in tree.m_Nodes)
		{
			node.CopyTo(this);
		}
	}
}
