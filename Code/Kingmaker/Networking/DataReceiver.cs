using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Kingmaker.Networking;

public abstract class DataReceiver
{
	private const byte AckCode = 23;

	private readonly TaskCompletionSource<bool> m_DownloadTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	private readonly PhotonActorNumber m_PlayerSource;

	private readonly PhotonManager m_Sender;

	private byte[] m_DownloadSaveBytes;

	private int m_CurrentOffset;

	protected abstract int MainPartLenght { get; }

	protected abstract int SenderUniqueNumber { get; }

	protected DataReceiver(PhotonActorNumber playerSource, [NotNull] PhotonManager sender)
	{
		m_PlayerSource = playerSource;
		m_Sender = sender;
	}

	public void OnMetaReceived(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		try
		{
			DeserializeMeta(bytes);
		}
		catch (Exception innerException)
		{
			m_DownloadTcs.SetException(new DataReceiveException("Can't parse meta message", innerException));
			return;
		}
		m_DownloadSaveBytes = new byte[MainPartLenght];
		PFLog.Net.Log($"[DataReceiver.OnSaveMetaReceived] {player}, N={SenderUniqueNumber}, len={MainPartLenght}");
	}

	public Task StartReceiving()
	{
		if (!SendAcknowledge(m_PlayerSource, m_CurrentOffset))
		{
			m_DownloadTcs.SetException(new SendMessageFailException($"Failed to send Acknowledge, {m_PlayerSource}, N={SenderUniqueNumber}"));
		}
		return m_DownloadTcs.Task;
	}

	public void OnDataReceived(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		if (player != m_PlayerSource)
		{
			m_DownloadTcs.SetException(new Exception($"{player} is differ from {m_PlayerSource}, N={SenderUniqueNumber}"));
			return;
		}
		bytes.CopyTo(m_DownloadSaveBytes.AsSpan(m_CurrentOffset));
		m_CurrentOffset += bytes.Length;
		PFLog.Net.Log($"[DataReceiver.OnDataReceived] Bytes {m_CurrentOffset}/{MainPartLenght} received, {m_PlayerSource}, N={SenderUniqueNumber}");
		if (m_CurrentOffset == MainPartLenght)
		{
			OnMainPartReceiveCompleted(m_PlayerSource, m_DownloadSaveBytes);
			m_DownloadTcs.SetResult(result: true);
		}
		if (!SendAcknowledge(player, m_CurrentOffset))
		{
			m_DownloadTcs.TrySetException(new SendMessageFailException($"Failed to send SaveAcknowledge, {m_PlayerSource}, N={SenderUniqueNumber}"));
		}
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		if (player == m_PlayerSource)
		{
			m_DownloadTcs.TrySetCanceled();
		}
	}

	protected abstract void DeserializeMeta(ReadOnlySpan<byte> bytes);

	protected abstract void OnMainPartReceiveCompleted(PhotonActorNumber playerSource, byte[] bytes);

	private bool SendAcknowledge(PhotonActorNumber player, int currentLength)
	{
		var (bytes, length) = AckPacketHelper.Make(SenderUniqueNumber, currentLength);
		return m_Sender.SendMessageTo(player, 23, bytes, 0, length);
	}

	public void OnCancel()
	{
		PFLog.Net.Error($"[DataReceiver.OnCancel] {m_PlayerSource}, N={SenderUniqueNumber}");
		m_DownloadTcs.TrySetCanceled();
	}
}
