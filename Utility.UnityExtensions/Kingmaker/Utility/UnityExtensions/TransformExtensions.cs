using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class TransformExtensions
{
	public static string GetPath(this Transform transform, bool asSiblingIndex = false)
	{
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(transform);
		while (transform.parent != null)
		{
			Transform parent = transform.parent;
			stack.Push(parent);
			transform = parent;
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (stack.Count > 0)
		{
			if (asSiblingIndex)
			{
				stringBuilder.Append("/").Append(stack.Pop().GetSiblingIndex());
			}
			else
			{
				stringBuilder.Append("/").Append(stack.Pop().name);
			}
		}
		string name = transform.gameObject.scene.name;
		string text = stringBuilder.ToString();
		return name + ":" + text.Substring(1, text.Length - 1);
	}
}
