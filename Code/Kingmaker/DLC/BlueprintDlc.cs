using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.ResourceLinks;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("49e0ff77d3eb4701bcd01d4c87089c43")]
public class BlueprintDlc : BlueprintScriptableObject, IBlueprintDlc
{
	[SerializeField]
	private LocalizedString m_DisplayName;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private BlueprintDlcRewardReference[] m_RewardReferences;

	[Header("Default Store Data")]
	[SerializeField]
	private SpriteLink m_DefaultKeyArtLink;

	public LocalizedString DefaultTitle;

	public LocalizedString DefaultDescription;

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

	public bool IsAvailable
	{
		get
		{
			IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(this);
			if (iDLCStatus == null)
			{
				PFLog.System.Error($"DLC {this} has no status in the DLCCache. Defaulting to unavailable.");
			}
			bool result = iDLCStatus != null && iDLCStatus.Purchased && iDLCStatus.IsMounted && iDLCStatus.IsEnabled;
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

	public IEnumerable<IDlcStore> GetDlcStores()
	{
		return this.GetComponents<DlcStore>();
	}
}
