using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class PortraitSelectorCommonView : BaseCharGenAppearancePageComponentView<CharGenPortraitsSelectorVM>, IConsoleEntityProxy, IConsoleEntity, ICharGenAppearancePhasePortraitHandler, ISubscriber
{
	[SerializeField]
	private CharGenPortraitTabSelectorView m_TabSelector;

	[SerializeField]
	private WidgetListMVVM m_WidgetListDefaultGroups;

	[SerializeField]
	private CharGenDefaultPortraitGroupView m_DefaultGroupPrefab;

	[SerializeField]
	private CharGenCustomPortraitGroupView m_CustomPortraitGroup;

	[SerializeField]
	private CharGenCustomPortraitCreatorView m_CustomPortraitCreatorView;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	[Header("Containers")]
	[SerializeField]
	private GameObject m_PortraitSelectorContainer;

	[SerializeField]
	private GameObject m_DefaultPortraitsContainer;

	[SerializeField]
	private GameObject m_CustomPortraitsContainer;

	[SerializeField]
	private GameObject m_CustomPortraitsInfoContainer;

	[Header("Controls")]
	[SerializeField]
	private OwlcatButton m_ChangePortraitButton;

	[SerializeField]
	private TextMeshProUGUI m_ChangePortraitLabel;

	[SerializeField]
	private TextMeshProUGUI m_ChangePortraitDescription;

	[Space]
	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	private bool m_IsInit;

	private bool m_IsInputAdded;

	private GridConsoleNavigationBehaviour m_Navigation;

	private readonly BoolReactiveProperty m_FocusOnScreen = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanLongClick = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsMainCharacter = new BoolReactiveProperty();

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public IConsoleEntity ConsoleEntityProxy => m_Navigation;

	private void Initialize()
	{
		if (!m_IsInit)
		{
			m_TabSelector.Initialize();
			m_TabSelector.gameObject.SetActive(value: true);
			m_CustomPortraitCreatorView.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		AddDisposable(EventBus.Subscribe(this));
		m_Navigation = new GridConsoleNavigationBehaviour();
		AddDisposable(m_Navigation.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		m_TabSelector.Bind(base.ViewModel.TabSelector);
		AddDisposable(m_WidgetListDefaultGroups.DrawEntries(base.ViewModel.PortraitGroupVms.Values.OrderBy((CharGenPortraitGroupVM gr) => gr.PortraitCategory), m_DefaultGroupPrefab));
		m_CustomPortraitGroup.Bind(base.ViewModel.CustomPortraitGroup);
		AddDisposable(base.ViewModel.CustomPortraitCreatorVM.Subscribe(delegate(CharGenCustomPortraitCreatorVM value)
		{
			m_CustomPortraitCreatorView.Bind(value);
			m_ScrollRectExtended.ScrollToTop();
		}));
		if ((bool)m_ChangePortraitButton)
		{
			bool flag = UINetUtility.IsControlMainCharacter();
			m_ChangePortraitButton.Or(null)?.SetInteractable(flag);
			if (!flag)
			{
				return;
			}
			AddDisposable(base.ViewModel.PortraitVM.Subscribe(delegate(CharGenPortraitVM value)
			{
				m_ChangePortraitButton.SetInteractable(value.PortraitData.IsCustom);
			}));
			AddDisposable(ObservableExtensions.Subscribe(m_ChangePortraitButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.OpenCustomPortraitCreator();
			}));
		}
		AddDisposable(base.ViewModel.CurrentTab.Subscribe(delegate(CharGenPortraitTabVM tab)
		{
			OnSwitchTab(tab.Tab);
		}));
		SetupLabels();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_IsInputAdded = false;
	}

	private void OnSwitchTab(CharGenPortraitTab tab)
	{
		if (tab != 0 && NetworkingManager.IsActive)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.NotAvailableInCoopMode, DialogMessageBoxBase.BoxType.Message, null);
			tab = CharGenPortraitTab.Default;
		}
		Game.Instance.GameCommandQueue.CharGenSwitchPortraitTab(tab);
	}

	void ICharGenAppearancePhasePortraitHandler.HandlePortraitTabChange(CharGenPortraitTab tab)
	{
		base.ViewModel.CurrentTab.Value = base.ViewModel.TabSelector.EntitiesCollection.FirstOrDefault((CharGenPortraitTabVM elem) => elem.Tab == tab);
		m_DefaultPortraitsContainer.SetActive(tab == CharGenPortraitTab.Default);
		m_CustomPortraitsContainer.SetActive(tab == CharGenPortraitTab.Custom);
		m_CustomPortraitsInfoContainer.SetActive(tab == CharGenPortraitTab.Custom);
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		m_Navigation.Clear();
		CharGenPortraitTabVM value = base.ViewModel.CurrentTab.Value;
		if (value == null)
		{
			return;
		}
		IConsoleEntity consoleEntity = null;
		if (value.Tab == CharGenPortraitTab.Default)
		{
			foreach (IWidgetView entry in m_WidgetListDefaultGroups.Entries)
			{
				if (entry is CharGenDefaultPortraitGroupView charGenDefaultPortraitGroupView)
				{
					m_Navigation.AddRow<CharGenDefaultPortraitGroupView>(charGenDefaultPortraitGroupView);
					if (consoleEntity == null)
					{
						consoleEntity = charGenDefaultPortraitGroupView;
					}
				}
			}
		}
		else
		{
			m_Navigation.AddRow<CharGenCustomPortraitGroupView>(m_CustomPortraitGroup);
			consoleEntity = m_CustomPortraitGroup;
		}
		if (IsFocused.Value)
		{
			m_Navigation.SetCurrentEntity(consoleEntity);
			SetFocus(value: true);
			RectTransform rectTransform = (m_Navigation.DeepestNestedFocus as MonoBehaviour)?.GetComponent<RectTransform>();
			if ((bool)m_ScrollRectExtended && (bool)rectTransform)
			{
				m_ScrollRectExtended.EnsureVisibleVertical(rectTransform);
			}
		}
		m_ScrollRectExtended.ScrollToTop();
	}

	public override void AddInput(ref InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		m_FocusOnScreen.Value = true;
		m_IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		if (!m_IsInputAdded)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				OnFunc01Click();
			}, 10, m_FocusOnScreen.And(m_IsMainCharacter).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
			AddDisposable(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CharGen.SwitchPortraitsCategoryTab));
			AddDisposable(inputBindStruct);
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				OpenPortraitCreatorAtCurrent();
			}, 10, m_CanLongClick, InputActionEventType.ButtonJustLongPressed);
			AddDisposable(hintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CharGen.ChangePortrait));
			AddDisposable(inputBindStruct2);
		}
		m_IsInputAdded = true;
	}

	private void OpenPortraitCreatorAtCurrent()
	{
		if (m_Navigation.DeepestNestedFocus is CharGenPortraitSelectorItemView charGenPortraitSelectorItemView)
		{
			base.ViewModel.SelectPortraitAndOpenCreator(charGenPortraitSelectorItemView.GetViewModel() as CharGenPortraitSelectorItemVM);
		}
	}

	public override void RemoveInput()
	{
		m_FocusOnScreen.Value = false;
	}

	private void OnFunc01Click()
	{
		base.ViewModel.SelectNextPortraitsTab();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			m_Navigation.FocusOnCurrentEntity();
			foreach (IWidgetView entry in m_WidgetListDefaultGroups.Entries)
			{
				(entry as CharGenDefaultPortraitGroupView)?.FocusOnSelectedEntityOrFirst();
			}
			m_CustomPortraitGroup.FocusOnSelectedEntityOrFirst();
		}
		else
		{
			m_Navigation.UnFocusCurrentEntity();
		}
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRectExtended.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 180f, smoothly: false, needPinch: false);
		}
		m_CanLongClick.Value = base.ViewModel.CurrentTab.Value.Tab == CharGenPortraitTab.Custom && ((entity as ILongConfirmClickHandler)?.CanLongConfirmClick() ?? false);
	}

	private void SetupLabels()
	{
		if ((bool)m_ChangePortraitLabel)
		{
			m_ChangePortraitLabel.text = UIStrings.Instance.CharGen.ChangePortrait;
		}
		if ((bool)m_ChangePortraitDescription)
		{
			string text = (Game.Instance.IsControllerMouse ? UIStrings.Instance.CharGen.ChangePortraitDescription : UIStrings.Instance.CharGen.ChangePortraitDescriptionConsole);
			m_ChangePortraitDescription.text = text;
		}
	}
}
