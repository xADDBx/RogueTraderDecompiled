using System;
using JetBrains.Annotations;

namespace Kingmaker.Networking;

public class ScreenshotReceiver : DataReceiver
{
	private readonly Action<PhotonActorNumber, ImageMetaData, byte[]> m_OnAvatarReceived;

	private ImageMetaData m_MetaData;

	protected override int MainPartLenght => m_MetaData.SaveLength;

	protected override int SenderUniqueNumber => m_MetaData.SenderUniqueNumber;

	public ScreenshotReceiver(PhotonActorNumber playerSource, [NotNull] PhotonManager sender, [NotNull] Action<PhotonActorNumber, ImageMetaData, byte[]> onAvatarReceived)
		: base(playerSource, sender)
	{
		m_OnAvatarReceived = onAvatarReceived;
	}

	protected override void DeserializeMeta(ReadOnlySpan<byte> bytes)
	{
		m_MetaData = NetMessageSerializer.DeserializeFromSpan<ImageMetaData>(bytes);
	}

	protected override void OnMainPartReceiveCompleted(PhotonActorNumber playerSource, byte[] bytes)
	{
		m_OnAvatarReceived(playerSource, m_MetaData, bytes);
	}
}
