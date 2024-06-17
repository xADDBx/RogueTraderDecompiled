using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Networking.NetGameFsm;

public interface INetGame
{
	void InitPlatform();

	void OnPlatformInitialized();

	void OnPlatformInitializeFailed();

	void InitNetSystem();

	void OnNetSystemInitialized();

	void OnNetSystemInitializeFailed(bool storeNotInitialized);

	void ChangeRegion(string region);

	void OnRegionChanged();

	void OnRegionChangeFailed();

	void CreateNewLobby();

	void OnLobbyCreated();

	void OnLobbyCreationFailed();

	void Join(string roomName);

	void OnLobbyJoined();

	void OnLobbyJoinFailed();

	void OnSaveReceived(PhotonActorNumber saveFromPlayer);

	bool StartGame(SaveInfoKey saveInfoKey, [CanBeNull] Action callback = null);

	bool StartGameWithoutSave();

	void StartPlaying();

	void StopPlaying(bool shouldLeaveLobby);

	void OnPlayingStopped();
}
