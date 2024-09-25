using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

public class EncyclopediaData : IHashable
{
	[JsonProperty]
	private readonly HashSet<BlueprintEncyclopediaNode> m_ViewedNodes = new HashSet<BlueprintEncyclopediaNode>();

	public IEnumerable<BlueprintEncyclopediaNode> ViewedNodes => m_ViewedNodes;

	public void MarkViewed(BlueprintEncyclopediaNode node)
	{
		if (m_ViewedNodes.Add(node))
		{
			EventBus.RaiseEvent(delegate(IEncyclopediaNodeViewedHandler h)
			{
				h.HandleEncyclopediaNodeViewed(node);
			});
		}
	}

	public bool IsViewed(BlueprintEncyclopediaNode node)
	{
		return m_ViewedNodes.Contains(node);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		HashSet<BlueprintEncyclopediaNode> viewedNodes = m_ViewedNodes;
		if (viewedNodes != null)
		{
			int num = 0;
			foreach (BlueprintEncyclopediaNode item in viewedNodes)
			{
				num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}
}
