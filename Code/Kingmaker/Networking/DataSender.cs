using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Networking;

public abstract class DataSender
{
	private const byte DataSendCode = 31;

	private const byte CancelCode = 32;

	private readonly RaiseEventOptions m_EventOptions;

	private readonly CancellationToken m_InterruptSendingCancellationToken;

	private readonly CancellationToken m_FullStopSendingCancellationToken;

	private readonly IProgress<DataTransferProgressInfo> m_Progress;

	private readonly PhotonManager m_Sender;

	private readonly byte m_SendMetaCode;

	protected readonly StreamsController m_StreamsController;

	protected readonly int m_UniqueNumber;

	private int m_Offset;

	protected DataSender(List<PhotonActorNumber> targetActors, int uniqueNumber, PhotonManager sender, byte sendMetaCode, CancellationToken interruptSendingCancellationToken, CancellationToken fullStopSendingCancellationToken, IProgress<DataTransferProgressInfo> progress = null)
	{
		m_InterruptSendingCancellationToken = interruptSendingCancellationToken;
		m_FullStopSendingCancellationToken = fullStopSendingCancellationToken;
		m_Progress = progress;
		m_UniqueNumber = uniqueNumber;
		m_Sender = sender;
		m_SendMetaCode = sendMetaCode;
		m_StreamsController = new StreamsController(targetActors);
		m_EventOptions = new RaiseEventOptions
		{
			TargetActors = targetActors.Select((PhotonActorNumber a) => a.ActorNumber).ToArray()
		};
	}

	public async Task Send()
	{
		CancellationToken interruptSendingCancellationToken = m_InterruptSendingCancellationToken;
		interruptSendingCancellationToken.ThrowIfCancellationRequested();
		interruptSendingCancellationToken = m_FullStopSendingCancellationToken;
		interruptSendingCancellationToken.ThrowIfCancellationRequested();
		using (ByteArraySlice metaMessage = GetMetaPackage())
		{
			using (m_StreamsController.InitAllPlayersReadyTcs(m_FullStopSendingCancellationToken, 0, m_UniqueNumber))
			{
				if (!m_Sender.SendMessage(m_SendMetaCode, metaMessage.Buffer, metaMessage.Offset, metaMessage.Count, m_EventOptions))
				{
					throw new SendMessageFailException($"Failed send meta [{m_SendMetaCode}], N={m_UniqueNumber}");
				}
				await m_StreamsController.WaitAllStreams();
			}
		}
		ArraySegment<byte> bytes = GetMainPartBytes();
		if (bytes == null)
		{
			return;
		}
		Report(0, 0, bytes.Count);
		int offset = 0;
		int index = 0;
		while (offset < bytes.Count)
		{
			await CheckCancel();
			while (PhotonManager.Cheat.PauseDataSending)
			{
				await Task.Delay(33, m_FullStopSendingCancellationToken);
			}
			int num = Mathf.Min(bytes.Count - offset, SaveMetaData.MaxPacketSize);
			ArraySegment<byte> arraySegment = bytes.Slice(offset, num);
			using (m_StreamsController.InitAllPlayersReadyTcs(m_FullStopSendingCancellationToken, m_Offset, m_UniqueNumber))
			{
				m_Offset += num;
				if (!m_Sender.SendMessage(31, arraySegment.Array, arraySegment.Offset, arraySegment.Count, m_EventOptions))
				{
					throw new SendMessageFailException($"Failed to send data (package #{index + 1}), N={m_UniqueNumber}");
				}
				Report(num, m_Offset, bytes.Count);
				offset += num;
				PFLog.Net.Log($"[DataSender.Send] PacketType.{(byte)31} message was send. Bytes: {offset}/{bytes.Count}, #{index + 1} N={m_UniqueNumber}");
				await m_StreamsController.WaitAnyStream();
			}
			int num2 = index + 1;
			index = num2;
		}
		await m_StreamsController.WaitAllStreams();
		async Task CheckCancel()
		{
			CancellationToken interruptSendingCancellationToken2 = m_InterruptSendingCancellationToken;
			if (interruptSendingCancellationToken2.IsCancellationRequested)
			{
				await m_StreamsController.WaitAllStreams();
				PFLog.Net.Error($"[DataSender.Send] Send cancel, N={m_UniqueNumber}");
				if (!m_Sender.SendMessage(32, null, 0, 0, m_EventOptions))
				{
					throw new SendMessageFailException($"Failed send cancel [{(byte)32}], N={m_UniqueNumber}");
				}
				interruptSendingCancellationToken2 = m_InterruptSendingCancellationToken;
				interruptSendingCancellationToken2.ThrowIfCancellationRequested();
			}
		}
	}

	public virtual void OnAck(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		if (!AckPacketHelper.TryGetOffset(player, m_UniqueNumber, bytes, out var currentOffset))
		{
			m_StreamsController.Failed(new DataReceiveException($"Invalid ack offset from {player}"));
		}
		else
		{
			m_StreamsController.OnAck(player, currentOffset);
		}
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		m_StreamsController.OnPlayerLeftRoom(player);
	}

	protected abstract ByteArraySlice GetMetaPackage();

	protected abstract ArraySegment<byte> GetMainPartBytes();

	private void Report(int change, int current, int full)
	{
		m_Progress?.Report(new DataTransferProgressInfo(change, current, full));
	}
}
