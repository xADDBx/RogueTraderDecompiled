using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.View.Exploration.Console;
using Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Console;

public class ColonyManagementConsoleView : ColonyManagementBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ColonyManagementNavigationConsoleView m_Navigation;

	[SerializeField]
	[UsedImplicitly]
	private ColonyManagementPageConsoleView m_Page;

	[SerializeField]
	private CanvasGroup m_PageCanvasGroup;

	[Header("Input")]
	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private NavigationBlockHighlight[] m_NavigationBlockHighlights;

	[SerializeField]
	private float m_NavigationBlockHighlightDefaultScale = 1f;

	[SerializeField]
	private float m_NavigationBlockHighlightSelectedScale = 1f;

	[SerializeField]
	private float m_NavigationBlockHighlightTransitionTime;

	private readonly ReactiveProperty<bool> m_CanInteract = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanShowInfo = new ReactiveProperty<bool>();

	private bool m_ShowTooltip;

	private bool m_LockInput;

	private Sequence m_PageAnimator;

	private InputLayer m_ResourcesInputLayer;

	private GridConsoleNavigationBehaviour m_ResourcesNavigationBehavior;

	private readonly BoolReactiveProperty m_ResourcesMode = new BoolReactiveProperty();

	private Dictionary<NavigationBlock, List<IConsoleNavigationEntity>> m_Entities = new Dictionary<NavigationBlock, List<IConsoleNavigationEntity>>();

	private NavigationBlock m_CurrentNavigationBlock;

	protected override void InitializeImpl()
	{
		m_Page.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_Navigation.Bind(base.ViewModel.NavigationVM);
		AddDisposable(base.ViewModel.ColonyManagementPage.Subscribe(m_Page.Bind));
		base.BindViewImplementation();
	}

	protected override void DestroyViewImplementation()
	{
		m_PageAnimator?.Kill();
		m_PageAnimator = null;
		base.DestroyViewImplementation();
	}

	protected override void OnHideImpl()
	{
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
		{
			h.HandleColonyProjectsUIClose();
		});
	}

	private void Close()
	{
		TooltipHelper.HideTooltip();
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	protected override void BuildNavigationImpl()
	{
		m_NavigationBehaviour.Clear();
		m_PointsNavigationBehaviour.Clear();
		m_Entities.Clear();
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		list.AddRange(m_Page.GetEventsNavigationEntities());
		List<IConsoleNavigationEntity> list2 = new List<IConsoleNavigationEntity>();
		m_PointsNavigationBehaviour.AddEntities(m_Page.GetProjectsBuiltListFloatNavigationEntities());
		list2.AddRange(m_PointsNavigationBehaviour.Entities.Select((IConsoleEntity entry) => (IConsoleNavigationEntity)entry));
		List<IConsoleNavigationEntity> list3 = new List<IConsoleNavigationEntity>();
		list3.AddRange(m_Page.GetStatsNavigationEntities());
		list3.AddRange(m_Page.GetTraitsNavigationEntities());
		m_Entities.Add(NavigationBlock.Left, list);
		m_Entities.Add(NavigationBlock.Center, list2);
		m_Entities.Add(NavigationBlock.Right, list3);
		m_NavigationBehaviour.AddColumn(list);
		m_NavigationBehaviour.AddColumn(new IConsoleNavigationEntity[1] { m_PointsNavigationBehaviour });
		m_NavigationBehaviour.AddColumn(list3);
		ResetHighlight();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InteractCurrentFocusedEntity();
		}, 8, m_CanInteract), UIStrings.Instance.SettingsUI.MenuConfirm));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(ToggleTooltip, 19, m_CanShowInfo), UIStrings.Instance.CommonTexts.Information));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddAxis(SwitchBlock, 2, base.ViewModel.HasColonies, repeat: true), UIStrings.Instance.CommonTexts.Select));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OpenColonyProjects();
		}, 10, base.ViewModel.HasColonies, InputActionEventType.ButtonJustReleased), UIStrings.Instance.ColonyProjectsTexts.OpenProjectsButton));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			ShowResources();
		}, 11, InputActionEventType.ButtonJustReleased), UIStrings.Instance.GlobalMap.ShowResources));
		AddDisposable(m_PrevHint.Bind(inputLayer.AddButton(delegate
		{
			SelectPrevColony();
		}, 14)));
		AddDisposable(m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			SelectNextColony();
		}, 15)));
		AddDisposable(m_ResourcesNavigationBehavior = new GridConsoleNavigationBehaviour());
		m_ResourcesInputLayer = m_ResourcesNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "ExplorationSpaceResources"
		});
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_ResourcesInputLayer.AddButton(delegate
		{
			CloseResources();
		}, 9, m_ResourcesMode), UIStrings.Instance.Dialog.CloseGlossary));
		AddDisposable(m_ResourcesInputLayer.AddButton(delegate
		{
			CloseResources();
		}, 11, m_ResourcesMode, InputActionEventType.ButtonJustReleased));
	}

	private void SwitchBlock(InputActionEventData obj, float value)
	{
		NavigationBlock[] array = (NavigationBlock[])Enum.GetValues(typeof(NavigationBlock));
		NavigationBlock navigationBlock = ((value < -0.5f) ? array[Mathf.Clamp(Array.IndexOf(array, m_CurrentNavigationBlock) - 1, 0, array.Length - 1)] : ((!(value > 0.5f)) ? m_CurrentNavigationBlock : array[Mathf.Clamp(Array.IndexOf(array, m_CurrentNavigationBlock) + 1, 0, array.Length - 1)]));
		NavigationBlock navigationBlock2 = navigationBlock;
		if (m_Entities.TryGetValue(navigationBlock2, out var value2))
		{
			if (value2.Any((IConsoleNavigationEntity x) => x.IsValid()))
			{
				m_CurrentNavigationBlock = navigationBlock2;
				m_NavigationBehaviour.FocusOnEntityManual(value2.FirstOrDefault());
			}
		}
		else
		{
			PFLog.UI.Error("ExplorationConsoleView.SwitchBlock - can't find entities to focus");
		}
	}

	private void SelectPrevColony()
	{
		if (!m_LockInput)
		{
			PlayPageAnimation();
			m_Navigation.SelectPrevColony();
			m_SelectorView.ChangeTab(m_Navigation.GetActiveColonyIndex());
			BuildNavigation();
		}
	}

	private void SelectNextColony()
	{
		if (!m_LockInput)
		{
			PlayPageAnimation();
			m_Navigation.SelectNextColony();
			m_SelectorView.ChangeTab(m_Navigation.GetActiveColonyIndex());
			BuildNavigation();
		}
	}

	private void PlayPageAnimation()
	{
		m_LockInput = true;
		m_PageAnimator?.Kill();
		m_PageAnimator = DOTween.Sequence().SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_PageAnimator.Append(m_PageCanvasGroup.DOFade(0f, 0.1f));
		m_PageAnimator.Append(m_PageCanvasGroup.DOFade(1f, 0.2f));
		m_PageAnimator.Play();
		DelayedInvoker.InvokeInTime(delegate
		{
			m_LockInput = false;
		}, 0.3f);
	}

	protected override void OnFocusChangedImpl(IConsoleEntity entity)
	{
		EnsureScrollsPosition(entity);
		using (IEnumerator<KeyValuePair<NavigationBlock, List<IConsoleNavigationEntity>>> enumerator = m_Entities.Where((KeyValuePair<NavigationBlock, List<IConsoleNavigationEntity>> list) => list.Value.HasItem((IConsoleNavigationEntity)entity)).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				KeyValuePair<NavigationBlock, List<IConsoleNavigationEntity>> current = enumerator.Current;
				m_CurrentNavigationBlock = current.Key;
				HighlightBlock(current.Key);
			}
		}
		if (entity is IExplorationComponentEntity explorationComponentEntity)
		{
			m_CurrentFocusedEntity = explorationComponentEntity;
			m_CanInteract.Value = explorationComponentEntity.CanInteract();
			bool flag = explorationComponentEntity.CanShowTooltip();
			m_CanShowInfo.Value = flag;
			m_ShowTooltip &= flag;
			ShowTooltip(explorationComponentEntity);
		}
		else
		{
			m_CurrentFocusedEntity = null;
			m_CanInteract.Value = false;
			m_CanShowInfo.Value = false;
		}
	}

	private void EnsureScrollsPosition(IConsoleEntity entity)
	{
		if (!(entity is ColonyTraitBaseView entity2))
		{
			if (entity is ColonyEventBaseView entity3)
			{
				m_Page.ScrollEventsList(entity3);
			}
		}
		else
		{
			m_Page.ScrollTraitsList(entity2);
		}
	}

	private void InteractCurrentFocusedEntity()
	{
		m_CurrentFocusedEntity?.Interact();
	}

	private void HighlightBlock(NavigationBlock navigationBlock)
	{
		NavigationBlockHighlight[] navigationBlockHighlights = m_NavigationBlockHighlights;
		for (int i = 0; i < navigationBlockHighlights.Length; i++)
		{
			NavigationBlockHighlight navigationBlockHighlight = navigationBlockHighlights[i];
			ShortcutExtensions.DOScale(endValue: (navigationBlockHighlight.NavigationBlock == navigationBlock) ? m_NavigationBlockHighlightSelectedScale : m_NavigationBlockHighlightDefaultScale, target: navigationBlockHighlight.HighlightContainer, duration: m_NavigationBlockHighlightTransitionTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		}
	}

	private void ResetHighlight()
	{
		NavigationBlockHighlight[] navigationBlockHighlights = m_NavigationBlockHighlights;
		for (int i = 0; i < navigationBlockHighlights.Length; i++)
		{
			navigationBlockHighlights[i].HighlightContainer.localScale = Vector3.one;
		}
	}

	private void ShowTooltip(IExplorationComponentEntity explorationComponentEntity)
	{
		if (m_ShowTooltip)
		{
			explorationComponentEntity.ShowTooltip();
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnFocusChangedImpl(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void SetResourcesNavigation()
	{
		m_ResourcesNavigationBehavior.Clear();
		IEnumerable<IConsoleNavigationEntity> spaceResourcesNavigationEntities = m_Page.GetSpaceResourcesNavigationEntities();
		if (spaceResourcesNavigationEntities.Any())
		{
			m_ResourcesNavigationBehavior.SetEntitiesHorizontal(spaceResourcesNavigationEntities.ToList());
		}
		if (m_ResourcesMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void OpenColonyProjects()
	{
		if (!m_LockInput)
		{
			base.ViewModel.OpenColonyProjects();
		}
	}

	private void ShowResources()
	{
		if (!m_LockInput)
		{
			m_NavigationBehaviour.UnFocusCurrentEntity();
			m_ResourcesMode.Value = true;
			SetResourcesNavigation();
			GamePad.Instance.PushLayer(m_ResourcesInputLayer);
			m_ResourcesNavigationBehavior.FocusOnFirstValidEntity();
		}
	}

	private void CloseResources()
	{
		m_ResourcesNavigationBehavior.UnFocusCurrentEntity();
		m_ResourcesMode.Value = false;
		TooltipHelper.HideTooltip();
		GamePad.Instance.PopLayer(m_ResourcesInputLayer);
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	protected override void SetLockUIForDialogImpl(bool value)
	{
		m_Navigation.SetInteractable(!value);
	}
}
