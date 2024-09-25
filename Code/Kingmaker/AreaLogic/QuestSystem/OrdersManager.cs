using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Globalmap.Blueprints.Colonization;

namespace Kingmaker.AreaLogic.QuestSystem;

public class OrdersManager
{
	public static bool CanCompleteOrder(QuestObjective objective)
	{
		OrderObjectiveInfo orderObjectiveInfo = objective?.Blueprint.GetComponent<OrderObjectiveInfo>();
		if (orderObjectiveInfo == null)
		{
			return false;
		}
		Dictionary<BlueprintResource, int> dictionary = Game.Instance.ColonizationController.AllResourcesInPool();
		bool flag = true;
		ResourceData[] resources = orderObjectiveInfo.Resources;
		foreach (ResourceData resourceData in resources)
		{
			if (!dictionary.TryGetValue(resourceData.Resource.Get(), out var value))
			{
				flag = false;
			}
			if (value < resourceData.Count)
			{
				flag = false;
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}
}
