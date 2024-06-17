using System;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Client.Photon;
using Kingmaker.Networking.Player;

namespace Kingmaker.Networking;

public class AvatarSender : DataSender
{
	private readonly PlayerAvatar m_Avatar;

	public AvatarSender(List<PhotonActorNumber> targetActors, PlayerAvatar avatar, int uniqueNumber, PhotonManager sender, CancellationToken interruptSendingCancellationToken, CancellationToken fullStopSendingCancellationToken)
		: base(targetActors, uniqueNumber, sender, 21, interruptSendingCancellationToken, fullStopSendingCancellationToken)
	{
		m_Avatar = avatar;
	}

	protected override ByteArraySlice GetMetaPackage()
	{
		AvatarMetaData value = default(AvatarMetaData);
		value.SenderUniqueNumber = m_UniqueNumber;
		value.AvatarWidth = m_Avatar.Width;
		value.SaveLength = m_Avatar.Data.Length;
		return NetMessageSerializer.SerializeToSlice(value);
	}

	protected override ArraySegment<byte> GetMainPartBytes()
	{
		return new ArraySegment<byte>(m_Avatar.Data);
	}
}
