using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Kingmaker.Networking;

public abstract class DataReceiver
{
	private const byte AckCode = 30;

	private readonly TaskCompletionSource<bool> m_DownloadTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

	private readonly PhotonActorNumber m_PlayerSource;

	protected readonly PhotonManager m_Sender;

	private readonly IProgress<DataTransferProgressInfo> m_Progress;

	private CancellationTokenRegistration m_CancellationTokenRegistration;

	private byte[] m_DownloadSaveBytes;

	private int m_CurrentOffset;

	public Task CompletionTask => m_DownloadTcs.Task;

	protected abstract int MainPartLength { get; }

	protected abstract int SenderUniqueNumber { get; }

	protected DataReceiver(PhotonActorNumber playerSource, [NotNull] PhotonManager sender, IProgress<DataTransferProgressInfo> progress = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		m_PlayerSource = playerSource;
		m_Sender = sender;
		m_Progress = progress;
		m_CancellationTokenRegistration = cancellationToken.Register(OnCancel);
	}

	public void Dispose()
	{
		m_CancellationTokenRegistration.Dispose();
	}

	public virtual void OnMetaReceived(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
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
		m_DownloadSaveBytes = new byte[MainPartLength];
		Report(0, 0, MainPartLength);
		PFLog.Net.Log($"[DataReceiver.OnSaveMetaReceived] {player}, N={SenderUniqueNumber}, len={MainPartLength}");
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
		if (!SendAcknowledge(player, m_CurrentOffset))
		{
			m_DownloadTcs.TrySetException(new SendMessageFailException($"Failed to send SaveAcknowledge, {m_PlayerSource}, N={SenderUniqueNumber}"));
		}
		bytes.CopyTo(m_DownloadSaveBytes.AsSpan(m_CurrentOffset));
		m_CurrentOffset += bytes.Length;
		Report(bytes.Length, m_CurrentOffset, MainPartLength);
		PFLog.Net.Log($"[DataReceiver.OnDataReceived] Bytes {m_CurrentOffset}/{MainPartLength} received, {m_PlayerSource}, N={SenderUniqueNumber}");
		if (m_CurrentOffset == MainPartLength)
		{
			OnMainPartReceiveCompleted(m_PlayerSource, m_DownloadSaveBytes);
			m_DownloadTcs.SetResult(result: true);
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

	protected virtual bool SendAcknowledge(PhotonActorNumber player, int currentLength)
	{
		var (bytes, length) = AckPacketHelper.Make(SenderUniqueNumber, currentLength);
		return m_Sender.SendMessageTo(player, 30, bytes, 0, length);
	}

	private void Report(int change, int current, int full)
	{
		m_Progress?.Report(new DataTransferProgressInfo(change, current, full));
	}

	public virtual void OnCancel()
	{
		PFLog.Net.Log($"[DataReceiver.OnCancel] {m_PlayerSource}, N={SenderUniqueNumber}");
		m_DownloadTcs.TrySetCanceled();
	}
}
