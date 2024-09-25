using System;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Client.Photon;

namespace Kingmaker.Networking;

public class SaveSender : DataSender
{
	private readonly ArraySegment<byte> m_SaveBytes;

	public SaveSender(List<PhotonActorNumber> targetActors, ArraySegment<byte> saveBytes, int uniqueNumber, PhotonManager sender, CancellationToken interruptSendingCancellationToken, CancellationToken fullStopSendingCancellationToken, IProgress<DataTransferProgressInfo> progress)
		: base(targetActors, uniqueNumber, sender, 24, interruptSendingCancellationToken, fullStopSendingCancellationToken, progress)
	{
		m_SaveBytes = saveBytes;
	}

	protected override ByteArraySlice GetMetaPackage()
	{
		DataTransporterSaveMetaData value = default(DataTransporterSaveMetaData);
		value.SenderUniqueNumber = m_UniqueNumber;
		ArraySegment<byte> saveBytes = m_SaveBytes;
		value.Length = saveBytes.Count;
		return NetMessageSerializer.SerializeToSlice(value);
	}

	protected override ArraySegment<byte> GetMainPartBytes()
	{
		return m_SaveBytes;
	}
}
