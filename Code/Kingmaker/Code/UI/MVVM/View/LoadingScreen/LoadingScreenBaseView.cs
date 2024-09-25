using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Legacy.LoadingScreen;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.LoadingScreen;

public class LoadingScreenBaseView : ViewBase<LoadingScreenVM>
{
	[Serializable]
	public class SettingTypeScreens
	{
		public BlueprintArea.SettingType Type;

		public List<LoadingScreenImage> Sprites;
	}

	private static readonly int ThresholdIndex = Shader.PropertyToID("_Threshold");

	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenHints m_Hints;

	[SerializeField]
	[UsedImplicitly]
	private List<Image> m_Points;

	[Header("Content")]
	[SerializeField]
	[UsedImplicitly]
	private GameObject m_MapContainer;

	[SerializeField]
	[UsedImplicitly]
	private Image BigArt;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenGlitchAnimator m_GlitchAnimator;

	[Header("LoadingSprites")]
	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenImage m_KeyArtTuple;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenImage m_WarpThemedScreenTuple;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenImage m_BridgeScreenshotTuple;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenImage m_StarSystemScreenTuple;

	[SerializeField]
	[UsedImplicitly]
	private LoadingScreenImage m_GlobalMapScreenTuple;

	[SerializeField]
	[UsedImplicitly]
	private List<LoadingScreenImage> m_SpaceShipScreenTuples;

	[SerializeField]
	[UsedImplicitly]
	private List<SettingTypeScreens> m_SettingTypeScreensList;

	[Header("Character")]
	[SerializeField]
	[UsedImplicitly]
	private Image m_CharacterPortrait;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_CharacterNameText;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_CharacterDescriptionText;

	[SerializeField]
	[UsedImplicitly]
	private Image m_CharacterWeapon;

	[Header("Variables")]
	[SerializeField]
	[UsedImplicitly]
	private float m_LoadingDissolveTime = 2f;

	[SerializeField]
	[UsedImplicitly]
	private float m_HidingDissolveTime = 0.5f;

	[SerializeField]
	[UsedImplicitly]
	private float m_MinDissolveStep = 0.02f;

	[SerializeField]
	[UsedImplicitly]
	private float m_MaxDissolveStep = 0.1f;

	[SerializeField]
	[UsedImplicitly]
	private float m_HidingMaxDissolveStep = 0.3f;

	[Header("Progress")]
	[SerializeField]
	private CanvasGroup m_ProgressBarContainer;

	[SerializeField]
	[UsedImplicitly]
	private Image ProgressImage;

	[SerializeField]
	private CanvasGroup m_ProgressPercentContainer;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI ProgressPercent;

	[Header("SaveTransfer")]
	[SerializeField]
	private CanvasGroup m_TransferSaveProgressBarContainer;

	[SerializeField]
	private Image m_TransferSaveProgress;

	[SerializeField]
	private CanvasGroup m_TransferSaveCountersContainer;

	[SerializeField]
	private TextMeshProUGUI m_TransferSaveProgressPercent;

	[SerializeField]
	private TextMeshProUGUI m_TransferSaveProgressSize;

	[Header("WaitForInput")]
	[SerializeField]
	private CanvasGroup m_WaitForUserInputContainer;

	[SerializeField]
	private TextMeshProUGUI m_WaitForUserInputText;

	[Header("Main")]
	[SerializeField]
	private GameObject m_CharacterContainer;

	[Header("Main")]
	[SerializeField]
	private GameObject m_MainContainer;

	[SerializeField]
	private GameObject m_BottomTitleObject;

	[SerializeField]
	private GameObject m_BottomDescriptionObject;

	[SerializeField]
	protected TextMeshProUGUI m_BottomTitleText;

	[SerializeField]
	protected TextMeshProUGUI m_BottomDescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_LocationName;

	[Header("Random")]
	[SerializeField]
	private int[] m_RandomTwoEqualPercents = new int[3] { 0, 50, 50 };

	[SerializeField]
	private int[] RandomTwoWithPriorityPercents = new int[3] { 0, 60, 40 };

	[SerializeField]
	private int[] RandomThreeWithPriorityPercents = new int[4] { 0, 40, 30, 30 };

	[SerializeField]
	private int[] RandomFourWithPriorityPercents = new int[5] { 0, 40, 20, 20, 20 };

	[SerializeField]
	private int[] RandomFiveWithPriorityPercents = new int[6] { 0, 40, 15, 15, 15, 15 };

	private bool m_IsInit;

	private bool m_ShowDissolve;

	private float m_CurrentThreshold;

	private float m_CurrentTime;

	private Sequence m_PointSequence;

	private float m_Progress;

	private float m_VirtualProgress;

	private Coroutine m_ProgressCoroutine;

	private int m_LoadingScreenType;

	private readonly LoadingScreenHints.LocationEnum m_LocationHints;

	private readonly LoadingScreenHints.LocationEnum m_BridgeHints = LoadingScreenHints.LocationEnum.BridgeHints;

	private readonly LoadingScreenHints.LocationEnum m_StarSystemHints = LoadingScreenHints.LocationEnum.StarSystemHints;

	private readonly LoadingScreenHints.LocationEnum m_GlobalMapHints = LoadingScreenHints.LocationEnum.GlobalMapHints;

	private readonly LoadingScreenHints.LocationEnum m_SpaceCombatHints = LoadingScreenHints.LocationEnum.SpaceCombatHints;

	private readonly LoadingScreenHints.LocationEnum m_MainMenuHints = LoadingScreenHints.LocationEnum.MainMenuHints;

	private Tween m_WaitForInputLoopAnimation;

	private LoadingScreenImage? m_LoaddedTuple;

	private bool m_IsCharacterScreen;

	private BaseUnitEntity m_CompanionOnLoadingScreen;

	private const string CAN_SWITCH_DLC_AFTER_PURCHASE_PREF_KEY = "first_open_can_switch_dlc_after_purchase";

	public static bool CanSwitchDlcAfterPurchaseShown => PlayerPrefs.GetInt("first_open_can_switch_dlc_after_purchase", 0) == 1;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			base.gameObject.SetActive(value: false);
			m_FadeAnimator.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.AreaProperty.Subscribe(SetupLoadingArea));
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		Show();
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			UpdateThreshold();
		}));
		AddDisposable(base.ViewModel.NeedUserInput.Subscribe(ShowUserInputWaiting));
		AddDisposable(base.ViewModel.IsSaveTransfer.Subscribe(delegate(bool value)
		{
			m_TransferSaveProgressBarContainer.gameObject.SetActive(value);
			m_TransferSaveCountersContainer.gameObject.SetActive(value);
			m_ProgressBarContainer.gameObject.SetActive(!value);
			m_ProgressPercentContainer.gameObject.SetActive(!value);
			m_WaitForUserInputContainer.gameObject.SetActive(!value && base.ViewModel.NeedUserInput.Value);
		}));
		AddDisposable(base.ViewModel.SaveTransferProgress.CombineLatest(base.ViewModel.SaveTransferTarget, (int progress, int target) => new { progress, target }).Subscribe(transfer =>
		{
			if (transfer.target == 0 || !base.ViewModel.IsSaveTransfer.Value)
			{
				m_TransferSaveProgressSize.text = string.Empty;
				m_TransferSaveProgressPercent.text = string.Empty;
				m_TransferSaveProgress.fillAmount = 0f;
			}
			else
			{
				m_TransferSaveProgress.fillAmount = (float)transfer.progress / (float)transfer.target;
				m_TransferSaveProgressSize.text = $"{transfer.progress / 1024}/{transfer.target / 1024} KB";
				m_TransferSaveProgressPercent.text = $"{100f * ((float)transfer.progress / (float)transfer.target):00}%";
			}
		}));
		AddDisposable(base.ViewModel.UserInputProgress.CombineLatest(base.ViewModel.UserInputTarget, base.ViewModel.UserInputMeIsPressed, (int progress, int target, bool me) => new { progress, target, me }).Subscribe(value =>
		{
			if (!value.me)
			{
				m_WaitForUserInputText.text = (Game.Instance.IsControllerMouse ? UIStrings.Instance.CommonTexts.PressAnyKey : UIStrings.Instance.CommonTexts.PressAnyKeyConsole);
			}
			else
			{
				m_WaitForUserInputContainer.gameObject.SetActive(value: true);
				m_WaitForUserInputContainer.alpha = 1f;
				m_WaitForUserInputText.text = string.Format(UIStrings.Instance.CommonTexts.WaitingOtherPlayer, value.progress, value.target);
			}
		}));
		SetTextFontSize(base.ViewModel.FontMultiplier);
	}

	protected override void DestroyViewImplementation()
	{
		if (!Game.Instance.IsSpaceCombat && PhotonManager.Lobby.IsActive && !PhotonManager.NetGame.NetRolesShowed && Game.Instance.SceneLoader.LoadedUIScene != GameScenes.MainMenu)
		{
			EventBus.RaiseEvent(delegate(INetRolesRequest h)
			{
				h.HandleNetRolesRequest();
			});
			PhotonManager.NetGame.NetRolesShowed = true;
		}
		ShowCanSwitchDlcAfterPurchase();
		EventBus.RaiseEvent(delegate(INetEvents h)
		{
			h.HandleNLoadingScreenClosed();
		});
		m_GlitchAnimator.StartGlitch(Hide);
		if (m_LoaddedTuple.HasValue)
		{
			m_LoaddedTuple?.Main.ForceUnload();
			m_LoaddedTuple?.Glitch.ForceUnload();
			m_LoaddedTuple = null;
		}
	}

	private void ShowCanSwitchDlcAfterPurchase()
	{
		if (PhotonManager.Lobby.IsActive)
		{
			return;
		}
		List<BlueprintDlc> list = (from d in Game.Instance?.Player?.GetAvailableAdditionalContentDlcForCurrentCampaign()
			select d as BlueprintDlc into dlc
			where dlc != null && dlc.DlcType == DlcTypeEnum.AdditionalContentDlc
			select dlc).ToList();
		if (CanSwitchDlcAfterPurchaseShown || list == null || list.Count <= 0 || !list.Any((BlueprintDlc dlc) => !dlc.IsEnabled))
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(m_Hints.NewPurchasedDLCHint, DialogMessageBoxBase.BoxType.Checkbox, delegate(DialogMessageBoxBase.BoxButton btn)
			{
				if (btn == DialogMessageBoxBase.BoxButton.Yes)
				{
					EventBus.RaiseEvent(delegate(IDlcManagerUIHandler h)
					{
						h.HandleOpenDlcManager(inGame: true);
					});
				}
			}, null, UIStrings.Instance.SettingsUI.DialogOk, UIStrings.Instance.SettingsUI.DialogCancel, null, null, null, 0, uint.MaxValue, null, null, SetCanSwitchDlcAfterPurchasePrefs);
		});
	}

	protected virtual void SetTextFontSize(float multiplier)
	{
	}

	private void Show()
	{
		m_GlitchAnimator.ClearGlitch();
		m_ProgressBarContainer.alpha = 1f;
		m_ProgressPercentContainer.alpha = 1f;
		KillWaitUserInputAnimation();
		Game.Instance.ResetLoadingProgress();
		base.ViewModel.State = LoadingScreenState.ShowAnimation;
		m_FadeAnimator.AppearAnimation(delegate
		{
			base.ViewModel.State = LoadingScreenState.Shown;
		});
		m_PointSequence = DOTween.Sequence();
		m_PointSequence.Play().SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Restart);
		m_CurrentThreshold = 1f;
		if (!m_ShowDissolve)
		{
			m_CurrentThreshold = 1f;
		}
		m_ProgressCoroutine = StartCoroutine(LoadingProgressCoroutine());
	}

	private void Hide()
	{
		KillWaitUserInputAnimation();
		if (base.ViewModel != null)
		{
			base.ViewModel.State = LoadingScreenState.HideAnimation;
		}
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
			if (base.ViewModel != null)
			{
				base.ViewModel.State = LoadingScreenState.Hidden;
			}
		});
		m_CurrentTime = 0f;
		m_ShowDissolve = false;
		m_PointSequence.Kill();
		m_PointSequence = null;
		Game.Instance.ResetLoadingProgress();
		if (m_ProgressCoroutine != null)
		{
			StopCoroutine(m_ProgressCoroutine);
		}
		SetProgress(1f);
		m_Progress = 0f;
		m_VirtualProgress = 0f;
		if (m_IsCharacterScreen)
		{
			RootUIContext.Instance.PreviousLoadingScreenCompanion = m_CompanionOnLoadingScreen;
		}
	}

	private void KillWaitUserInputAnimation()
	{
		m_WaitForUserInputContainer.DOKill();
		DOTween.Kill(m_WaitForUserInputContainer);
		m_WaitForInputLoopAnimation?.Kill();
		m_WaitForInputLoopAnimation = null;
		m_WaitForUserInputContainer.alpha = 0f;
		m_WaitForUserInputContainer.gameObject.SetActive(value: false);
	}

	private void SetupLoadingArea(BlueprintArea area)
	{
		SwitchLoadingScreen(1);
		if (area == null)
		{
			if (m_LocationName != null)
			{
				m_LocationName.transform.parent.gameObject.SetActive(value: false);
			}
			ShowEmptyAreaScreen();
			return;
		}
		if (area.IsShipArea)
		{
			ShowSpaceAreaScreen(area);
		}
		else if (area == Game.Instance.Player.CombatRandomEncounterState.Area)
		{
			ShowWarpAreaScreen(area);
		}
		else if (area.name == "VoidshipBridge")
		{
			ShowVoidshipAreaScreen(area);
		}
		else
		{
			ShowClassicAreaScreen(area);
		}
		m_ShowDissolve = true;
		m_CurrentThreshold = 1f;
		if (!(m_LocationName == null))
		{
			m_LocationName.transform.parent.gameObject.SetActive(!string.IsNullOrWhiteSpace(area.AreaDisplayName) || !string.IsNullOrWhiteSpace(area.Name));
			if (!string.IsNullOrWhiteSpace(area.AreaDisplayName) || !string.IsNullOrWhiteSpace(area.Name))
			{
				m_LocationName.text = ((!string.IsNullOrWhiteSpace(area.AreaDisplayName)) ? area.AreaDisplayName : area.Name);
			}
		}
	}

	private void ShowEmptyAreaScreen()
	{
		m_LoadingScreenType = base.ViewModel.RandomLoadingScreen(3, RandomThreeWithPriorityPercents);
		if (m_LoadingScreenType == 0)
		{
			ShowCompanionScreen(m_MainMenuHints);
			return;
		}
		LoadingScreenImage mainSprites = ((m_LoadingScreenType == 1) ? m_BridgeScreenshotTuple : m_KeyArtTuple);
		SetMainSprites(mainSprites);
		m_BottomTitleObject.SetActive(value: false);
		m_BottomTitleText.gameObject.SetActive(value: false);
		m_BottomDescriptionObject.SetActive(value: true);
		m_BottomDescriptionText.text = m_Hints.TakeHint(m_MainMenuHints, base.ViewModel.Random);
	}

	private void ShowSpaceAreaScreen(BlueprintArea area)
	{
		m_LoadingScreenType = ((area.AreaStatGameMode == GameModeType.GlobalMap) ? base.ViewModel.RandomLoadingScreen(4, RandomFourWithPriorityPercents) : base.ViewModel.RandomLoadingScreen(5, RandomFiveWithPriorityPercents));
		switch (m_LoadingScreenType)
		{
		case 0:
			ShowClassicAreaScreen(area);
			return;
		case 1:
			ShowCompanionScreen((area.AreaStatGameMode == GameModeType.StarSystem) ? m_StarSystemHints : ((area.AreaStatGameMode == GameModeType.SpaceCombat) ? m_SpaceCombatHints : m_GlobalMapHints));
			return;
		}
		LoadingScreenImage mainSprites = ((area.AreaStatGameMode == GameModeType.StarSystem) ? (m_LoadingScreenType switch
		{
			2 => m_KeyArtTuple, 
			3 => m_WarpThemedScreenTuple, 
			4 => m_StarSystemScreenTuple, 
			_ => m_SpaceShipScreenTuples.Random(PFStatefulRandom.UI), 
		}) : ((!(area.AreaStatGameMode == GameModeType.GlobalMap)) ? (m_LoadingScreenType switch
		{
			2 => m_BridgeScreenshotTuple, 
			3 => m_KeyArtTuple, 
			4 => m_WarpThemedScreenTuple, 
			_ => m_SpaceShipScreenTuples.Random(PFStatefulRandom.UI), 
		}) : (m_LoadingScreenType switch
		{
			2 => m_KeyArtTuple, 
			3 => m_BridgeScreenshotTuple, 
			_ => m_GlobalMapScreenTuple, 
		})));
		SetMainSprites(mainSprites);
		StandartDescriptionOrHint(area, (area.AreaStatGameMode == GameModeType.StarSystem) ? m_StarSystemHints : ((area.AreaStatGameMode == GameModeType.GlobalMap) ? m_GlobalMapHints : m_SpaceCombatHints));
	}

	private void ShowWarpAreaScreen(BlueprintArea area)
	{
		m_LoadingScreenType = base.ViewModel.RandomLoadingScreen(2, RandomTwoWithPriorityPercents);
		if (m_LoadingScreenType == 0)
		{
			LocationScreen(area, m_WarpThemedScreenTuple);
		}
		else if (m_LoadingScreenType == 1)
		{
			SetMainSprites(m_WarpThemedScreenTuple);
		}
		StandartDescriptionOrHint(area, m_LocationHints);
	}

	private void ShowVoidshipAreaScreen(BlueprintArea area)
	{
		m_LoadingScreenType = base.ViewModel.RandomLoadingScreen(3, RandomFourWithPriorityPercents);
		if (m_LoadingScreenType == 1)
		{
			ShowCompanionScreen(m_BridgeHints);
			return;
		}
		if (m_LoadingScreenType == 0)
		{
			ShowClassicAreaScreen(area);
		}
		else
		{
			SetMainSprites((m_LoadingScreenType == 2) ? m_BridgeScreenshotTuple : m_KeyArtTuple);
		}
		StandartDescriptionOrHint(area, m_BridgeHints);
	}

	private void ShowClassicAreaScreen(BlueprintArea area)
	{
		LoadingScreenImage mainSprites = (area.LoadingScreenSprites.Any() ? new LoadingScreenImage?(area.LoadingScreenSprites.Random(PFStatefulRandom.UI)) : ((area.ArtSetting == BlueprintArea.SettingType.Unspecified) ? new LoadingScreenImage?(m_KeyArtTuple) : m_SettingTypeScreensList.FirstItem((SettingTypeScreens s) => s.Type == area.ArtSetting)?.Sprites.Random(PFStatefulRandom.UI))) ?? m_KeyArtTuple;
		StandartDescriptionOrHint(area, m_LocationHints);
		if (area.LoadingScreenSprites.Any() || area.ArtSetting != 0)
		{
			SetMainSprites(mainSprites);
			return;
		}
		m_LoadingScreenType = base.ViewModel.RandomLoadingScreen(2, RandomTwoWithPriorityPercents);
		if (m_LoadingScreenType == 0)
		{
			SetMainSprites(mainSprites);
		}
		else if (m_LoadingScreenType == 1)
		{
			ShowCompanionScreen(m_LocationHints);
		}
	}

	private void SwitchLoadingScreen(int screen)
	{
		m_MainContainer.SetActive(screen == 1);
		m_CharacterContainer.SetActive(screen == 2);
		m_IsCharacterScreen = screen == 2;
	}

	private void LocationScreen(BlueprintArea area, LoadingScreenImage keySprite)
	{
		LoadingScreenImage mainSprites = keySprite;
		if (area.LoadingScreenSprites.Any())
		{
			mainSprites = area.LoadingScreenSprites.Random(PFStatefulRandom.UI);
		}
		else if (area.ArtSetting != 0)
		{
			mainSprites = m_SettingTypeScreensList.FirstItem((SettingTypeScreens s) => s.Type == area.ArtSetting)?.Sprites.Random(PFStatefulRandom.UI) ?? keySprite;
		}
		SetMainSprites(mainSprites);
	}

	private void StandartKeyArtAndHint(LoadingScreenHints.LocationEnum locationEnum)
	{
		SwitchLoadingScreen(1);
		m_BottomTitleObject.SetActive(value: false);
		m_BottomTitleText.gameObject.SetActive(value: false);
		m_BottomDescriptionObject.SetActive(value: true);
		m_BottomDescriptionText.text = m_Hints.TakeHint(locationEnum, base.ViewModel.Random);
		SetMainSprites(m_KeyArtTuple);
	}

	private void StandartDescriptionOrHint(BlueprintArea area, LoadingScreenHints.LocationEnum locationEnum)
	{
		bool flag = string.IsNullOrWhiteSpace(area.Description);
		bool flag2 = string.IsNullOrWhiteSpace(area.AreaDisplayName) && string.IsNullOrWhiteSpace(area.Name);
		m_BottomTitleObject.SetActive(value: false);
		m_BottomTitleText.gameObject.SetActive(value: false);
		m_BottomDescriptionObject.SetActive(value: true);
		m_BottomDescriptionText.text = ((!flag && !flag2) ? area.Description : m_Hints.TakeHint(locationEnum, base.ViewModel.Random));
	}

	private void ShowCompanionScreen(LoadingScreenHints.LocationEnum locationEnum)
	{
		SwitchLoadingScreen(2);
		List<BaseUnitEntity> list = Game.Instance.Player.RemoteCompanions.ToTempList();
		List<BaseUnitEntity> actualCompanionsGroup = Game.Instance.SelectionCharacter.ActualGroup;
		list.AddRange(Game.Instance.Player.Party.Where((BaseUnitEntity pc) => pc != Game.Instance.Player.MainCharacterEntity && !pc.IsPet && !pc.IsCustomCompanion() && actualCompanionsGroup.Contains(pc)));
		if (list.Empty())
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		List<BaseUnitEntity> list2 = list.Where((BaseUnitEntity c) => c.Portrait.LoadingPortrait != null).ToList();
		list2 = ((list2.Count > 1) ? list2.Where((BaseUnitEntity c) => c.Blueprint != RootUIContext.Instance.PreviousLoadingScreenCompanion?.Blueprint).ToList() : list2);
		if (list2.Empty())
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		CompanionStoriesManager companionsStories = Game.Instance.Player.CompanionStories;
		list2 = list2.Where(delegate(BaseUnitEntity c)
		{
			BlueprintCompanionStory blueprintCompanionStory2 = companionsStories.Get(c.ToBaseUnitEntity()).LastOrDefault();
			return blueprintCompanionStory2 != null && !string.IsNullOrWhiteSpace(blueprintCompanionStory2.Title) && !string.IsNullOrWhiteSpace(blueprintCompanionStory2.Description);
		}).ToList();
		if (list2.Empty())
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		UnitReference unitReference = list2[base.ViewModel.Random.Range(0, list2.Count)].FromBaseUnitEntity();
		if (unitReference == null)
		{
			StandartKeyArtAndHint(locationEnum);
			return;
		}
		BlueprintCompanionStory blueprintCompanionStory = companionsStories.Get(unitReference.Entity.ToBaseUnitEntity()).LastOrDefault();
		TextMeshProUGUI characterNameText = m_CharacterNameText;
		LocalizedString localizedString = blueprintCompanionStory?.Title;
		characterNameText.text = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		TextMeshProUGUI characterDescriptionText = m_CharacterDescriptionText;
		localizedString = blueprintCompanionStory?.Description;
		characterDescriptionText.text = ((localizedString != null) ? ((string)localizedString) : string.Empty);
		SetCharacterSprites(unitReference.Entity.ToBaseUnitEntity().Portrait.LoadingPortrait, unitReference.Entity.ToBaseUnitEntity().Portrait.LoadingGlitchPortrait);
		m_CompanionOnLoadingScreen = unitReference.Entity.ToBaseUnitEntity();
	}

	private void UpdateThreshold()
	{
		if (m_ShowDissolve)
		{
			m_CurrentTime += Time.unscaledDeltaTime;
			float num = ((base.ViewModel.State == LoadingScreenState.HideAnimation) ? m_HidingDissolveTime : m_LoadingDissolveTime);
			if (!(m_CurrentTime < num * m_MinDissolveStep))
			{
				m_CurrentTime = 0f;
				float minDissolveStep = m_MinDissolveStep;
				float max = ((base.ViewModel.State == LoadingScreenState.HideAnimation) ? m_HidingMaxDissolveStep : m_MaxDissolveStep);
				float num2 = Mathf.Clamp(m_CurrentTime / num, minDissolveStep, max);
				m_CurrentThreshold -= num2;
				m_CurrentThreshold = Mathf.Clamp01(m_CurrentThreshold);
			}
		}
	}

	private IEnumerator LoadingProgressCoroutine()
	{
		while (m_VirtualProgress < 1f)
		{
			while (Game.Instance.IsLoadingProgressPaused)
			{
				yield return null;
			}
			SetProgress(Game.Instance.UILoadingProgress);
			yield return null;
		}
	}

	private void SetProgress(float progress)
	{
		m_Progress = Mathf.Max(progress, m_Progress);
		m_VirtualProgress = ((m_Progress > m_VirtualProgress) ? m_Progress : (m_VirtualProgress + Time.unscaledDeltaTime / 300f));
		ProgressImage.fillAmount = m_VirtualProgress;
		float num = Mathf.Clamp(m_VirtualProgress * 100f, 0f, 100f);
		ProgressPercent.text = UIConfig.Instance.PercentHelper.AddPercentTo($"{num:0}");
	}

	protected virtual void ShowUserInputWaiting(bool state)
	{
		if (!state)
		{
			KillWaitUserInputAnimation();
			return;
		}
		UISounds.Instance.Play(UISounds.Instance.Sounds.LoadingScreen.WaitForUserInputShow, isButton: false, playAnyway: true);
		m_ProgressBarContainer.DOFade(0f, 1f).OnComplete(StartPressAnyKeyLoopAnimation).SetUpdate(isIndependentUpdate: true);
		m_ProgressPercentContainer.DOFade(0f, 1f).OnComplete(StartPressAnyKeyLoopAnimation).SetUpdate(isIndependentUpdate: true);
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			if (Input.anyKeyDown)
			{
				CloseWait();
			}
		}));
	}

	protected void CloseWait()
	{
		UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
		UISounds.Instance.Play(UISounds.Instance.Sounds.LoadingScreen.WaitForUserInputHide, isButton: false, playAnyway: true);
		if (PhotonManager.Lobby.IsLoading)
		{
			PhotonManager.Instance.ContinueLoading();
		}
		EventBus.RaiseEvent(delegate(IContinueLoadingHandler h)
		{
			h.HandleContinueLoading();
		});
	}

	private void StartPressAnyKeyLoopAnimation()
	{
		m_WaitForUserInputContainer.gameObject.SetActive(value: true);
		m_WaitForInputLoopAnimation = m_WaitForUserInputContainer.DOFade(1f, 0.8f).SetUpdate(isIndependentUpdate: true).SetLoops(-1, LoopType.Yoyo)
			.OnKill(delegate
			{
				m_WaitForUserInputContainer.alpha = 0f;
				m_WaitForUserInputContainer.gameObject.SetActive(value: false);
			})
			.SetAutoKill(autoKillOnCompletion: false);
	}

	private void SetMainSprites(LoadingScreenImage tuple)
	{
		m_LoaddedTuple = tuple;
		BigArt.sprite = tuple.Main.Load(ignorePreloadWarning: false, hold: true);
		m_GlitchAnimator.SetGlitchImage(tuple.Glitch.Load(ignorePreloadWarning: false, hold: true));
	}

	private void SetCharacterSprites(Sprite main, Sprite glitch)
	{
		m_CharacterPortrait.sprite = main;
		m_GlitchAnimator.SetGlitchImage(glitch);
	}

	[Cheat(Name = "clear_can_switch_dlc_after_purchase")]
	public static void ClearCanSwitchDlcAfterPurchasePrefs()
	{
		PlayerPrefs.SetInt("first_open_can_switch_dlc_after_purchase", 0);
		PlayerPrefs.Save();
	}

	[Cheat(Name = "set_can_switch_dlc_after_purchase")]
	public static void SetCanSwitchDlcAfterPurchasePrefs()
	{
		PlayerPrefs.SetInt("first_open_can_switch_dlc_after_purchase", 1);
		PlayerPrefs.Save();
	}
}
