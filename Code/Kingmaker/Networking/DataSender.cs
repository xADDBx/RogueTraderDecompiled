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
	private readonly struct PlayerAckStatus
	{
		public readonly PhotonActorNumber Player;

		public readonly bool Received;

		public PlayerAckStatus(PhotonActorNumber player, bool received)
		{
			Player = player;
			Received = received;
		}
	}

	private const byte DataSendCode = 24;

	private const byte CancelCode = 25;

	private readonly RaiseEventOptions m_EventOptions;

	private readonly List<PlayerAckStatus> m_AckReceived;

	private readonly CancellationToken m_InterruptSendingCancellationToken;

	private readonly CancellationToken m_FullStopSendingCancellationToken;

	private readonly PhotonManager m_Sender;

	private readonly byte m_SendMetaCode;

	protected readonly int m_UniqueNumber;

	private int m_Offset;

	private TaskCompletionSource<bool> m_AllPlayersReadyTcs;

	private bool IsStarted => m_AllPlayersReadyTcs != null;

	protected DataSender(List<PhotonActorNumber> targetActors, int uniqueNumber, PhotonManager sender, byte sendMetaCode, CancellationToken interruptSendingCancellationToken, CancellationToken fullStopSendingCancellationToken)
	{
		m_InterruptSendingCancellationToken = interruptSendingCancellationToken;
		m_FullStopSendingCancellationToken = fullStopSendingCancellationToken;
		m_UniqueNumber = uniqueNumber;
		m_Sender = sender;
		m_SendMetaCode = sendMetaCode;
		m_EventOptions = new RaiseEventOptions
		{
			TargetActors = targetActors.Select((PhotonActorNumber a) => a.ActorNumber).ToArray()
		};
		m_AckReceived = new List<PlayerAckStatus>(targetActors.Count);
		foreach (PhotonActorNumber targetActor in targetActors)
		{
			m_AckReceived.Add(new PlayerAckStatus(targetActor, received: false));
		}
	}

	public async Task Send()
	{
		CancellationToken interruptSendingCancellationToken = m_InterruptSendingCancellationToken;
		interruptSendingCancellationToken.ThrowIfCancellationRequested();
		interruptSendingCancellationToken = m_FullStopSendingCancellationToken;
		interruptSendingCancellationToken.ThrowIfCancellationRequested();
		using (ByteArraySlice metaMessage = GetMetaPackage())
		{
			using (InitAllPlayersReadyTcs(m_FullStopSendingCancellationToken))
			{
				if (!m_Sender.SendMessage(m_SendMetaCode, metaMessage.Buffer, metaMessage.Offset, metaMessage.Count, m_EventOptions))
				{
					throw new SendMessageFailException($"Failed send meta [{m_SendMetaCode}], N={m_UniqueNumber}");
				}
				await m_AllPlayersReadyTcs.Task;
			}
		}
		ArraySegment<byte> bytes = GetMainPartBytes();
		int offset = 0;
		int index = 0;
		while (offset < bytes.Count)
		{
			CheckCancel();
			int num = Mathf.Min(bytes.Count - offset, SaveMetaData.MaxPacketSize);
			ArraySegment<byte> arraySegment = bytes.Slice(offset, num);
			m_Offset += num;
			using (InitAllPlayersReadyTcs(m_FullStopSendingCancellationToken))
			{
				if (!m_Sender.SendMessage(24, arraySegment.Array, arraySegment.Offset, arraySegment.Count, m_EventOptions))
				{
					throw new SendMessageFailException($"Failed to send data (package #{index + 1}), N={m_UniqueNumber}");
				}
				offset += num;
				PFLog.Net.Log($"[DataSender.Send] PacketType.{(byte)24} message was send. Bytes: {offset}/{bytes.Count}, #{index + 1} N={m_UniqueNumber}");
				await m_AllPlayersReadyTcs.Task;
			}
			int num2 = index + 1;
			index = num2;
		}
		void CheckCancel()
		{
			CancellationToken interruptSendingCancellationToken2 = m_InterruptSendingCancellationToken;
			if (interruptSendingCancellationToken2.IsCancellationRequested)
			{
				PFLog.Net.Error($"[DataSender.Send] Send cancel, N={m_UniqueNumber}");
				if (!m_Sender.SendMessage(25, null, 0, 0, m_EventOptions))
				{
					throw new SendMessageFailException($"Failed send cancel [{(byte)25}], N={m_UniqueNumber}");
				}
				interruptSendingCancellationToken2 = m_InterruptSendingCancellationToken;
				interruptSendingCancellationToken2.ThrowIfCancellationRequested();
			}
		}
		IDisposable InitAllPlayersReadyTcs(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			m_AllPlayersReadyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			for (int i = 0; i < m_AckReceived.Count; i++)
			{
				m_AckReceived[i] = new PlayerAckStatus(m_AckReceived[i].Player, received: false);
			}
			CheckAllPlayersReady();
			return token.CanBeCanceled ? token.Register(delegate
			{
				m_AllPlayersReadyTcs.TrySetCanceled();
			}) : default(CancellationTokenRegistration);
		}
	}

	public void OnAck(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		if (!AckPacketHelper.CheckAck(player, m_UniqueNumber, m_Offset, bytes))
		{
			return;
		}
		for (int i = 0; i < m_AckReceived.Count; i++)
		{
			if (player == m_AckReceived[i].Player)
			{
				if (m_AckReceived[i].Received)
				{
					PFLog.Net.Error($"[DataSender.OnAck] message duplicated! Player #{player.ToString()}, N={m_UniqueNumber}");
					return;
				}
				m_AckReceived[i] = new PlayerAckStatus(m_AckReceived[i].Player, received: true);
				CheckAllPlayersReady();
				return;
			}
		}
		PFLog.Net.Error(string.Format("[DataSender.OnAck] {0} not found, targets={1}, N={2}", player, string.Join(", ", m_EventOptions.TargetActors), m_UniqueNumber));
	}

	private void CheckAllPlayersReady()
	{
		if (m_AckReceived.Count == 0)
		{
			m_AllPlayersReadyTcs.TrySetCanceled();
			return;
		}
		foreach (PlayerAckStatus item in m_AckReceived)
		{
			if (!item.Received)
			{
				return;
			}
		}
		m_AllPlayersReadyTcs.TrySetResult(result: true);
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		for (int i = 0; i < m_AckReceived.Count; i++)
		{
			if (m_AckReceived[i].Player == player)
			{
				m_AckReceived.RemoveAt(i);
				break;
			}
		}
		if (IsStarted)
		{
			CheckAllPlayersReady();
		}
	}

	protected abstract ByteArraySlice GetMetaPackage();

	protected abstract ArraySegment<byte> GetMainPartBytes();
}
