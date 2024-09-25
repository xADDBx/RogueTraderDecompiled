using System;

namespace Kingmaker.Pathfinding;

public static class NodeListExtensions
{
	public static bool NonSingle(this ref NodeList nodeList)
	{
		bool flag = false;
		foreach (CustomGridNodeBase node in nodeList)
		{
			_ = node;
			if (flag)
			{
				return true;
			}
			flag = true;
		}
		return false;
	}

	public static CustomGridNodeBase First(this ref NodeList nodeList)
	{
		using (NodeList.Enumerator enumerator = nodeList.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		throw new InvalidOperationException("Node list is empty");
	}
}
