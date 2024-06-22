using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.Sound;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.DLC;

[TypeId("49e0ff77d3eb4701bcd01d4c87089c43")]
public class BlueprintDlc : BlueprintScriptableObject, IBlueprintDlc
{
	public enum DlcPurchaseState
	{
		Purchased,
		AvailableToPurchase,
		ComingSoon
	}

	[SerializeField]
	private LocalizedString m_DisplayName;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private BlueprintDlcRewardReference[] m_RewardReferences;

	[Header("Default Store Data")]
	[SerializeField]
	private SpriteLink m_DefaultKeyArtLink;

	[SerializeField]
	private SpriteLink m_DlcItemArtLink;

	public bool UseVideo;

	[ShowIf("UseVideo")]
	[SerializeField]
	private VideoLink m_DefaultVideoLink;

	[ShowIf("UseVideo")]
	[AkEventReference]
	public string SoundStartEvent;

	[ShowIf("UseVideo")]
	[AkEventReference]
	public string SoundStopEvent;

	public LocalizedString DefaultTitle;

	public LocalizedString DefaultDescription;

	[SerializeField]
	private DlcTypeEnum m_DlcType;

	[SerializeField]
	private BlueprintDlcReference m_ParentDlc;

	[SerializeField]
	private bool m_HideDlc;

	[Header("MainMenu")]
	[SerializeField]
	private VideoLink m_MainMenuBackgroundVideoLink;

	[SerializeField]
	private SpriteLink m_TopMonitorArtLink;

	[SerializeField]
	private SpriteLink m_BottomMonitorArtLink;

	[SerializeField]
	private AkStateReference m_MusicSetting;

	public string DlcDisplayName => m_DisplayName.Text;

	public string DlcDescription => m_Description.Text;

	public string Id => base.AssetGuidThreadSafe;

	public IEnumerable<IBlueprintDlcReward> Rewards
	{
		get
		{
			IEnumerable<IBlueprintDlcReward> enumerable = m_RewardReferences?.Dereference();
			return enumerable ?? Enumerable.Empty<IBlueprintDlcReward>();
		}
	}

	public Sprite DefaultKeyArt => m_DefaultKeyArtLink?.Load();

	public Sprite DlcItemArtLink => m_DlcItemArtLink?.Load();

	public VideoClip DefaultVideo => m_DefaultVideoLink?.Load();

	public bool IsEnabled
	{
		get
		{
			if (DlcType == DlcTypeEnum.AdditionalContentDlc)
			{
				return Game.Instance.Player.GetAdditionalContentDlcStatus(this);
			}
			return true;
		}
	}

	public bool IsAvailable
	{
		get
		{
			IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(this);
			if (iDLCStatus == null)
			{
				PFLog.System.Error($"DLC {this} has no status in the DLCCache. Defaulting to unavailable.");
			}
			bool result = iDLCStatus != null && iDLCStatus.Purchased && iDLCStatus.IsMounted;
			if ((bool)ContextData<DlcNetManager.LocalPlayerDlcCheck>.Current)
			{
				return result;
			}
			if (!PhotonManager.Lobby.IsActive)
			{
				return result;
			}
			if ((bool)ContextData<DlcExtension.LoadSaveDlcCheck>.Current)
			{
				return PhotonManager.DLC.DLCsInConnect.Contains(Id);
			}
			if (PhotonManager.Lobby.IsPlaying)
			{
				return PhotonManager.DLC.DLCsInGame.Contains(Id);
			}
			if (PhotonManager.Lobby.InLobby)
			{
				return PhotonManager.DLC.DLCsInConnect.Contains(Id);
			}
			return result;
		}
	}

	public bool IsActive
	{
		get
		{
			if (IsAvailable)
			{
				return IsEnabled;
			}
			return false;
		}
	}

	public bool IsPurchased => StoreManager.DLCCache.Get(this)?.Purchased ?? false;

	public DlcTypeEnum DlcType => m_DlcType;

	public BlueprintDlc ParentDlc => m_ParentDlc;

	public bool HideDlc => m_HideDlc;

	public VideoClip MainMenuBackgroundVideo => m_MainMenuBackgroundVideoLink?.Load();

	public Sprite TopMonitorArt => m_TopMonitorArtLink?.Load();

	public Sprite BottomMonitorArt => m_BottomMonitorArtLink?.Load();

	public AkStateReference MusicSetting => m_MusicSetting;

	public IEnumerable<IDlcStore> GetDlcStores()
	{
		return this.GetComponents<DlcStore>();
	}

	public string GetDlcName()
	{
		if (string.IsNullOrWhiteSpace(DlcDisplayName))
		{
			if (string.IsNullOrEmpty(DefaultTitle))
			{
				return name;
			}
			return DefaultTitle;
		}
		return DlcDisplayName;
	}

	public DlcPurchaseState GetPurchaseState()
	{
		IDlcStore dlcStore = GetDlcStores().FirstOrDefault((IDlcStore store) => store.IsSuitable);
		if ((dlcStore == null || dlcStore.ComingSoon) && !IsPurchased)
		{
			return DlcPurchaseState.ComingSoon;
		}
		if (!IsPurchased)
		{
			return DlcPurchaseState.AvailableToPurchase;
		}
		return DlcPurchaseState.Purchased;
	}

	public void SwitchDlcValue(bool value)
	{
		Game.Instance.Player.UpdateAdditionalContentDlcStatus(this, value);
		EventBus.RaiseEvent(delegate(INewGameSwitchOnOffDlcHandler h)
		{
			h.HandleNewGameSwitchOnOffDlc(this, value);
		});
	}

	public bool GetDlcSwitchOnOffState()
	{
		return IsEnabled;
	}
}
