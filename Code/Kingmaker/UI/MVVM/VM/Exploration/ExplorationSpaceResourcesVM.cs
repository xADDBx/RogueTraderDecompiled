using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationSpaceResourcesVM : SystemMapSpaceResourcesVM
{
	private readonly Dictionary<BlueprintResource, int> m_AdditionalResources = new Dictionary<BlueprintResource, int>();

	public void SetAdditionalResources(Colony colony)
	{
		m_AdditionalResources.Clear();
		if (colony == null)
		{
			UpdateData();
			return;
		}
		foreach (KeyValuePair<BlueprintResource, int> item in colony.RequiredResourcesForColony())
		{
			m_AdditionalResources.Add(item.Key, -item.Value);
		}
		foreach (KeyValuePair<BlueprintResource, int> item2 in colony.ProducedResourcesByColony())
		{
			if (!m_AdditionalResources.TryGetValue(item2.Key, out var _))
			{
				m_AdditionalResources.Add(item2.Key, 0);
			}
			m_AdditionalResources[item2.Key] += item2.Value;
		}
		UpdateData();
	}

	protected override void AddResourcesImpl(Dictionary<BlueprintResource, int> resources)
	{
		foreach (KeyValuePair<BlueprintResource, int> additionalResource in m_AdditionalResources)
		{
			var (blueprintResource, delta) = (KeyValuePair<BlueprintResource, int>)(ref additionalResource);
			ResourcesVMs.FirstOrDefault((ColonyResourceVM item) => item.BlueprintResource.Value == blueprintResource)?.UpdateCountAdditional(delta);
		}
	}
}
