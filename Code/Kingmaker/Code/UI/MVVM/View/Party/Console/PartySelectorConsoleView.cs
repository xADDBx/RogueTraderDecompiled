using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Party.Console;

public class PartySelectorConsoleView : ViewBase<PartyVM>, ISwitchPartyCharactersHandler, ISubscriber
{
	[SerializeField]
	private PartySelectorItemConsoleView m_ItemPrefab;

	[SerializeField]
	private GridLayoutGroup m_Content;

	[Space]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_LinkAllHint;

	[SerializeField]
	private ConsoleHint m_LinkHint;

	[SerializeField]
	private ConsoleHint m_LevelUpHint;

	[SerializeField]
	private ConsoleHint m_MoveToNextHint;

	[SerializeField]
	private ConsoleHint m_MoveToPreviousHint;

	private List<PartySelectorItemConsoleView> m_CreatedItems = new List<PartySelectorItemConsoleView>();

	private List<PartySelectorItemConsoleView> m_Characters = new List<PartySelectorItemConsoleView>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private IReadOnlyReactiveProperty<PartySelectorItemConsoleView> m_SelectedEntity;

	private readonly BoolReactiveProperty m_IsLevelUp = new BoolReactiveProperty();

	private bool IsInHub
	{
		get
		{
			if (!RootUIContext.Instance.IsSpace)
			{
				return Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode ?? false;
			}
			return true;
		}
	}

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		InitializeItems();
	}

	private void InitializeItems()
	{
		m_CreatedItems = m_Content.GetComponentsInChildren<PartySelectorItemConsoleView>(includeInactive: true).ToList();
		foreach (PartySelectorItemConsoleView createdItem in m_CreatedItems)
		{
			createdItem.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		base.ViewModel.UpdateConsoleGroup();
		for (int i = 0; i < base.ViewModel.CharactersVM.Count - m_CreatedItems.Count; i++)
		{
			PartySelectorItemConsoleView partySelectorItemConsoleView = Object.Instantiate(m_ItemPrefab, m_Content.transform, worldPositionStays: false);
			partySelectorItemConsoleView.Initialize();
			m_CreatedItems.Add(partySelectorItemConsoleView);
		}
		m_Characters.Clear();
		m_Characters = m_CreatedItems.GetRange(0, base.ViewModel.CharactersVM.Count);
		for (int j = 0; j < m_Characters.Count; j++)
		{
			if (base.ViewModel.CharactersVM[j].UnitEntityData != null)
			{
				m_Characters[j].Bind(base.ViewModel.CharactersVM[j]);
			}
		}
		AddDisposable(m_CanvasSortingComponent.PushView());
		m_FadeAnimator.AppearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		CreateNavigation();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: true, ModalWindowUIType.PartySelector);
		});
		EventBus.RaiseEvent(delegate(ICullFocusHandler h)
		{
			h.HandleRemoveFocus();
		});
	}

	protected override void DestroyViewImplementation()
	{
		PartySelectorItemConsoleView partySelectorItemConsoleView = m_NavigationBehaviour?.CurrentEntity as PartySelectorItemConsoleView;
		m_FadeAnimator.DisappearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		m_NavigationBehaviour?.Clear();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: false, ModalWindowUIType.PartySelector);
		});
		EventBus.RaiseEvent(delegate(ICullFocusHandler h)
		{
			h.HandleRestoreFocus();
		});
		if (partySelectorItemConsoleView != null)
		{
			partySelectorItemConsoleView.SetSelected();
		}
		foreach (PartySelectorItemConsoleView character in m_Characters)
		{
			character.Unbind();
		}
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, Vector2Int.one));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "PartySelectorConsoleView"
		});
		if (IsInHub)
		{
			CreateTwoColumnsNavigation();
		}
		else
		{
			CreateNavigationWithRemote();
		}
		if (m_Characters.Count > 0)
		{
			PartySelectorItemConsoleView partySelectorItemConsoleView = m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c != null && c.IsBinded && c.UnitEntityData == UIUtility.GetCurrentSelectedUnit());
			partySelectorItemConsoleView = ((partySelectorItemConsoleView != null) ? partySelectorItemConsoleView : m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c.IsBinded && (c.UnitEntityData?.IsDirectlyControllable() ?? false)));
			partySelectorItemConsoleView = ((partySelectorItemConsoleView != null) ? partySelectorItemConsoleView : m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c.IsBinded));
			m_NavigationBehaviour.FocusOnEntityManual(partySelectorItemConsoleView, fullReset: false);
		}
		m_SelectedEntity = m_NavigationBehaviour.DeepestFocusAsObservable.Select((IConsoleEntity e) => e as PartySelectorItemConsoleView).ToReactiveProperty();
		AddDisposable(m_SelectedEntity.Subscribe(delegate
		{
			OnFocusChanged();
		}));
		CreateInput();
	}

	private void CreateNavigationWithRemote()
	{
		m_Content.constraintCount = 1;
		List<PartySelectorItemConsoleView> range = m_Characters.GetRange(0, m_Characters.Count);
		m_NavigationBehaviour.AddRow(range.Where((PartySelectorItemConsoleView item) => item.IsBinded && item.UnitEntityData.InPartyAndControllable()).ToList());
	}

	private void CreateTwoColumnsNavigation()
	{
		int num = ((m_Characters.Count((PartySelectorItemConsoleView c) => c.gameObject.activeSelf) <= 6) ? 1 : 2);
		m_Content.constraintCount = num;
		int num2 = Mathf.CeilToInt(1f * (float)m_Characters.Count / (float)num);
		List<PartySelectorItemConsoleView> range = m_Characters.GetRange(0, num2);
		List<PartySelectorItemConsoleView> range2 = m_Characters.GetRange(num2, m_Characters.Count - num2);
		m_NavigationBehaviour.AddRow(range.Where((PartySelectorItemConsoleView item) => item.IsBinded && item.UnitEntityData.InPartyAndControllable()).ToList());
		m_NavigationBehaviour.AddRow(range2.ToList());
	}

	private void CreateInput()
	{
		if (IsInHub || m_Characters.Count <= 0)
		{
			AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
			return;
		}
		if (m_Characters.Any((PartySelectorItemConsoleView c) => c.UnitEntityData.IsDirectlyControllable()))
		{
			AddDisposable(m_LinkHint.Bind(m_InputLayer.AddButton(SetLink, 11, InputActionEventType.ButtonJustReleased)));
			AddDisposable(m_LinkAllHint.Bind(m_InputLayer.AddButton(SetMassLink, 11, InputActionEventType.ButtonJustLongPressed)));
			AddDisposable(m_LevelUpHint.Bind(m_InputLayer.AddButton(LevelUp, 10, m_IsLevelUp)));
			m_LevelUpHint.SetLabel(UIStrings.Instance.MainMenu.LevelUp);
			AddDisposable(m_MoveToNextHint.Bind(m_InputLayer.AddButton(delegate
			{
				MoveCharacter(next: true);
			}, 15)));
			AddDisposable(m_MoveToPreviousHint.Bind(m_InputLayer.AddButton(delegate
			{
				MoveCharacter(next: false);
			}, 14)));
		}
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void SetLink(InputActionEventData data)
	{
		if (m_SelectedEntity != null && m_SelectedEntity.Value.UnitEntityData.IsDirectlyControllable())
		{
			BlueprintUISound.UISoundPartySelectorConsole partySelectorConsole = UISounds.Instance.Sounds.PartySelectorConsole;
			((m_SelectedEntity.Value.Or(null)?.IsLinked?.Value).GetValueOrDefault() ? partySelectorConsole.UnselectOne : partySelectorConsole.SelectOne).Play();
			m_SelectedEntity?.Value.SetLink();
			OnFocusChanged();
		}
	}

	private void SetMassLink(InputActionEventData data)
	{
		BlueprintUISound.UISoundPartySelectorConsole partySelectorConsole = UISounds.Instance.Sounds.PartySelectorConsole;
		((m_SelectedEntity.Value.Or(null)?.IsLinked?.Value).GetValueOrDefault() ? partySelectorConsole.UnselectAll : partySelectorConsole.SelectAll).Play();
		base.ViewModel.SetMassLink();
		OnFocusChanged();
	}

	private void LevelUp(InputActionEventData data)
	{
		m_SelectedEntity.Value.Or(null)?.LevelUp();
	}

	private void MoveCharacter(bool next)
	{
		base.ViewModel.SwitchCharacter(next);
	}

	private void OnFocusChanged()
	{
		string label = ((m_SelectedEntity.Value.Or(null)?.IsLinked?.Value).GetValueOrDefault() ? UIStrings.Instance.PartyTexts.Unlink : UIStrings.Instance.PartyTexts.Link);
		m_LinkHint.SetLabel(label);
		string label2 = (base.ViewModel.CharactersVM.Where((PartyCharacterVM c) => c.UnitEntityData != null).Any((PartyCharacterVM c) => c.IsLinked.Value) ? UIStrings.Instance.PartyTexts.UnlinkAll : UIStrings.Instance.PartyTexts.LinkAll);
		m_LinkAllHint.SetLabel(label2);
		m_IsLevelUp.Value = m_SelectedEntity.Value.Or(null)?.IsLevelUp.Value ?? false;
	}

	public void HandleSwitchPartyCharacters(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c.UnitEntityData == unit1));
		}, 1);
	}
}
