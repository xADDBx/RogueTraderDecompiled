using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kingmaker.Networking;

public class StreamsController
{
	private readonly struct StreamPacketData
	{
		public readonly int Offset;

		public readonly TaskCompletionSource<bool> Tcs;

		private readonly List<PlayerAckStatus> m_AckReceived;

		private readonly int m_UniqueNumber;

		public StreamPacketData(int offset, List<PhotonActorNumber> targetActors, int uniqueNumber)
		{
			Offset = offset;
			Tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			m_AckReceived = new List<PlayerAckStatus>(targetActors.Count);
			foreach (PhotonActorNumber targetActor in targetActors)
			{
				m_AckReceived.Add(new PlayerAckStatus(targetActor, received: false));
			}
			m_UniqueNumber = uniqueNumber;
		}

		public void OnAck(PhotonActorNumber player)
		{
			for (int i = 0; i < m_AckReceived.Count; i++)
			{
				if (player == m_AckReceived[i].Player)
				{
					if (m_AckReceived[i].Received)
					{
						PFLog.Net.Error($"[DataSender.OnAck] message duplicated! Player #{player.ToString()}, N={m_UniqueNumber}");
					}
					else
					{
						m_AckReceived[i] = new PlayerAckStatus(m_AckReceived[i].Player, received: true);
					}
					return;
				}
			}
			PFLog.Net.Error(string.Format("[DataSender.OnAck] {0} not found, targets={1}, N={2}", player, string.Join(", ", m_AckReceived.Select((PlayerAckStatus t) => t.Player)), m_UniqueNumber));
		}

		public void OnPlayerLeft(PhotonActorNumber player)
		{
			for (int i = 0; i < m_AckReceived.Count; i++)
			{
				if (m_AckReceived[i].Player == player)
				{
					m_AckReceived.RemoveAt(i);
					break;
				}
			}
		}

		public bool CheckAllPlayersReady()
		{
			if (m_AckReceived.Count == 0)
			{
				Tcs.TrySetException(new DataTransportAllTargetsLeftException());
				return true;
			}
			foreach (PlayerAckStatus item in m_AckReceived)
			{
				if (!item.Received)
				{
					return false;
				}
			}
			Tcs.TrySetResult(result: true);
			return true;
		}
	}

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

	public static int DefaultStreamsCount = 3;

	private readonly List<StreamPacketData> m_StreamsData;

	private readonly List<PhotonActorNumber> m_TargetActors;

	private readonly int m_StreamsCount;

	public StreamsController(List<PhotonActorNumber> targetActors, int streamsCount = -1)
	{
		m_TargetActors = targetActors;
		m_StreamsCount = ((streamsCount == -1) ? DefaultStreamsCount : streamsCount);
		m_StreamsData = new List<StreamPacketData>(m_StreamsCount);
		foreach (PhotonActorNumber targetActor in targetActors)
		{
			_ = targetActor;
		}
	}

	public async Task WaitAnyStream()
	{
		if (m_StreamsData.Count < m_StreamsCount)
		{
			return;
		}
		List<Task> list = new List<Task>(m_StreamsData.Count);
		foreach (StreamPacketData streamsDatum in m_StreamsData)
		{
			list.Add(streamsDatum.Tcs.Task);
		}
		await (await Task.WhenAny(list));
	}

	public async Task WaitAllStreams()
	{
		while (m_StreamsData.Count > 0)
		{
			await m_StreamsData[0].Tcs.Task;
		}
	}

	public void Clear()
	{
		m_StreamsData.Clear();
	}

	public IDisposable InitAllPlayersReadyTcs(CancellationToken token, int offset = 0, int uniqueNumber = 0)
	{
		token.ThrowIfCancellationRequested();
		StreamPacketData data = new StreamPacketData(offset, m_TargetActors, uniqueNumber);
		m_StreamsData.Add(data);
		CheckAllPlayersReady();
		return token.CanBeCanceled ? token.Register(delegate
		{
			data.Tcs.TrySetCanceled();
		}) : default(CancellationTokenRegistration);
	}

	private void CheckAllPlayersReady()
	{
		int num = 0;
		while (num < m_StreamsData.Count)
		{
			if (m_StreamsData[num].CheckAllPlayersReady())
			{
				m_StreamsData.RemoveAt(0);
			}
			else
			{
				num++;
			}
		}
	}

	public void OnAck(PhotonActorNumber player, int offset)
	{
		for (int i = 0; i < m_StreamsData.Count && m_StreamsData[i].Offset != offset; i++)
		{
		}
		if (!TryGetByOffset(offset, m_StreamsData, out var data2))
		{
			PFLog.Net.Error($"[StreamsController.OnAck]: unexpected offset! Player #{player.ToString()} offset={offset}");
			return;
		}
		data2.OnAck(player);
		PFLog.Net.Log($"[StreamsController.OnAck]: Player #{player.ToString()} offset={offset}");
		CheckAllPlayersReady();
		static bool TryGetByOffset(int offset, List<StreamPacketData> streamData, out StreamPacketData data)
		{
			foreach (StreamPacketData streamDatum in streamData)
			{
				if (streamDatum.Offset == offset)
				{
					data = streamDatum;
					return true;
				}
			}
			data = default(StreamPacketData);
			return false;
		}
	}

	public void Failed(Exception exception)
	{
		foreach (StreamPacketData streamsDatum in m_StreamsData)
		{
			streamsDatum.Tcs.TrySetException(exception);
		}
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		m_TargetActors.Remove(player);
		foreach (StreamPacketData streamsDatum in m_StreamsData)
		{
			streamsDatum.OnPlayerLeft(player);
		}
		CheckAllPlayersReady();
	}
}
