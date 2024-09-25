using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ExitGames.Client.Photon;
using Kingmaker.Blueprints;
using Kingmaker.Enums;

namespace Kingmaker.Networking;

public class CustomPortraitSender : DataSender
{
	private readonly PortraitData m_PortraitData;

	private int m_AllFilesLength;

	public CustomPortraitSender(List<PhotonActorNumber> targetActors, PortraitData portraitData, int uniqueNumber, PhotonManager sender, CancellationToken interruptSendingCancellationToken, CancellationToken fullStopSendingCancellationToken, IProgress<DataTransferProgressInfo> progress)
		: base(targetActors, uniqueNumber, sender, 23, interruptSendingCancellationToken, fullStopSendingCancellationToken, progress)
	{
		m_PortraitData = portraitData;
	}

	protected override ByteArraySlice GetMetaPackage()
	{
		CustomPortraitMetaData customPortraitMetaData = default(CustomPortraitMetaData);
		customPortraitMetaData.SenderUniqueNumber = m_UniqueNumber;
		customPortraitMetaData.CustomPortraitId = m_PortraitData.CustomId;
		customPortraitMetaData.PortraitGuid = CustomPortraitsManager.Instance.GetOrCreatePortraitGuid(m_PortraitData.CustomId);
		customPortraitMetaData.LengthSmallPortrait = GetPortraitLength(PortraitType.SmallPortrait);
		customPortraitMetaData.LengthHalfPortrait = GetPortraitLength(PortraitType.HalfLengthPortrait);
		customPortraitMetaData.LengthFullPortrait = GetPortraitLength(PortraitType.FullLengthPortrait);
		CustomPortraitMetaData value = customPortraitMetaData;
		m_AllFilesLength = value.LengthSmallPortrait + value.LengthHalfPortrait + value.LengthFullPortrait;
		return NetMessageSerializer.SerializeToSlice(value);
		int GetPortraitLength(PortraitType type)
		{
			return (int)new FileInfo(CustomPortraitsManager.Instance.GetPortraitPath(m_PortraitData.CustomId, type)).Length;
		}
	}

	protected override ArraySegment<byte> GetMainPartBytes()
	{
		byte[] array = new byte[m_AllFilesLength];
		int num = 0;
		for (int i = 0; i <= 2; i++)
		{
			using FileStream fileStream = new FileStream(CustomPortraitsManager.Instance.GetPortraitPath(m_PortraitData.CustomId, (PortraitType)i), FileMode.Open);
			num += fileStream.Read(array, num, m_AllFilesLength - num);
		}
		return new ArraySegment<byte>(array);
	}
}
