using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;

namespace Kingmaker.Networking;

public class CharGenPortraitSyncer : IProgress<DataTransferProgressInfo>
{
	private class SenderData
	{
		public Guid Guid;

		public SyncPortraitCommandSender SyncCommand;
	}

	private class ReceiverData
	{
		public BlueprintPortrait Blueprint;

		public Guid Guid;

		public bool AlreadyHasPortrait;
	}

	private class SyncPortraitCommandSender : CommandSender
	{
		private readonly Guid m_PortraitGuid;

		private const byte Code = 40;

		private const int UniqueNumber = 100500;

		public SyncPortraitCommandSender(Guid portraitGuid, List<PhotonActorNumber> targetActors, PhotonManager sender, CancellationToken interruptSendingCancellationToken, CancellationToken fullStopSendingCancellationToken, IProgress<DataTransferProgressInfo> progress = null)
			: base(targetActors, 100500, sender, 40, interruptSendingCancellationToken, fullStopSendingCancellationToken, progress)
		{
			m_PortraitGuid = portraitGuid;
		}

		protected override ByteArraySlice GetMetaPackage()
		{
			SaveMetaAcknowledgeData saveMetaAcknowledgeData = new SaveMetaAcknowledgeData();
			saveMetaAcknowledgeData.PortraitsGuid = new SaveMetaAcknowledgeData.GuidData[1]
			{
				new SaveMetaAcknowledgeData.GuidData
				{
					Guid = m_PortraitGuid,
					AlreadyHave = true
				}
			};
			return NetMessageSerializer.SerializeToSlice(saveMetaAcknowledgeData);
		}
	}

	private readonly PhotonManager m_PhotonManager;

	private readonly CustomPortraitsManager m_PortraitsManager;

	private SenderData m_Sender;

	private ReceiverData m_Receiver;

	private readonly FloatReactiveProperty m_LoadingPortraitProgress = new FloatReactiveProperty();

	private readonly ReactiveCommand m_LoadingPortraitProgressCloseTrigger = new ReactiveCommand();

	public CharGenPortraitSyncer(PhotonManager photonManager, CustomPortraitsManager portraitsManager)
	{
		m_PhotonManager = photonManager;
		m_PortraitsManager = portraitsManager;
	}

	public void SetNewPortraitForSending(Guid guid)
	{
		if (m_Sender == null)
		{
			m_Sender = new SenderData();
		}
		m_Sender.Guid = guid;
	}

	public void SetPortraitForReceiving(Guid guid, BlueprintPortrait blueprint, bool alreadyHasPortrait)
	{
		if (m_Receiver == null)
		{
			m_Receiver = new ReceiverData();
		}
		m_Receiver.Blueprint = blueprint;
		m_Receiver.Guid = guid;
		m_Receiver.AlreadyHasPortrait = alreadyHasPortrait;
	}

	public void ClearPortraitForSending()
	{
		PFLog.Net.Log("[CharGenPortraitSyncer.ClearPortraitForSending]");
		m_Sender = null;
	}

	public bool IsNeedSyncPortrait()
	{
		if (m_Sender != null)
		{
			return m_Sender.Guid != default(Guid);
		}
		return false;
	}

	public async Task SyncPortraits(bool isSender)
	{
		PFLog.Net.Log($"[CharGenPortraitSyncer.SyncPortraits] as sender={isSender}");
		if (isSender)
		{
			ShowUI();
			try
			{
				await SendPortrait(m_Sender.Guid);
				return;
			}
			finally
			{
				HideUI();
			}
		}
		if (m_Receiver.AlreadyHasPortrait)
		{
			return;
		}
		ShowUI();
		try
		{
			PhotonActorNumber player = Game.Instance.Player.MainCharacterEntity.GetPlayer();
			await StartReceivingPortrait(player);
		}
		finally
		{
			HideUI();
		}
	}

	public void OnMessage(byte code, PhotonActorNumber actorNumber, ReadOnlySpan<byte> bytes)
	{
		switch (code)
		{
		case 40:
			SendCommandAck(actorNumber, bytes);
			break;
		case 41:
			m_Sender.SyncCommand.OnAck(actorNumber, bytes);
			break;
		default:
			throw new Exception($"Unexpected code: {code}");
		}
	}

	private async Task StartReceivingPortrait(PhotonActorNumber sourceActorNumber)
	{
		DataTransporter dataTransporter = PhotonManager.Instance.DataTransporter;
		try
		{
			PFLog.Net.Log($"[CharGenPortraitSyncer.StartReceivingPortrait] start {m_Receiver.Guid}");
			Task waitSourcePlayerLeftTask = PhotonManager.Instance.WaitPlayerLeftRoom(sourceActorNumber);
			if (waitSourcePlayerLeftTask.IsCompleted)
			{
				return;
			}
			TaskCompletionSource<bool> waitDataReceiverCreated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			Task portraitReceiveTask;
			await using (default(CancellationToken).Register(delegate
			{
				waitDataReceiverCreated.TrySetCanceled();
			}))
			{
				dataTransporter.CustomPortraitProgress += Report;
				portraitReceiveTask = null;
				dataTransporter.CreateNewReceiver += DataTransporterOnCreateNewReceiver;
				try
				{
					await Task.WhenAny(waitSourcePlayerLeftTask, waitDataReceiverCreated.Task);
				}
				finally
				{
					dataTransporter.CreateNewReceiver -= DataTransporterOnCreateNewReceiver;
				}
				if (!waitDataReceiverCreated.Task.IsCompleted)
				{
					if (waitSourcePlayerLeftTask.IsCompletedSuccessfully)
					{
						return;
					}
					await waitSourcePlayerLeftTask;
					return;
				}
				if (!waitDataReceiverCreated.Task.IsCompletedSuccessfully)
				{
					return;
				}
				await portraitReceiveTask;
				CustomPortraitsManager.Instance.TryGetPortraitId(m_Receiver.Guid, out var id);
				m_Receiver.Blueprint.Data = new PortraitData(id);
				EventBus.RaiseEvent(delegate(ICharGenPortraitHandler h)
				{
					h.HandleSetPortrait(m_Receiver.Blueprint);
				});
				PFLog.Net.Log($"[CharGenPortraitSyncer.StartReceivingPortrait] end receiving {m_Receiver.Guid}");
			}
			void DataTransporterOnCreateNewReceiver(DataReceiver obj)
			{
				if (obj.GetType() == typeof(CustomPortraitReceiver))
				{
					portraitReceiveTask = obj.CompletionTask;
					waitDataReceiverCreated.TrySetResult(result: true);
				}
			}
		}
		catch (OperationCanceledException)
		{
			PFLog.Net.Log($"[CharGenPortraitSyncer.StartReceivingPortrait] cancelled {m_Receiver.Guid}");
		}
		catch (Exception ex2)
		{
			PFLog.Net.Exception(ex2);
		}
		finally
		{
			dataTransporter.CustomPortraitProgress -= Report;
		}
	}

	private void ShowUI()
	{
		string waitMessage = UIStrings.Instance.CharGen.WaitForDownloadingPortraits;
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(waitMessage, DialogMessageBoxBase.BoxType.ProgressBar, null, null, null, null, null, null, null, 0, uint.MaxValue, m_LoadingPortraitProgress, m_LoadingPortraitProgressCloseTrigger);
		});
	}

	private void HideUI()
	{
		m_LoadingPortraitProgressCloseTrigger.Execute();
	}

	public void Report(DataTransferProgressInfo progress)
	{
		m_LoadingPortraitProgress.Value = (float)progress.CurrentProgress / (float)progress.FullProgress;
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		m_Sender?.SyncCommand?.OnPlayerLeftRoom(player);
	}

	private async Task SendPortrait(Guid portraitGuid)
	{
		m_Sender.SyncCommand = new SyncPortraitCommandSender(portraitGuid, GetAllPlayers(), m_PhotonManager, default(CancellationToken), default(CancellationToken));
		List<PhotonActorNumber> list = GetWaitingPortraitPlayers(await m_Sender.SyncCommand.SendCommand(), portraitGuid);
		if (list.Count == 0)
		{
			PFLog.Net.Log("[CharGenPortraitSyncer.SendPortrait] no targets");
			return;
		}
		if (!m_PortraitsManager.TryGetPortraitId(portraitGuid, out var id))
		{
			PFLog.Net.Error($"[CharGenPortraitSyncer.Do] can't find portrait {portraitGuid}");
			return;
		}
		PFLog.Net.Log($"[CharGenPortraitSyncer.SendPortrait] start sending {portraitGuid}");
		PortraitData portrait = new PortraitData(id);
		await PhotonManager.Instance.DataTransporter.SendCustomPortrait(portrait, list, this);
		PFLog.Net.Log($"[CharGenPortraitSyncer.SendPortrait] end sending {portraitGuid}");
		List<PhotonActorNumber> GetAllPlayers()
		{
			ReadonlyList<PlayerInfo> allPlayers = m_PhotonManager.AllPlayers;
			if (allPlayers.Count <= 1)
			{
				return null;
			}
			List<PhotonActorNumber> list3 = new List<PhotonActorNumber>(allPlayers.Count - 1);
			foreach (PlayerInfo item in allPlayers)
			{
				if (item.UserId != m_PhotonManager.LocalPlayerUserId)
				{
					list3.Add(item.Player);
				}
			}
			return list3;
		}
		static List<PhotonActorNumber> GetWaitingPortraitPlayers(List<CommandSender.AckData> responseFromPlayers, Guid portraitGuid)
		{
			List<PhotonActorNumber> list2 = new List<PhotonActorNumber>();
			foreach (CommandSender.AckData responseFromPlayer in responseFromPlayers)
			{
				SaveMetaAcknowledgeData saveMetaAcknowledgeData;
				try
				{
					saveMetaAcknowledgeData = NetMessageSerializer.DeserializeFromSpan<SaveMetaAcknowledgeData>(responseFromPlayer.Bytes);
				}
				catch (Exception ex)
				{
					PFLog.Net.Exception(ex, $"Can parse bytes from {responseFromPlayer.Player}");
					return list2;
				}
				if (!saveMetaAcknowledgeData.PortraitsGuid[0].AlreadyHave)
				{
					list2.Add(responseFromPlayer.Player);
					PFLog.Net.Log($"[CharGenPortraitSyncer.GetWaitingPortraitPlayers] add {responseFromPlayer.Player}");
				}
			}
			return list2;
		}
	}

	private void SendCommandAck(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		SaveMetaAcknowledgeData saveMetaAcknowledgeData;
		try
		{
			saveMetaAcknowledgeData = NetMessageSerializer.DeserializeFromSpan<SaveMetaAcknowledgeData>(bytes);
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex, $"Can parse bytes from {player}");
			return;
		}
		Guid guid2 = saveMetaAcknowledgeData.PortraitsGuid[0].Guid;
		string portraitId;
		bool alreadyHasPortrait2 = SavePacker.TryGetPortraitIdFromGuid(guid2, out portraitId);
		if (!SendAck(m_PhotonManager, player, guid2, alreadyHasPortrait2))
		{
			PFLog.Net.Error($"[CharGenPortraitSyncer.SendCommandAck] can't send {player} ack");
		}
		static bool SendAck(PhotonManager sender, PhotonActorNumber actorNumber, Guid guid, bool alreadyHasPortrait)
		{
			PFLog.Net.Log($"[CharGenPortraitSyncer.SendCommandAck] {actorNumber}, {guid}, {alreadyHasPortrait}");
			ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(new SaveMetaAcknowledgeData
			{
				PortraitsGuid = new SaveMetaAcknowledgeData.GuidData[1]
				{
					new SaveMetaAcknowledgeData.GuidData
					{
						Guid = guid,
						AlreadyHave = alreadyHasPortrait
					}
				}
			});
			return sender.SendMessageTo(actorNumber, 41, byteArraySlice.Buffer, byteArraySlice.Offset, byteArraySlice.Count);
		}
	}
}
