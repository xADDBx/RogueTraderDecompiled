using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking.Platforms.Session;

namespace Kingmaker.Networking.NetGameFsm;

public interface INetGame
{
	JoinableUserTypes CurrentJoinableUserType { get; }

	InvitableUserTypes CurrentInvitableUserType { get; }

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

	void StopPlaying(bool shouldLeaveLobby, string reason);

	void OnPlayingStopped();
}
