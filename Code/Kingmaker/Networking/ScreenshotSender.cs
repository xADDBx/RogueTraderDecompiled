using System;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Client.Photon;

namespace Kingmaker.Networking;

public class ScreenshotSender : DataSender
{
	private readonly byte[] m_ScreenshotData;

	public ScreenshotSender(List<PhotonActorNumber> targetActors, byte[] screenshotData, int uniqueNumber, PhotonManager sender, CancellationToken cancellationToken, CancellationToken fullStopSendingCancellationToken)
		: base(targetActors, uniqueNumber, sender, 22, cancellationToken, fullStopSendingCancellationToken)
	{
		m_ScreenshotData = screenshotData;
	}

	protected override ByteArraySlice GetMetaPackage()
	{
		ImageMetaData value = default(ImageMetaData);
		value.SenderUniqueNumber = m_UniqueNumber;
		value.SaveLength = m_ScreenshotData.Length;
		return NetMessageSerializer.SerializeToSlice(value);
	}

	protected override ArraySegment<byte> GetMainPartBytes()
	{
		return new ArraySegment<byte>(m_ScreenshotData);
	}
}
