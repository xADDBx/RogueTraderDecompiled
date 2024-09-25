using System;
using Kingmaker.Localization;
using Kingmaker.Networking.Platforms.Session;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UINetLobbyTexts
{
	public LocalizedString NetHeader;

	public LocalizedString ConnectingLabel;

	public LocalizedString CreateLobby;

	public LocalizedString JoinLobby;

	public LocalizedString JoinLobbyCodePlaceholder;

	public LocalizedString RegionHeader;

	public LocalizedString DisconnectLobby;

	public LocalizedString CopyLobbyId;

	public LocalizedString CopyLobbyIdHint;

	public LocalizedString PasteLobbyId;

	public LocalizedString Launch;

	public LocalizedString Reconnect;

	public LocalizedString ChooseSaveHeader;

	public LocalizedString ChooseSaveHint;

	public LocalizedString WillShowNotAllSavesBecauseOfDlc;

	public LocalizedString LaunchInGameHint;

	public LocalizedString DesyncWasDetected;

	public LocalizedString LeaveLobbyMessageBox;

	public LocalizedString InviteLobbyMessageBox;

	public LocalizedString LastPlayerLeftLobbyMessageBox;

	public LocalizedString NewPlayerJoinToActiveLobby;

	public LocalizedString NewPlayerJoinToLobby;

	public LocalizedString PlayerLeftRoomWarning;

	public LocalizedString KickMessage;

	public LocalizedString KickPlayerMessage;

	public LocalizedString WarningPlayerIsNotControlMainCharacter;

	public LocalizedString HowToPingCoopLabelPc;

	public LocalizedString HowToPingCoopLabelConsole;

	public LocalizedString CoopVer;

	public LocalizedString CoopVerTooltip;

	public LocalizedString CoopRegionTooltip;

	public LocalizedString CoopLobbyCodeTooltip;

	public LocalizedString ShowLobbyCode;

	public LocalizedString HideLobbyCode;

	public LocalizedString InvitePlayer;

	public LocalizedString InviteEpicGamesPlayer;

	public LocalizedString KickPlayer;

	public LocalizedString SelectPlayers;

	public LocalizedString UnselectPlayers;

	public LocalizedString SelectRegion;

	public LocalizedString HowToPlay;

	public LocalizedString NeedSameRegionAndCoopVer;

	public LocalizedString IsNotEnoughPlayersForGame;

	public LocalizedString ImpossibleToStartCoopGameInThisMoment;

	public LocalizedString NotAvailableInCoopMode;

	public LocalizedString ResetCurrentSave;

	public LocalizedString SignInToEpicGamesStore;

	public LocalizedString CanBeAProblemsWithMods;

	public LocalizedString NeedSaveForStartGame;

	public LocalizedString StoreOverlayIsNotAvailable;

	public LocalizedString PlayerHasNoDlcs;

	public LocalizedString YouAreTheHostNow;

	public LocalizedString BlockedPlayerInLobby;

	public LocalizedString CantJoinLobbyDuePrivacySettings;

	public LocalizedString ShowPlayerInformation;

	public LocalizedString ShowGamerCard;

	public LocalizedString DlcList;

	public LocalizedString HostsDlcList;

	public LocalizedString HostHasNoDlc;

	public LocalizedString CantChooseAnySavesBecauseOfDlc;

	public LocalizedString DlcSharedByHost;

	public LocalizedString JoinableUserTypesLabel;

	public LocalizedString InvitableUserTypesLabel;

	public LocalizedString UserTypeDropdownNoOne;

	public LocalizedString UserTypeDropdownAnyone;

	public LocalizedString UserTypeDropdownFriends;

	public LocalizedString UserTypeDropdownFriendsOfFriends;

	public LocalizedString UserTypeDropdownLeader;

	public string GetJoinableUserTypeLabel(JoinableUserTypes type)
	{
		return type switch
		{
			JoinableUserTypes.NoOne => UserTypeDropdownNoOne, 
			JoinableUserTypes.Anyone => UserTypeDropdownAnyone, 
			JoinableUserTypes.Friends => UserTypeDropdownFriends, 
			JoinableUserTypes.FriendsOfFriends => UserTypeDropdownFriendsOfFriends, 
			_ => string.Empty, 
		};
	}

	public string GetInvitableUserTypeLabel(InvitableUserTypes type)
	{
		return type switch
		{
			InvitableUserTypes.Anyone => UserTypeDropdownAnyone, 
			InvitableUserTypes.Leader => UserTypeDropdownLeader, 
			_ => string.Empty, 
		};
	}
}
