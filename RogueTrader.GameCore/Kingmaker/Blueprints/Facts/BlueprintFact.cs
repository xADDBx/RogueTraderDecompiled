using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using MemoryPack;
using StateHasher.Core;

namespace Kingmaker.Blueprints.Facts;

[HashRoot]
[TypeId("bdddf6ca1cd54d888a214ffce2286e39")]
[MemoryPackable(GenerateType.NoGenerate)]
public abstract class BlueprintFact : BlueprintScriptableObject
{
	public bool? IsGameStateCache;

	protected abstract Type GetFactType();

	public void CollectComponents(List<BlueprintComponent> result)
	{
		Queue<BlueprintScriptableObject> queue = new Queue<BlueprintScriptableObject>();
		HashSet<BlueprintScriptableObject> hashSet = new HashSet<BlueprintScriptableObject>();
		queue.Enqueue(this);
		hashSet.Add(this);
		while (queue.Count > 0)
		{
			BlueprintComponent[] componentsArray = queue.Dequeue().ComponentsArray;
			foreach (BlueprintComponent blueprintComponent in componentsArray)
			{
				ComponentsList componentsList = blueprintComponent as ComponentsList;
				if ((bool)componentsList)
				{
					BlueprintComponentList list = componentsList.List;
					if (list != null && !hashSet.Contains(list))
					{
						queue.Enqueue(list);
						hashSet.Add(list);
					}
				}
				else
				{
					result.Add(blueprintComponent);
				}
			}
		}
	}
}
