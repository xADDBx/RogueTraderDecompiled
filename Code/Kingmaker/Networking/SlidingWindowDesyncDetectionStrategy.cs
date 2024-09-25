using System.Collections.Generic;
using System.Linq;
using Kingmaker.Networking.Desync;

namespace Kingmaker.Networking;

public class SlidingWindowDesyncDetectionStrategy : BaseDesyncDetectionStrategy
{
	private const int TicksUntilSeriousDesync = 41;

	private readonly IDesyncHandler m_PotentialDesyncHandler;

	private readonly IDesyncHandler m_SeriousDesyncHandler;

	private int m_DesyncTickFirst = -32768;

	private int m_DesyncTickLast = -32768;

	public override bool HasDesync
	{
		get
		{
			int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
			if (41 < currentNetworkTick - m_DesyncTickFirst)
			{
				return currentNetworkTick - m_DesyncTickLast < 100;
			}
			return false;
		}
	}

	public SlidingWindowDesyncDetectionStrategy()
	{
		m_PotentialDesyncHandler = InitializePotentialDesyncHandler();
		m_SeriousDesyncHandler = InitializeSeriousDesyncHandler();
	}

	private static IDesyncHandler InitializePotentialDesyncHandler()
	{
		List<IDesyncHandler> list = new List<IDesyncHandler>();
		PFLog.Net.Log("[SlidingWindowDesyncDetectionStrategy] " + string.Join(", ", list.Select((IDesyncHandler x) => x.GetType().Name)) + " will be used for potential desync handle.");
		return new CompositeDesyncHandler(list);
	}

	private static IDesyncHandler InitializeSeriousDesyncHandler()
	{
		List<IDesyncHandler> list = new List<IDesyncHandler>
		{
			new UIDesyncHandler()
		};
		list.Add(new SendToRemoteDesyncHandler(new ReleasePropsCollector()));
		PFLog.Net.Log("[SlidingWindowDesyncDetectionStrategy] " + string.Join(", ", list.Select((IDesyncHandler x) => x.GetType().Name)) + " will be used for serious desync handle.");
		return new CompositeDesyncHandler(list);
	}

	private bool HasPotentialDesync(int tickIndex)
	{
		return tickIndex - m_DesyncTickLast <= 41;
	}

	public override void ReportState()
	{
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		if (!HasPotentialDesync(currentNetworkTick))
		{
			PhotonManager.Sync.HandleDesync(currentNetworkTick, m_PotentialDesyncHandler);
			m_DesyncTickFirst = currentNetworkTick;
			m_DesyncTickLast = currentNetworkTick;
		}
		else
		{
			m_DesyncTickLast = currentNetworkTick;
		}
		if (HasDesync && !base.WasDesync)
		{
			PhotonManager.Sync.HandleDesync(currentNetworkTick, m_SeriousDesyncHandler);
			base.WasDesync = true;
		}
	}

	public override void Reset()
	{
		m_DesyncTickFirst = -32768;
		m_DesyncTickLast = -32768;
		base.WasDesync = false;
	}
}
