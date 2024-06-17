using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Common.Dropdown;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(WidgetListMVVM))]
[RequireComponent(typeof(OwlcatToggleGroup))]
public class OwlcatDropdown : ViewBase<OwlcatDropdownVM>, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private OwlcatMultiButton m_MainMultiButton;

	[SerializeField]
	private DropdownItemView m_DropdownItemView;

	[Space]
	[SerializeField]
	private DropdownItemView m_DropdownItemViewPrefab;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private OwlcatToggleGroup m_ToggleGroup;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private float m_ItemHeight = 21.6f;

	private readonly ReactiveProperty<bool> m_IsOn = new ReactiveProperty<bool>();

	private const string OnLayer = "On";

	private const string OffLayer = "Off";

	private CompositeDisposable m_Subscriptions;

	private CompositeDisposable m_PanelSubscriptions;

	private readonly List<DropdownItemView> m_Items = new List<DropdownItemView>();

	private InputLayer m_InputLayer;

	public static readonly string InputLayerContextName = "OwlcatDropdownInput";

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GameObject m_Blocker;

	private bool m_IsEnteredWithMouse;

	private static List<Canvas> s_ListPool;

	public IReadOnlyReactiveProperty<bool> IsOn => m_IsOn;

	public IConsoleEntity ConsoleEntityProxy => m_MainMultiButton;

	public IReadOnlyReactiveProperty<int> Index => base.ViewModel.Index;

	public int VMCollectionCount => base.ViewModel.VMCollection.Count;

	protected override void BindViewImplementation()
	{
		m_ScrollRect.gameObject.SetActive(value: false);
		AddDisposable(base.ViewModel.SelectedVM.Subscribe(ChangeValue));
		CreateInput();
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}

	private void CreateInput()
	{
		m_Subscriptions = new CompositeDisposable();
		m_Subscriptions.Add(ObservableExtensions.Subscribe(m_MainMultiButton.OnLeftClickAsObservable(), delegate
		{
			m_IsEnteredWithMouse = true;
			SetState(value: true);
		}));
		m_Subscriptions.Add(ObservableExtensions.Subscribe(m_MainMultiButton.OnConfirmClickAsObservable(), delegate
		{
			SetState(value: true);
		}));
	}

	private void ChangeValue(IViewModel viewModel)
	{
		m_DropdownItemView.BindWidgetVM(viewModel);
	}

	public string GetCurrentTextValue()
	{
		return m_DropdownItemView.TextValue;
	}

	public void SetInteractable(bool state)
	{
		m_MainMultiButton.SetInteractable(state);
	}

	public void SetIndex(int index)
	{
		base.ViewModel.SetIndex(index);
	}

	public void SetState(bool value, bool immediately = false)
	{
		if (base.ViewModel?.VMCollection != null && base.ViewModel.VMCollection.Count != 0 && m_IsOn.Value != value)
		{
			m_IsOn.Value = value;
			if (value)
			{
				Show();
			}
			else
			{
				Hide(immediately);
			}
		}
	}

	private void Show()
	{
		m_Subscriptions?.Dispose();
		m_MainMultiButton.SetFocus(value: false);
		m_MainMultiButton.SetActiveLayer("On");
		m_FadeAnimator.AppearAnimation();
		m_Blocker = CreateBlocker();
		m_ScrollRect.gameObject.SetActive(value: true);
		m_PanelSubscriptions = new CompositeDisposable();
		m_PanelSubscriptions.Add(m_WidgetList.DrawEntries(base.ViewModel.VMCollection, m_DropdownItemViewPrefab));
		m_Items.Clear();
		foreach (IWidgetView entry in m_WidgetList.Entries)
		{
			if (entry is DropdownItemView dropdownItemView)
			{
				m_Items.Add(dropdownItemView);
				dropdownItemView.SetToggleGroup(m_ToggleGroup);
				if (dropdownItemView.GetViewModel() == base.ViewModel.SelectedVM.Value)
				{
					dropdownItemView.Toggle.Set(value: true);
				}
				dropdownItemView.SetItemHeight(m_ItemHeight);
			}
		}
		m_PanelSubscriptions.Add(m_ToggleGroup.ActiveToggle.Skip(1).NotNull().Subscribe(delegate(OwlcatToggle toggle)
		{
			int index = m_Items.FindIndex((DropdownItemView item) => item.Toggle == toggle);
			SetIndex(index);
			SetState(value: false);
		}));
		BuildNavigation();
		CreatePanelInput();
		UISounds.Instance.Sounds.DropdownMenu.DropdownMenuShow.Play();
	}

	private void Hide(bool immediately = false)
	{
		m_MainMultiButton.SetActiveLayer("Off");
		m_MainMultiButton.SetFocus(!m_IsEnteredWithMouse);
		if (immediately)
		{
			Clear();
			CreateInput();
			m_ScrollRect.gameObject.SetActive(value: false);
			UISounds.Instance.Sounds.DropdownMenu.DropdownMenuHide.Play();
			return;
		}
		m_FadeAnimator.DisappearAnimation(delegate
		{
			Clear();
			CreateInput();
			m_ScrollRect.gameObject.SetActive(value: false);
			UISounds.Instance.Sounds.DropdownMenu.DropdownMenuHide.Play();
		});
	}

	private void Clear()
	{
		m_IsEnteredWithMouse = false;
		m_Items.Clear();
		m_Subscriptions?.Dispose();
		m_Subscriptions = null;
		m_PanelSubscriptions?.Dispose();
		m_PanelSubscriptions = null;
		m_InputLayer = null;
		m_WidgetList.Clear();
		if (m_Blocker != null)
		{
			UnityEngine.Object.Destroy(m_Blocker);
		}
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour = m_ToggleGroup.GetNavigationBehaviour();
		m_NavigationBehaviour.FocusOnEntityManual(m_ToggleGroup.ActiveToggle.Value);
		m_ScrollRect.SnapToCenter(m_ToggleGroup.ActiveToggle.Value.transform as RectTransform);
		m_PanelSubscriptions.Add(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
	}

	private void CreatePanelInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		m_PanelSubscriptions.Add(EscHotkeyManager.Instance.Subscribe(delegate
		{
			SetState(value: false);
		}));
		m_PanelSubscriptions.Add(m_InputLayer.AddButton(delegate
		{
			SetState(value: false);
		}, 9));
		m_PanelSubscriptions.Add(m_InputLayer.AddAxis(Scroll, 3));
		m_PanelSubscriptions.Add(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private GameObject CreateBlocker()
	{
		s_ListPool = CollectionPool<List<Canvas>, Canvas>.Get();
		base.gameObject.GetComponentsInParent(includeInactive: false, s_ListPool);
		if (s_ListPool.Count == 0)
		{
			return null;
		}
		List<Canvas> list = s_ListPool;
		Canvas canvas = list[list.Count - 1];
		using (IEnumerator<Canvas> enumerator = s_ListPool.Where((Canvas t) => t.isRootCanvas).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				canvas = enumerator.Current;
			}
		}
		CollectionPool<List<Canvas>, Canvas>.Release(s_ListPool);
		GameObject gameObject = new GameObject("Blocker");
		RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
		rectTransform.SetParent(canvas.transform, worldPositionStays: false);
		rectTransform.anchorMin = Vector3.zero;
		rectTransform.anchorMax = Vector3.one;
		rectTransform.sizeDelta = Vector2.zero;
		GameObject gameObject2 = m_ScrollRect.gameObject;
		Canvas canvas2 = gameObject.AddComponent<Canvas>();
		canvas2.overrideSorting = true;
		Canvas component = gameObject2.GetComponent<Canvas>();
		canvas2.sortingLayerID = component.sortingLayerID;
		canvas2.sortingOrder = component.sortingOrder - 1;
		Canvas canvas3 = null;
		Transform parent = gameObject2.transform.parent;
		while (parent != null)
		{
			canvas3 = parent.GetComponent<Canvas>();
			if (canvas3 != null)
			{
				break;
			}
			parent = parent.parent;
		}
		if (canvas3 != null)
		{
			Component[] components = canvas3.GetComponents<BaseRaycaster>();
			components = components;
			for (int i = 0; i < components.Length; i++)
			{
				Type type = components[i].GetType();
				if (gameObject.GetComponent(type) == null)
				{
					gameObject.AddComponent(type);
				}
			}
		}
		else
		{
			GetOrAddComponent<GraphicRaycaster>(gameObject);
		}
		gameObject.AddComponent<Image>().color = Color.clear;
		gameObject.AddComponent<Button>().onClick.AddListener(delegate
		{
			SetState(value: false);
		});
		return gameObject;
	}

	private static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (!val)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_ScrollRect.Scroll(value, smooth: true);
	}

	public void OnDisable()
	{
		SetState(value: false, immediately: true);
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_NavigationBehaviour;
	}
}
