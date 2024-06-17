using System;
using System.Collections;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ActionBar.PC;
using Kingmaker.Code.UI.MVVM.View.IngameMenu.PC;
using Kingmaker.Code.UI.MVVM.View.Inspect;
using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.MVVM.View.CombatLog.PC;
using Kingmaker.UI.MVVM.View.SurfaceCombat.PC;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.PC;

public class SurfaceHUDPCView : ViewBase<SurfaceHUDVM>, IGameModeHandler, ISubscriber
{
	[SerializeField]
	private OwlcatMultiButton m_EndTurnButton;

	[SerializeField]
	private MoveAnimator m_EndTurnButtonAnimator;

	[SerializeField]
	private TextMeshProUGUI m_EndTurnBindText;

	[SerializeField]
	private InitiativeTrackerView m_InitiativeTrackerView;

	[SerializeField]
	private CombatStartWindowPCView m_CombatStartWindowView;

	[SerializeField]
	private InspectPCView m_InspectPCView;

	[SerializeField]
	private CombatLogPCView m_CombatLogPCView;

	[SerializeField]
	private IngameMenuNewPCView m_IngameMenuPCView;

	[SerializeField]
	private IngameMenuSettingsButtonPCView m_IngameMenuSettingsButtonPCView;

	[SerializeField]
	private PartyPCView m_PartyPCView;

	[SerializeField]
	private SurfaceCombatCurrentUnitView m_SurfaceCombatCurrentUnitView;

	[SerializeField]
	private SurfaceActionBarPCView m_ActionBarView;

	private IDisposable m_TurnUnitSubscribtion;

	private IDisposable m_SelectedUnitSubscribtion;

	private SettingsEntityKeyBindingPair m_PauseBind;

	private bool m_IsFillingSkipCutscene;

	private const float FillDuration = 1.5f;

	private const float FillTarget = 100f;

	private readonly FloatReactiveProperty m_CurrentFill = new FloatReactiveProperty();

	[Header("SkipHint")]
	[SerializeField]
	private TextMeshProUGUI m_SkipText;

	[SerializeField]
	private Color m_SkipHintColor = Color.white;

	[SerializeField]
	private Image m_FillImage;

	[SerializeField]
	private FadeAnimator m_SkipCutsceneHintHolderFade;

	private readonly BoolReactiveProperty m_IsSkipCutsceneHintActive = new BoolReactiveProperty(initialValue: false);

	private IEnumerator m_HideCloseCutsceneHint;

	private IEnumerator m_FillSkipCutsceneCoroutine;

	private string m_SkipHintColorTag => ColorUtility.ToHtmlStringRGB(m_SkipHintColor);

	internal void Initialize()
	{
		m_CombatStartWindowView.Initialize();
		m_InitiativeTrackerView.Initialize();
		m_InspectPCView.Initialize();
		m_CombatLogPCView.Initialize();
		m_IngameMenuPCView.Initialize();
		m_IngameMenuSettingsButtonPCView.Initialize();
		m_PartyPCView.Initialize();
		m_ActionBarView.Initialize();
		m_SurfaceCombatCurrentUnitView.Initialize();
		m_EndTurnButtonAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.ShowEndTurn).Subscribe(ShowEndTurnButton));
		AddDisposable(base.ViewModel.IsTurnBasedActive.And(base.ViewModel.CanEndTurn).Subscribe(delegate(bool val)
		{
			m_EndTurnButton.SetInteractable(val);
			m_EndTurnButton.SetActiveLayer(val ? "Interactable" : "NonInteractable");
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_EndTurnButton.OnLeftClickAsObservable(), delegate
		{
			UISounds.Instance.Sounds.Combat.EndTurn.Play();
			base.ViewModel.EndTurn();
		}));
		AddDisposable(base.ViewModel.InitiativeTrackerVM.Subscribe(m_InitiativeTrackerView.Bind));
		AddDisposable(base.ViewModel.CombatStartWindowVM.Subscribe(m_CombatStartWindowView.Bind));
		AddDisposable(base.ViewModel.CurrentUnit.Subscribe(m_SurfaceCombatCurrentUnitView.Bind));
		m_InspectPCView.Bind(base.ViewModel.InspectVM);
		m_CombatLogPCView.Bind(base.ViewModel.CombatLogVM);
		m_IngameMenuPCView.Bind(base.ViewModel.IngameMenuVM);
		m_IngameMenuSettingsButtonPCView.Bind(base.ViewModel.IngameMenuSettingsButtonVM);
		m_PartyPCView.Bind(base.ViewModel.PartyVM);
		m_ActionBarView.Bind(base.ViewModel.ActionBarVM);
		m_PauseBind = SettingsRoot.Controls.Keybindings.General.Pause;
		m_PauseBind.OnValueChanged += SetEndTurnBindText;
		SetEndTurnBindText();
		SetSkipCutsceneSettings();
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SpeedUpEnemiesTurn.name + UIConsts.SuffixOn, delegate
		{
			SpeedUp(state: true);
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SpeedUpEnemiesTurn.name + UIConsts.SuffixOff, delegate
		{
			SpeedUp(state: false);
		}));
		m_SkipText.text = UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.GetBinding(0).GetPrettyString() + " " + UIStrings.Instance.CommonTexts.SkipHold;
		m_FillImage.fillAmount = 0f;
		m_SkipCutsceneHintHolderFade.DisappearAnimation();
	}

	private void SetSkipCutsceneSettings()
	{
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOn, delegate
		{
			TrySkipCutscene(state: true);
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOff, delegate
		{
			TrySkipCutscene(state: false);
		}));
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.LateUpdateAsObservable(), delegate
		{
			InternalUpdate();
		}));
	}

	protected virtual void InternalUpdate()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.Cutscene) && !m_IsSkipCutsceneHintActive.Value && Input.anyKeyDown)
		{
			ShowSkipHint();
		}
	}

	private void ShowSkipHint()
	{
		m_IsSkipCutsceneHintActive.Value = true;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (CutsceneLock.Active && !(gameMode != GameModeType.Cutscene))
		{
			m_FillImage.fillAmount = 0f;
			m_IsFillingSkipCutscene = false;
			m_IsSkipCutsceneHintActive.Value = false;
			m_SkipCutsceneHintHolderFade.DisappearAnimation();
			AddDisposable(m_IsSkipCutsceneHintActive.Subscribe(HandleSkipCutsceneHintState));
			AddDisposable(m_CurrentFill.Subscribe(delegate(float value)
			{
				m_FillImage.fillAmount = value / 100f;
			}));
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (!CutsceneLock.Active && m_HideCloseCutsceneHint != null)
		{
			HideCutsceneHints();
			if (m_HideCloseCutsceneHint != null)
			{
				StopCoroutine(m_HideCloseCutsceneHint);
			}
			if (m_FillSkipCutsceneCoroutine != null)
			{
				StopCoroutine(m_FillSkipCutsceneCoroutine);
			}
			m_CurrentFill.Value = 0f;
			m_SkipCutsceneHintHolderFade.DisappearAnimation();
		}
	}

	private void HideCutsceneHints()
	{
		m_IsSkipCutsceneHintActive.Value = false;
		m_SkipCutsceneHintHolderFade.DisappearAnimation();
	}

	private void HandleSkipCutsceneHintState(bool value)
	{
		if (!Game.Instance.State.Cutscenes.TryFind((CutscenePlayerData p) => p.Cutscene.LockControl && p.Cutscene.NonSkippable, out var _))
		{
			if (m_FillSkipCutsceneCoroutine != null && !m_IsFillingSkipCutscene)
			{
				StopCoroutine(m_FillSkipCutsceneCoroutine);
			}
			if (m_HideCloseCutsceneHint != null)
			{
				StopCoroutine(m_HideCloseCutsceneHint);
			}
			if (value)
			{
				m_SkipCutsceneHintHolderFade.AppearAnimation();
				string prettyString = UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.GetBinding(0).GetPrettyString();
				m_SkipText.text = "<color=#" + m_SkipHintColorTag + ">[" + prettyString + "]</color> " + UIStrings.Instance.CommonTexts.SkipHold.Text;
				m_HideCloseCutsceneHint = HandleSkipHint();
				StartCoroutine(m_HideCloseCutsceneHint);
			}
		}
	}

	private IEnumerator HandleSkipHint()
	{
		yield return new WaitForSecondsRealtime(3f);
		m_IsSkipCutsceneHintActive.Value = false;
		m_SkipCutsceneHintHolderFade.DisappearAnimation();
	}

	private void OnCutSceneDecline()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.Cutscene))
		{
			EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
			{
				h.HandleOnHideBark();
			});
			m_IsSkipCutsceneHintActive.Value = false;
			m_IsFillingSkipCutscene = false;
			m_SkipCutsceneHintHolderFade.DisappearAnimation();
			Game.Instance.GameCommandQueue.SkipCutscene();
		}
	}

	public void TrySkipCutscene(bool state)
	{
		if (Game.Instance.CurrentMode != GameModeType.Cutscene)
		{
			return;
		}
		if (state && !m_IsFillingSkipCutscene)
		{
			m_IsFillingSkipCutscene = true;
			m_FillSkipCutsceneCoroutine = FillSkipCutsceneCoroutine();
			StartCoroutine(m_FillSkipCutsceneCoroutine);
			if (m_HideCloseCutsceneHint != null)
			{
				StopCoroutine(m_HideCloseCutsceneHint);
			}
		}
		else if (!state && m_IsFillingSkipCutscene)
		{
			m_IsFillingSkipCutscene = false;
			m_CurrentFill.Value = 0f;
			HandleSkipCutsceneHintState(value: true);
		}
	}

	private IEnumerator FillSkipCutsceneCoroutine()
	{
		float elapsedTime = 0f;
		float startFill = m_CurrentFill.Value;
		while (elapsedTime < 1.5f)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / 1.5f);
			m_CurrentFill.Value = Mathf.Lerp(startFill, 100f, t);
			yield return null;
		}
		m_CurrentFill.Value = 100f;
		OnCutSceneDecline();
	}

	private void SpeedUp(bool state)
	{
		Game.Instance.SpeedUp(state);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_PauseBind.OnValueChanged -= SetEndTurnBindText;
		m_PauseBind = null;
		m_SelectedUnitSubscribtion?.Dispose();
		m_SelectedUnitSubscribtion = null;
		m_TurnUnitSubscribtion?.Dispose();
		m_TurnUnitSubscribtion = null;
		if (m_HideCloseCutsceneHint != null)
		{
			HideCutsceneHints();
			StopCoroutine(m_HideCloseCutsceneHint);
			m_HideCloseCutsceneHint = null;
		}
		if (m_FillSkipCutsceneCoroutine != null)
		{
			HideCutsceneHints();
			StopCoroutine(m_FillSkipCutsceneCoroutine);
			m_FillSkipCutsceneCoroutine = null;
		}
		m_CurrentFill.Value = 0f;
	}

	private void ShowEndTurnButton(bool val)
	{
		if (val)
		{
			base.gameObject.SetActive(value: true);
			m_EndTurnButtonAnimator.AppearAnimation();
		}
		else
		{
			m_EndTurnButtonAnimator.DisappearAnimation();
		}
	}

	private void SetEndTurnBindText(KeyBindingPair keyBindingPair = default(KeyBindingPair))
	{
		AddDisposable(m_EndTurnButton.SetHint(UIStrings.Instance.Tooltips.EndTurn, "Pause"));
		m_EndTurnBindText.text = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName("Pause"));
	}
}
