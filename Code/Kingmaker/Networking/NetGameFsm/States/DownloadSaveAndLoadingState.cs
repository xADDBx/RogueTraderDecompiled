using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Tools;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Replay;
using Kingmaker.Utility.Fsm;
using Kingmaker.Utility.Random;

namespace Kingmaker.Networking.NetGameFsm.States;

public class DownloadSaveAndLoadingState : StateLongAsync
{
	public class Args
	{
		public PhotonActorNumber SaveFromPlayer;

		[CanBeNull]
		public readonly FakeLoadingProcessCoroutine TransitionLoadingProcess;

		public Args(PhotonActorNumber saveFromPlayer, [CanBeNull] FakeLoadingProcessCoroutine transitionLoadingProcess)
		{
			SaveFromPlayer = saveFromPlayer;
			TransitionLoadingProcess = transitionLoadingProcess;
		}
	}

	private readonly INetGame m_NetGame;

	private readonly Args m_Args;

	public DownloadSaveAndLoadingState(Args args, INetGame netGame)
	{
		m_NetGame = netGame;
		m_Args = args;
	}

	public override async Task OnExit()
	{
		await base.OnExit();
		m_Args.TransitionLoadingProcess?.Dispose();
		PhotonManager.Save.ClearState();
	}

	protected override async Task DoAction(CancellationToken cancellationToken)
	{
		await Awaiters.UnityThread;
		LobbyNetManager.SetState(LobbyNetManager.State.Loading);
		LobbyNetManager.SetState(LobbyNetManager.State.Playing);
		m_Args.TransitionLoadingProcess?.Hide();
		string text = await DownloadSave(m_Args.SaveFromPlayer, cancellationToken);
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
		m_NetGame.StopPlaying(shouldLeaveLobby, "DownloadSaveAndLoadingState");
	}

	private static async Task<string> DownloadSave(PhotonActorNumber saveFromPlayer, CancellationToken cancellationToken)
	{
		using FakeLoadingProcessCoroutine loadingCoroutine = new FakeLoadingProcessCoroutine();
		SaveNetManager.SaveReceiveData saveReceiveData = await PhotonManager.Save.DownloadSave(saveFromPlayer, cancellationToken);
		SaveInfo saveInfo = SaveManager.LoadZipSave(SaveSystemJsonSerializer.Serializer, saveReceiveData.SaveFilePath);
		cancellationToken.ThrowIfCancellationRequested();
		PFStatefulRandom.OverrideRandomNoise(saveReceiveData.RandomNoise);
		Game.Instance.LoadGameLocal(saveInfo, Utils.CallbackToTask(out var task));
		loadingCoroutine.Dispose();
		await task.OrCancelledBy(cancellationToken);
		return saveInfo.SaveId;
	}
}
