using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking.Tools;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Replay;
using Kingmaker.Utility.Fsm;
using Kingmaker.Utility.Random;

namespace Kingmaker.Networking.NetGameFsm.States;

public class SaveReceivingState : StateLongAsync
{
	private readonly INetGame m_NetGame;

	private readonly PhotonActorNumber m_SaveFromPlayer;

	public SaveReceivingState(PhotonActorNumber saveFromPlayer, INetGame netGame)
	{
		m_NetGame = netGame;
		m_SaveFromPlayer = saveFromPlayer;
	}

	public override async Task OnExit()
	{
		await base.OnExit();
		PhotonManager.Save.ClearState();
	}

	protected override async Task DoAction(CancellationToken cancellationToken)
	{
		LobbyNetManager.SetState(LobbyNetManager.State.Loading);
		LobbyNetManager.SetState(LobbyNetManager.State.Playing);
		string text = await DownloadSave(m_SaveFromPlayer, cancellationToken);
		LobbyNetManager.SetFirstLoadCompleted(firstLoadCompleted: true);
		Kingmaker.Replay.Replay.SetSaveData(text);
		PhotonManager.OnStartCoopSession(text);
		m_NetGame.StartPlaying();
	}

	protected override async void OnActionException(Exception exception)
	{
		base.OnActionException(exception);
		PFLog.Net.Exception(exception);
		await Awaiters.UnityThread;
		bool shouldLeaveLobby = true;
		if (!(exception is SendMessageFailException))
		{
			if (!(exception is SaveSourceDisconnectedException))
			{
				if (!(exception is SaveReceiveException))
				{
					if (exception is SaveNotFoundException)
					{
						EventBus.RaiseEvent(delegate(INetSaveUploadDownloadErrorHandler h)
						{
							h.HandleSaveNotFoundError();
						});
					}
					else
					{
						EventBus.RaiseEvent(delegate(INetSaveUploadDownloadErrorHandler h)
						{
							h.HandleUnknownException();
						});
					}
				}
				else
				{
					EventBus.RaiseEvent(delegate(INetSaveUploadDownloadErrorHandler h)
					{
						h.HandleSaveReceiveError();
					});
				}
			}
			else
			{
				shouldLeaveLobby = false;
				EventBus.RaiseEvent(delegate(INetSaveUploadDownloadErrorHandler h)
				{
					h.HandleSaveSourceDisconnectedError();
				});
			}
		}
		else
		{
			EventBus.RaiseEvent(delegate(INetSaveUploadDownloadErrorHandler h)
			{
				h.HandleSendMessageFailError();
			});
		}
		m_NetGame.StopPlaying(shouldLeaveLobby);
	}

	private static async Task<string> DownloadSave(PhotonActorNumber saveFromPlayer, CancellationToken cancellationToken)
	{
		using (FakeLoadingProcessCoroutine loadingCoroutine = new FakeLoadingProcessCoroutine())
		{
			SaveNetManager.SaveReceiveData data = await PhotonManager.Save.DownloadSave(saveFromPlayer, cancellationToken);
			SaveInfo saveInfo = await GetSave(data.SaveInfoKey, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			PFStatefulRandom.OverrideRandomNoise(data.RandomNoise);
			Game.Instance.LoadGameLocal(saveInfo, Utils.CallbackToTask(out var task));
			loadingCoroutine.Dispose();
			await task.OrCancelledBy(cancellationToken);
			return data.SaveInfoKey.Id;
		}
		static async Task<SaveInfo> GetSave(SaveInfoKey key, CancellationToken token)
		{
			Game.Instance.SaveManager.UpdateSaveListAsync();
			while (!Game.Instance.SaveManager.AreSavesUpToDate)
			{
				await Task.Delay(16, token);
			}
			if (!Game.Instance.SaveManager.TryFind(key, out var saveInfo2))
			{
				throw new SaveNotFoundException("Save not found: " + key);
			}
			if (!SaveManager.IsCoopSave(saveInfo2))
			{
				throw new SaveNotFoundException($"Invalid save type {saveInfo2.Type}");
			}
			return saveInfo2;
		}
	}
}
