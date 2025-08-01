using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Party.Console;

public class PartyConsoleView : ViewBase<PartyVM>, IGameModeHandler, ISubscriber, IFullScreenUIHandler, IFullScreenLocalMapUIHandler
{
	[Header("Content Size Control")]
	[SerializeField]
	private HorizontalLayoutGroup m_LayoutGroup;

	[SerializeField]
	private ContentSizeFitterExtended m_ContentSizeFitter;

	[Header("ConsoleHints")]
	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[Header("Misc")]
	[SerializeField]
	private List<FullScreenUIType> m_WindowsWithParty = new List<FullScreenUIType> { FullScreenUIType.Inventory };

	[SerializeField]
	private GameObject m_LeftArrow;

	[SerializeField]
	private GameObject m_RightArrow;

	private RectTransform m_RectTransform;

	private CanvasGroup m_CanvasGroup;

	private FullScreenUIType m_FullScreenUIType;

	private bool m_Hide;

	private bool m_LocalMapEnabled;

	private bool m_PartySelectorEnabled;

	private readonly List<PartyCharacterConsoleView> m_Characters = new List<PartyCharacterConsoleView>();

	private PartyCharacterConsoleView[] m_CharacterViews;

	public bool PartySelectorEnabled
	{
		set
		{
			m_PartySelectorEnabled = value;
			CheckVisible();
		}
	}

	private CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : GetComponent<CanvasGroup>());

	private RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	public void Initialize()
	{
		m_Characters.Clear();
		m_CharacterViews = GetComponentsInChildren<PartyCharacterConsoleView>(includeInactive: true);
		PartyCharacterConsoleView[] characterViews = m_CharacterViews;
		foreach (PartyCharacterConsoleView partyCharacterConsoleView in characterViews)
		{
			partyCharacterConsoleView.Initialize(null);
			m_Characters.Add(partyCharacterConsoleView);
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		for (int i = 0; i < m_Characters.Count; i++)
		{
			PartyCharacterVM partyCharacterVM = base.ViewModel.CharactersVM[i];
			m_Characters[i].Bind(partyCharacterVM);
			AddDisposable(partyCharacterVM.IsEnable.Subscribe(delegate
			{
				UpdateLayout();
			}));
		}
		DelayedInvoker.InvokeInFrames(UpdatePartySizeAndLayout, 1);
		AddDisposable(UniRxExtensionMethods.Subscribe(base.ViewModel.UpdateViewLayout, delegate
		{
			DelayedInvoker.InvokeInFrames(UpdatePartySizeAndLayout, 1);
		}));
		UpdateLayout();
		CheckVisible();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enable)
	{
		ReactiveProperty<bool> property = new ReactiveProperty<bool>(Game.Instance.SelectionCharacter.ActualGroup.Count > 1);
		AddDisposable(m_PreviousHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectNeighbour(next: false);
		}, 14, property.And(enable).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
		AddDisposable(m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectNeighbour(next: true);
		}, 15, property.And(enable).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
	}

	private void UpdateLayout()
	{
		m_LayoutGroup.enabled = true;
		m_ContentSizeFitter.enabled = true;
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		Vector2 size = ((RectTransform)base.transform).rect.size;
		m_LayoutGroup.enabled = false;
		m_ContentSizeFitter.enabled = false;
		((RectTransform)base.transform).sizeDelta = size;
		m_Characters.ForEach(delegate(PartyCharacterConsoleView ch)
		{
			ch.UpdateBasePosition();
		});
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		CheckVisible();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void CheckVisible()
	{
		if (Game.Instance.CurrentMode == GameModeType.Dialog || Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			HideAnimation(hide: true);
		}
		else if (RootUIContext.Instance.IsSpace)
		{
			HideAnimation(!m_WindowsWithParty.Contains(m_FullScreenUIType));
		}
		else if (m_LocalMapEnabled || m_PartySelectorEnabled)
		{
			HideAnimation(hide: true);
		}
		else
		{
			HideAnimation((m_FullScreenUIType != 0 || base.ViewModel.ModalWindowUIType == ModalWindowUIType.Respec) && !m_WindowsWithParty.Contains(m_FullScreenUIType));
		}
	}

	public void HideAnimation(bool hide)
	{
		if (hide != m_Hide)
		{
			m_Hide = hide;
			CanvasGroup.DOKill();
			CanvasGroup.DOFade(m_Hide ? 0f : 1f, 0.2f).SetUpdate(isIndependentUpdate: true);
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType type)
	{
		m_FullScreenUIType = (state ? type : FullScreenUIType.Unknown);
		CheckVisible();
	}

	public void HandleFullScreenLocalMapChanged(bool state)
	{
		m_LocalMapEnabled = state;
		CheckVisible();
	}

	private void UpdatePartySizeAndLayout()
	{
		m_LeftArrow.SetActive(value: false);
		m_RightArrow.SetActive(value: false);
		int num = 6;
		if (Game.Instance.RootUiContext.FullScreenUIType != FullScreenUIType.Inventory)
		{
			num = 12;
		}
		int num2 = Game.Instance.SelectionCharacter.ActualGroup.IndexOf(Game.Instance.SelectionCharacter.SelectedUnitInUI.Value);
		int num3 = num - 1;
		int num4 = num2 / num * num;
		if (num2 / num > 0 && num2 % num == 0 && Game.Instance.SelectionCharacter.ActualGroup.Count - num4 >= 0 && Game.Instance.SelectionCharacter.ActualGroup.Count - num4 < 6)
		{
			num4 = Game.Instance.SelectionCharacter.ActualGroup.Count - num;
		}
		if (num4 < 0)
		{
			num4 = 0;
		}
		m_LeftArrow.SetActive(num2 > num3);
		m_RightArrow.SetActive(num2 < Game.Instance.SelectionCharacter.ActualGroup.Count - num - 1);
		m_Characters.Clear();
		PartyCharacterConsoleView[] characterViews = m_CharacterViews;
		foreach (PartyCharacterConsoleView partyCharacterConsoleView in characterViews)
		{
			partyCharacterConsoleView.gameObject.SetActive(value: false);
			partyCharacterConsoleView.Initialize(null);
			m_Characters.Add(partyCharacterConsoleView);
		}
		for (int j = 0; j < num; j++)
		{
			PartyCharacterVM partyCharacterVM = base.ViewModel.CharactersVM[num4 + j];
			m_Characters[j].Bind(partyCharacterVM);
			AddDisposable(partyCharacterVM.IsEnable.Subscribe(delegate
			{
				UpdateLayout();
			}));
		}
		UpdateLayout();
		CheckVisible();
	}
}
