using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Blueprints.Root;

[TypeId("c2a033e22d0c431fa1c6ee6ded847c56")]
public class BlueprintCampaign : BlueprintScriptableObject
{
	public LocalizedString Title;

	public LocalizedString Description;

	public SpriteLink KeyArtLink;

	public bool ComingSoon;

	public bool HideInRelease;

	public bool HideInUI;

	public bool ToBeContinued;

	public bool IsDungeon;

	public bool AllowMythicChange;

	public bool IsLongImportActivate;

	public bool UseVideo;

	[ShowIf("UseVideo")]
	[SerializeField]
	public VideoLink VideoLink;

	[ShowIf("UseVideo")]
	[AkEventReference]
	public string SoundStartEvent;

	[ShowIf("UseVideo")]
	[AkEventReference]
	public string SoundStopEvent;

	[SerializeField]
	private BlueprintAreaPresetReference m_StartGamePreset;

	[SerializeField]
	private BlueprintUnitReference[] m_Pregens;

	public bool IsMainGameContent;

	private bool? m_IsAvailable;

	private BlueprintDlcRewardCampaign m_DlcReward;

	private BlueprintDlcReference[] m_AdditionalContentDlc;

	[SerializeField]
	private bool m_IsImportRequired;

	[ShowIf("IsImportRequired")]
	public SaveImportSettings ImportFromMainCampaign;

	public SaveImportSettings[] ImportSettings;

	public bool IsVisible
	{
		get
		{
			if (BuildModeUtility.IsRelease)
			{
				return !HideInRelease;
			}
			return true;
		}
	}

	public Sprite KeyArt => KeyArtLink?.Load() ?? UIConfig.Instance.KeyArt;

	public VideoClip Video => VideoLink?.Load();

	public BlueprintAreaPreset StartGamePreset => m_StartGamePreset;

	public bool IsAvailable => (m_IsAvailable ?? (m_IsAvailable = IsMainGameContent || BlueprintRoot.Instance.DlcSettings.Dlcs.Where((IBlueprintDlc _dlc) => _dlc.IsAvailable).SelectMany((IBlueprintDlc _dlc) => _dlc.Rewards).Any((IBlueprintDlcReward _dlcReward) => _dlcReward is BlueprintDlcRewardCampaign blueprintDlcRewardCampaign && blueprintDlcRewardCampaign.Campaign == this))).Value;

	public BlueprintDlcRewardCampaign DlcReward => m_DlcReward ?? (m_DlcReward = (IsMainGameContent ? null : (BlueprintRoot.Instance.DlcSettings.Dlcs.SelectMany((IBlueprintDlc _dlc) => _dlc.Rewards).FirstOrDefault((IBlueprintDlcReward _dlcReward) => _dlcReward is BlueprintDlcRewardCampaign blueprintDlcRewardCampaign && blueprintDlcRewardCampaign.Campaign == this) as BlueprintDlcRewardCampaign)));

	public IEnumerable<BlueprintDlc> AdditionalContentDlc
	{
		get
		{
			if (m_AdditionalContentDlc == null)
			{
				List<BlueprintDlcReference> list = new List<BlueprintDlcReference>();
				foreach (IBlueprintDlc dlc in BlueprintRoot.Instance.DlcSettings.Dlcs)
				{
					if (dlc.Rewards.Any((IBlueprintDlcReward br) => br is BlueprintDlcRewardCampaignAdditionalContent blueprintDlcRewardCampaignAdditionalContent && blueprintDlcRewardCampaignAdditionalContent.Campaign == this) && dlc is BlueprintDlc bp && dlc.GetDlcStores().TryFind((IDlcStore x) => x.AllowsPurchase, out var _))
					{
						list.Add(bp.ToReference<BlueprintDlcReference>());
					}
				}
				m_AdditionalContentDlc = list.ToArray();
			}
			return m_AdditionalContentDlc?.Dereference();
		}
	}

	public bool IsImportRequired => m_IsImportRequired;

	public SaveImportSettings GetImportSettings(BlueprintCampaign campaign, bool newGame)
	{
		if (campaign == null)
		{
			campaign = Game.Instance.Player.Campaign;
		}
		return ImportSettings.FindOrDefault((SaveImportSettings importSettings) => importSettings.Campaign == campaign && importSettings.NewGameOnly == newGame);
	}

	public bool MayImport(bool newGame)
	{
		return Game.Instance.Player.Campaign.GetImportSettings(this, newGame) != null;
	}

	public void RecheckAvailability()
	{
		m_IsAvailable = null;
	}
}
