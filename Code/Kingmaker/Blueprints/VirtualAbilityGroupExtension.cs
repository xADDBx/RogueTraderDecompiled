using System.Collections.Generic;

namespace Kingmaker.Blueprints;

public static class VirtualAbilityGroupExtension
{
	public static List<BlueprintAbilityGroup> GetAllAbilityGroups(this BlueprintAbilityGroup group)
	{
		List<BlueprintAbilityGroup> result = new List<BlueprintAbilityGroup>();
		CollectAbilityGroupsRecursive(result, group);
		return result;
	}

	public static void CollectAbilityGroupsRecursive(List<BlueprintAbilityGroup> result, BlueprintAbilityGroup group)
	{
		VirtualAbilityGroupComponent component = group.GetComponent<VirtualAbilityGroupComponent>();
		if (component != null)
		{
			foreach (BlueprintAbilityGroup group2 in component.Groups)
			{
				CollectAbilityGroupsRecursive(result, group2);
			}
			return;
		}
		result.Add(group);
	}
}
