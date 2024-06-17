using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking.Tools;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Replay;
using Kingmaker.Utility.Fsm;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Networking.NetGameFsm.States;

internal class UploadAndStartLoadingState : StateLongAsync
{
	public class Args
	{
		public readonly SaveInfoKey SaveInfoKey;

		public readonly Action Callback;

		public Args(SaveInfoKey saveInfoKey, [CanBeNull] Action callback)
		{
			SaveInfoKey = saveInfoKey;
			Callback = callback;
		}
	}

	private readonly SaveInfoKey m_SaveInfoKey;

	private readonly Action m_Callback;

	private readonly INetGame m_NetGame;

	public UploadAndStartLoadingState([NotNull] Args args, INetGame netGame)
	{
		m_SaveInfoKey = args.SaveInfoKey;
		m_Callback = args.Callback;
		m_NetGame = netGame;
	}

	public override async Task OnExit()
	{
		await base.OnExit();
		PhotonManager.Save.ClearState();
	}

	protected override async Task DoAction(CancellationToken cancellationToken)
	{
		FakeLoadingProcessCoroutine saveLoadingBetweenSaveCreationAndUpload = null;
		try
		{
			SaveInfoKey saveInfoKey = m_SaveInfoKey;
			if (saveInfoKey.IsEmpty)
			{
				saveInfoKey = await CreateSave(cancellationToken);
				saveLoadingBetweenSaveCreationAndUpload = new FakeLoadingProcessCoroutine(LoadingProcessTag.Save);
			}
			SaveInfo saveInfo = await GetSave(saveInfoKey, cancellationToken);
			LobbyNetManager.SetState(LobbyNetManager.State.Playing);
			saveLoadingBetweenSaveCreationAndUpload?.Dispose();
			saveLoadingBetweenSaveCreationAndUpload = null;
			await UploadAndLoadGameAsync(saveInfo, m_Callback, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			LobbyNetManager.SetFirstLoadCompleted(firstLoadCompleted: true);
			Kingmaker.Replay.Replay.SetSaveData(saveInfo.SaveId);
			PhotonManager.OnStartCoopSession(saveInfo.SaveId);
			m_NetGame.StartPlaying();
		}
		finally
		{
			saveLoadingBetweenSaveCreationAndUpload?.Dispose();
		}
	}

	protected override async void OnActionException(Exception exception)
	{
		base.OnActionException(exception);
		PFLog.Net.Exception(exception);
		await Awaiters.UnityThread;
		bool shouldLeaveLobby = true;
		if (!(exception is SendMessageFailException))
		{
			if (exception is SaveNotFoundException)
			{
				shouldLeaveLobby = false;
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
				h.HandleSendMessageFailError();
			});
		}
		m_NetGame.StopPlaying(shouldLeaveLobby);
	}

	private static async Task<SaveInfoKey> CreateSave(CancellationToken cancellationToken)
	{
		PFLog.Net.Log("Creating new save...");
		Game.Instance.SaveManager.UpdateSaveListAsync();
		while (!Game.Instance.SaveManager.AreSavesUpToDate)
		{
			await Task.Delay(16, cancellationToken);
		}
		SaveInfo saveInfo = Game.Instance.SaveManager.FirstOrDefault(SaveManager.IsCoopSave) ?? Game.Instance.SaveManager.CreateNewSave("net_save_4b31b8ad92353a02");
		saveInfo.Type = SaveInfo.SaveType.Coop;
		SaveInfoKey saveInfoKey = (SaveInfoKey)saveInfo;
		cancellationToken.ThrowIfCancellationRequested();
		Game.Instance.SaveGame(saveInfo, Utils.CallbackToTask(out var task));
		await task;
		return saveInfoKey;
	}

	private static async Task<SaveInfo> GetSave(SaveInfoKey saveInfoKey, CancellationToken cancellationToken)
	{
		PFLog.Net.Log("Searching for save...");
		Game.Instance.SaveManager.UpdateSaveListAsync();
		while (!Game.Instance.SaveManager.AreSavesUpToDate)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await Task.Yield();
		}
		LobbyNetManager.SetState(LobbyNetManager.State.Loading);
		if (!Game.Instance.SaveManager.TryFind(saveInfoKey, out var saveInfo))
		{
			throw new SaveNotFoundException($"Save was not found! '{saveInfoKey}'");
		}
		return saveInfo;
	}

	private static async Task UploadAndLoadGameAsync([NotNull] SaveInfo saveInfo, [CanBeNull] Action action, CancellationToken cancellationToken)
	{
		using FakeLoadingProcessCoroutine loadingCoroutine = new FakeLoadingProcessCoroutine();
		uint randomNoise = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		await PhotonManager.Save.UploadSave(saveInfo, randomNoise, cancellationToken);
		PFStatefulRandom.OverrideRandomNoise(randomNoise);
		Game.Instance.LoadGameLocal(saveInfo, Utils.CallbackToTask(out var task));
		loadingCoroutine.Dispose();
		await task.OrCancelledBy(cancellationToken);
		action?.Invoke();
	}
}
