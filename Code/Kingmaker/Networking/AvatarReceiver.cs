using System;
using JetBrains.Annotations;
using Kingmaker.Networking.Player;

namespace Kingmaker.Networking;

public class AvatarReceiver : DataReceiver
{
	private readonly Action<PhotonActorNumber, PlayerAvatar> m_OnAvatarReceived;

	private AvatarMetaData m_MetaData;

	protected override int MainPartLenght => m_MetaData.SaveLength;

	protected override int SenderUniqueNumber => m_MetaData.SenderUniqueNumber;

	public AvatarReceiver(PhotonActorNumber playerSource, [NotNull] PhotonManager sender, [NotNull] Action<PhotonActorNumber, PlayerAvatar> onAvatarReceived)
		: base(playerSource, sender)
	{
		m_OnAvatarReceived = onAvatarReceived;
	}

	protected override void DeserializeMeta(ReadOnlySpan<byte> bytes)
	{
		m_MetaData = NetMessageSerializer.DeserializeFromSpan<AvatarMetaData>(bytes);
	}

	protected override void OnMainPartReceiveCompleted(PhotonActorNumber playerSource, byte[] bytes)
	{
		PlayerAvatar arg = new PlayerAvatar(m_MetaData.AvatarWidth, bytes);
		m_OnAvatarReceived(playerSource, arg);
	}
}
