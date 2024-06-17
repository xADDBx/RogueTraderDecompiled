using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.View.Covers;

namespace Kingmaker.Controllers;

public class ForcedCoversController : IControllerTick, IController
{
	private readonly Dictionary<CustomGridNodeBase, LosCalculations.CoverType> m_Cache = new Dictionary<CustomGridNodeBase, LosCalculations.CoverType>();

	private HashSet<IDynamicCoverProvider> m_RegisteredCoverProviders = new HashSet<IDynamicCoverProvider>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		m_Cache.Clear();
		foreach (IDynamicCoverProvider registeredCoverProvider in m_RegisteredCoverProviders)
		{
			foreach (CustomGridNodeBase node in registeredCoverProvider.Nodes)
			{
				m_Cache[node] = registeredCoverProvider.CoverType;
			}
		}
	}

	public void RegisterCoverProvider(IDynamicCoverProvider coverProvider)
	{
		m_RegisteredCoverProviders.Add(coverProvider);
	}

	public void UnregisterCoverProvider(IDynamicCoverProvider coverProvider)
	{
		m_RegisteredCoverProviders.Remove(coverProvider);
	}

	public bool TryGetCoverType(CustomGridNodeBase node, out LosCalculations.CoverType coverType)
	{
		return m_Cache.TryGetValue(node, out coverType);
	}
}
