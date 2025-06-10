using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

public class PartyPCView : ViewBase<PartyVM>, IGameModeHandler, ISubscriber, IFullScreenUIHandler, IFullScreenLocalMapUIHandler, ISwitchPartyCharactersHandler
{
	[Header("Switch characters")]
	[SerializeField]
	private OwlcatButton m_Next;

	[SerializeField]
	private OwlcatButton m_Prev;

	[Header("Content Size Control")]
	[SerializeField]
	private HorizontalLayoutGroup m_LayoutGroup;

	[SerializeField]
	private ContentSizeFitterExtended m_ContentSizeFitter;

	[Header("Show hide position")]
	[SerializeField]
	private float m_HidePosY = -295f;

	[SerializeField]
	private float m_ShowPosY;

	private RectTransform m_RectTransform;

	private CanvasGroup m_CanvasGroup;

	private FullScreenUIType m_FullScreenUIType;

	private bool m_Hide;

	private bool m_LocalMapEnabled;

	private readonly List<PartyCharacterPCView> m_Characters = new List<PartyCharacterPCView>();

	private CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : GetComponent<CanvasGroup>());

	private RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	public void Initialize()
	{
		m_Characters.Clear();
		PartyCharacterPCView[] componentsInChildren = GetComponentsInChildren<PartyCharacterPCView>(includeInactive: true);
		foreach (PartyCharacterPCView partyCharacterPCView in componentsInChildren)
		{
			partyCharacterPCView.Initialize(DragCharacter);
			m_Characters.Add(partyCharacterPCView);
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.NextEnable.Subscribe(m_Next.gameObject.SetActive));
		AddDisposable(base.ViewModel.PrevEnable.Subscribe(m_Prev.gameObject.SetActive));
		AddDisposable(ObservableExtensions.Subscribe(m_Next.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Next();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_Prev.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Prev();
		}));
		AddDisposable(Game.Instance.Keyboard.Bind("SelectAllCharacters", SelectAll));
		for (int i = 0; i < m_Characters.Count; i++)
		{
			PartyCharacterVM partyCharacterVM = base.ViewModel.CharactersVM[i];
			m_Characters[i].Bind(partyCharacterVM);
			AddDisposable(partyCharacterVM.IsEnable.Subscribe(delegate
			{
				UpdateLayout();
			}));
		}
		UpdateLayout();
		CheckVisible();
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevCharacter.name, base.ViewModel.SelectPrevCharacter));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextCharacter.name, base.ViewModel.SelectNextCharacter));
	}

	protected override void DestroyViewImplementation()
	{
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
		m_Characters.ForEach(delegate(PartyCharacterPCView ch)
		{
			ch.UpdateBasePosition();
		});
	}

	private void SelectAll()
	{
		UIAccess.SelectionManager.SelectAll();
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
			return;
		}
		if (RootUIContext.Instance.IsSpace && m_FullScreenUIType == FullScreenUIType.Unknown)
		{
			HideAnimation(hide: true);
			return;
		}
		if (m_FullScreenUIType == FullScreenUIType.EscapeMenu)
		{
			HideAnimation(hide: true);
			return;
		}
		if (m_LocalMapEnabled)
		{
			HideAnimation(hide: true);
			return;
		}
		bool flag = m_FullScreenUIType switch
		{
			FullScreenUIType.Encyclopedia => true, 
			FullScreenUIType.Journal => true, 
			FullScreenUIType.LocalMap => true, 
			FullScreenUIType.ShipCustomization => true, 
			FullScreenUIType.ColonyManagement => true, 
			_ => false, 
		};
		bool flag2 = base.ViewModel.ModalWindowUIType == ModalWindowUIType.Respec;
		bool flag3 = flag2;
		HideAnimation(flag || flag3);
	}

	public void HideAnimation(bool hide)
	{
		if (hide != m_Hide)
		{
			m_Hide = hide;
			CanvasGroup.DOKill();
			RectTransform.DOKill();
			CanvasGroup.DOFade(m_Hide ? 0f : 1f, 0.2f).SetDelay(m_Hide ? 0.0001f : 0.2f).SetUpdate(isIndependentUpdate: true);
			RectTransform.DOAnchorPosY(m_Hide ? m_HidePosY : m_ShowPosY, 0.2f).SetDelay(m_Hide ? 0.0001f : 0.2f).SetEase(m_Hide ? Ease.InCubic : Ease.OutCubic)
				.SetUpdate(isIndependentUpdate: true);
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType type)
	{
		m_FullScreenUIType = (state ? type : FullScreenUIType.Unknown);
		DelayedInvoker.InvokeInFrames(CheckVisible, (!state) ? 1 : 0);
	}

	private void DragCharacter(PartyCharacterPCView mainCharacter)
	{
		foreach (PartyCharacterPCView character in m_Characters)
		{
			if (!(mainCharacter == character) && character.HasUnit)
			{
				float num = character.RectTransform.sizeDelta.x / 2f;
				if (mainCharacter.RectTransform.localPosition.x > character.BasePositionX - num && mainCharacter.RectTransform.localPosition.x < character.BasePositionX + num)
				{
					base.ViewModel.SwitchCharacter(mainCharacter.UnitEntityData, character.UnitEntityData);
					PartyCharacterPCView partyCharacterPCView = character;
					float basePositionX = character.BasePositionX;
					float basePositionX2 = mainCharacter.BasePositionX;
					mainCharacter.BasePositionX = basePositionX;
					partyCharacterPCView.BasePositionX = basePositionX2;
					character.RectTransform.DOLocalMoveX(character.BasePositionX, 0.2f).SetUpdate(isIndependentUpdate: true);
					break;
				}
			}
		}
	}

	public void HandleFullScreenLocalMapChanged(bool state)
	{
		m_LocalMapEnabled = state;
		DelayedInvoker.InvokeInFrames(CheckVisible, (!state) ? 1 : 0);
	}

	public void HandleSwitchPartyCharacters(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		PartyCharacterPCView mainCharacter = m_Characters.FirstOrDefault((PartyCharacterPCView c) => c.UnitEntityData == unit1);
		PartyCharacterPCView secondCharacter = m_Characters.FirstOrDefault((PartyCharacterPCView c) => c.UnitEntityData == unit2);
		if (!(mainCharacter == null) && !(secondCharacter == null))
		{
			int index = m_Characters.IndexOf(mainCharacter);
			int index2 = m_Characters.IndexOf(secondCharacter);
			m_Characters[index] = secondCharacter;
			m_Characters[index2] = mainCharacter;
			PartyCharacterVM viewModel = base.ViewModel.CharactersVM.FirstOrDefault((PartyCharacterVM vm) => vm.UnitEntityData == secondCharacter.UnitEntityData);
			PartyCharacterVM viewModel2 = base.ViewModel.CharactersVM.FirstOrDefault((PartyCharacterVM vm) => vm.UnitEntityData == mainCharacter.UnitEntityData);
			m_Characters[index].Bind(viewModel);
			m_Characters[index2].Bind(viewModel2);
		}
	}
}
