using System.Collections.Generic;
using System.Linq;
using Kingmaker.Networking.Desync;

namespace Kingmaker.Networking;

public class DefaultDesyncDetectionStrategy : BaseDesyncDetectionStrategy
{
	private readonly IDesyncHandler m_DesyncHandler;

	private int m_DesyncTickLast = -32768;

	public override bool HasDesync => Game.Instance.RealTimeController.CurrentNetworkTick - m_DesyncTickLast < 100;

	public DefaultDesyncDetectionStrategy()
	{
		m_DesyncHandler = InitializeDesyncHandler();
	}

	private static IDesyncHandler InitializeDesyncHandler()
	{
		List<IDesyncHandler> list = new List<IDesyncHandler>
		{
			new UIDesyncHandler()
		};
		list.Add(new SendToRemoteDesyncHandler(new ReleasePropsCollector()));
		PFLog.Net.Log("[DefaultDesyncDetectionStrategy] " + string.Join(", ", list.Select((IDesyncHandler x) => x.GetType().Name)) + " will be used for desync handle.");
		return new CompositeDesyncHandler(list);
	}

	public override void ReportState()
	{
		int stepIndex = (m_DesyncTickLast = Game.Instance.RealTimeController.CurrentNetworkTick);
		if (!base.WasDesync)
		{
			PhotonManager.Sync.HandleDesync(stepIndex, m_DesyncHandler);
			base.WasDesync = true;
		}
	}

	public override void Reset()
	{
		m_DesyncTickLast = -32768;
		base.WasDesync = false;
	}
}
