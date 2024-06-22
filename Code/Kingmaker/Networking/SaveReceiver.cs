using System;
using System.Threading;
using JetBrains.Annotations;

namespace Kingmaker.Networking;

public class SaveReceiver : DataReceiver
{
	private readonly Action<byte[]> m_OnReceived;

	private DataTransporterSaveMetaData m_MetaData;

	protected override int MainPartLength => m_MetaData.Length;

	protected override int SenderUniqueNumber => m_MetaData.SenderUniqueNumber;

	public SaveReceiver(PhotonActorNumber playerSource, [NotNull] PhotonManager sender, [NotNull] Action<byte[]> onReceived, IProgress<DataTransferProgressInfo> progress, CancellationToken cancellationToken)
		: base(playerSource, sender, progress, cancellationToken)
	{
		m_OnReceived = onReceived;
	}

	protected override void DeserializeMeta(ReadOnlySpan<byte> bytes)
	{
		m_MetaData = NetMessageSerializer.DeserializeFromSpan<DataTransporterSaveMetaData>(bytes);
	}

	protected override void OnMainPartReceiveCompleted(PhotonActorNumber playerSource, byte[] bytes)
	{
		m_OnReceived(bytes);
	}
}
