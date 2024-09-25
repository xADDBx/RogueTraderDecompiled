using System.Collections.Generic;
using Kingmaker.Networking.Hash;

namespace Kingmaker.Networking.Desync;

public class CompositeDesyncHandler : IDesyncHandler
{
	private readonly List<IDesyncHandler> m_Collectors;

	public CompositeDesyncHandler(List<IDesyncHandler> collectors)
	{
		m_Collectors = collectors;
	}

	public void RaiseDesync(HashableState data, DesyncMeta meta)
	{
		foreach (IDesyncHandler collector in m_Collectors)
		{
			collector.RaiseDesync(data, meta);
		}
	}
}
