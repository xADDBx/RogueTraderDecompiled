using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kingmaker.Networking;

public abstract class CommandSender : DataSender
{
	public struct AckData
	{
		public PhotonActorNumber Player;

		public byte[] Bytes;
	}

	private readonly List<AckData> m_AckData = new List<AckData>();

	protected CommandSender(List<PhotonActorNumber> targetActors, int uniqueNumber, PhotonManager sender, byte sendMetaCode, CancellationToken interruptSendingCancellationToken, CancellationToken fullStopSendingCancellationToken, IProgress<DataTransferProgressInfo> progress = null)
		: base(targetActors, uniqueNumber, sender, sendMetaCode, interruptSendingCancellationToken, fullStopSendingCancellationToken, progress)
	{
	}

	public async Task<List<AckData>> SendCommand()
	{
		await Send();
		return m_AckData;
	}

	protected override ArraySegment<byte> GetMainPartBytes()
	{
		return null;
	}

	public override void OnAck(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		m_AckData.Add(new AckData
		{
			Player = player,
			Bytes = bytes.ToArray()
		});
		m_StreamsController.OnAck(player, 0);
	}
}
