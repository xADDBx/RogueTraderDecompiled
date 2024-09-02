using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.View.CombatLog.Console;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.View.Exploration.PC;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Console;

public class ExplorationConsoleView : ExplorationBaseView
{
	[Header("Components")]
	[SerializeField]
	protected ExplorationResourceListConsoleView m_ExplorationResourceListConsoleView;

	[SerializeField]
	private ExplorationColonyStatsWrapperConsoleView m_ExplorationColonyStatsWrapperConsoleView;

	[SerializeField]
	private ExplorationColonyTraitsWrapperConsoleView m_ExplorationColonyTraitsWrapperConsoleView;

	[SerializeField]
	private ExplorationPointOfInterestListConsoleView m_ExplorationPointOfInterestListView;

	[SerializeField]
	private ExplorationColonyRewardsWrapperConsoleView m_ExplorationColonyRewardsWrapperConsoleView;

	[SerializeField]
	private ExplorationColonyEventsWrapperConsoleView m_ExplorationColonyEventsWrapperConsoleView;

	[Header("Projects")]
	[SerializeField]
	private ExplorationColonyProjectsWrapperConsoleView m_ExplorationColonyProjectsWrapperPCView;

	[SerializeField]
	private ExplorationColonyProjectsBuiltListWrapperConsoleView m_ExplorationColonyProjectsBuiltListWrapperConsoleView;

	[Header("Bark Part")]
	[SerializeField]
	private ExplorationSpaceBarksHolderPCView m_ExplorationSpaceBarksHolderPCView;

	[Header("All Resources")]
	[SerializeField]
	private ExplorationSpaceResourcesWrapperConsoleView m_ExplorationSpaceResourcesWrapperConsoleView;

	[Header("Input")]
	[SerializeField]
	protected ExplorationScanHintWrapperConsoleView m_ExplorationScanHintWrapperConsoleView;

	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private CombatLogConsoleView m_CombatLogConsoleView;

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

	private readonly ReactiveProperty<bool> m_IsScanning = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasMultipleBlocks = new ReactiveProperty<bool>();

	private bool m_ShowTooltip;

	private InputLayer m_ResourcesInputLayer;

	private GridConsoleNavigationBehaviour m_ResourcesNavigationBehavior;

	private readonly BoolReactiveProperty m_ResourcesMode = new BoolReactiveProperty();

	private Dictionary<NavigationBlock, List<IConsoleNavigationEntity>> m_Entities = new Dictionary<NavigationBlock, List<IConsoleNavigationEntity>>();

	private NavigationBlock m_CurrentNavigationBlock;

	protected override void InitializeImpl()
	{
		m_ExplorationColonyTraitsWrapperConsoleView.Initialize();
		m_ExplorationColonyEventsWrapperConsoleView.Initialize();
		m_ExplorationColonyRewardsWrapperConsoleView.Initialize();
		m_ExplorationColonyProjectsWrapperPCView.Initialize();
		m_ExplorationColonyStatsWrapperConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ExplorationResourceListConsoleView.Bind(base.ViewModel.ExplorationResourceListVM);
		m_ExplorationSpaceResourcesWrapperConsoleView.Bind(base.ViewModel.ExplorationSpaceResourcesWrapperVM);
		m_ExplorationColonyStatsWrapperConsoleView.Bind(base.ViewModel.ExplorationColonyStatsWrapperVM);
		m_ExplorationColonyTraitsWrapperConsoleView.Bind(base.ViewModel.ExplorationColonyTraitsWrapperVM);
		m_ExplorationColonyEventsWrapperConsoleView.Bind(base.ViewModel.ExplorationColonyEventsWrapperVM);
		m_ExplorationColonyRewardsWrapperConsoleView.Bind(base.ViewModel.ExplorationColonyRewardsWrapperVM);
		m_ExplorationColonyProjectsWrapperPCView.Bind(base.ViewModel.ExplorationColonyProjectsWrapperVM);
		m_ExplorationColonyProjectsBuiltListWrapperConsoleView.Bind(base.ViewModel.ExplorationColonyProjectsBuiltListWrapperVM);
		m_ExplorationScanHintWrapperConsoleView.Bind(base.ViewModel.ExplorationScanButtonWrapperVM);
		m_ExplorationPointOfInterestListView.Bind(base.ViewModel.ExplorationPointOfInterestListVM);
		m_ExplorationSpaceBarksHolderPCView.Bind(base.ViewModel.ExplorationSpaceBarksHolderVM);
	}

	protected override void DestroyViewImplementation()
	{
		m_ShowTooltip = false;
		base.DestroyViewImplementation();
	}

	protected override void HideWindowImpl()
	{
		m_ShowTooltip = false;
		TooltipHelper.HideTooltip();
	}

	protected override void ScanPlanetImpl()
	{
		m_IsScanning.Value = true;
	}

	protected override void ScanAnimationImpl()
	{
		m_IsScanning.Value = false;
	}

	protected override void ClearScanProgressImpl()
	{
		m_IsScanning.Value = false;
	}

	protected override void BuildNavigationImpl()
	{
		m_NavigationBehaviour.Clear();
		m_PointsNavigationBehaviour.Clear();
		m_Entities.Clear();
		if (base.ViewModel.IsExplored.Value)
		{
			List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
			AddEntities(list, m_ExplorationColonyEventsWrapperConsoleView.GetNavigationEntities());
			List<IConsoleNavigationEntity> pointsNavigation = GetPointsNavigation();
			List<IConsoleNavigationEntity> list2 = new List<IConsoleNavigationEntity>();
			AddEntities(list2, m_ExplorationResourceListConsoleView.GetNavigationEntities());
			AddEntities(list2, m_ExplorationColonyStatsWrapperConsoleView.GetNavigationEntities());
			AddEntities(list2, m_ExplorationColonyTraitsWrapperConsoleView.GetNavigationEntities());
			m_Entities.Add(NavigationBlock.Left, list);
			m_Entities.Add(NavigationBlock.Center, pointsNavigation);
			m_Entities.Add(NavigationBlock.Right, list2);
			m_NavigationBehaviour.AddColumn(list);
			m_NavigationBehaviour.AddColumn(new IConsoleNavigationEntity[1] { m_PointsNavigationBehaviour });
			m_NavigationBehaviour.AddColumn(list2);
			ResetHighlight();
			bool[] source = new bool[3]
			{
				BlockHasValidEntities(list),
				BlockHasValidEntities(pointsNavigation),
				BlockHasValidEntities(list2)
			};
			m_HasMultipleBlocks.Value = source.Count((bool b) => b) > 1;
		}
	}

	private static void AddEntities(List<IConsoleNavigationEntity> list, IEnumerable<IConsoleNavigationEntity> entities)
	{
		if (entities != null)
		{
			list.AddRange(entities);
		}
	}

	private List<IConsoleNavigationEntity> GetPointsNavigation()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		m_PointsNavigationBehaviour.AddEntities(m_ExplorationPointOfInterestListView.GetNavigationEntities());
		List<IFloatConsoleNavigationEntity> floatNavigationEntities = m_ExplorationColonyProjectsBuiltListWrapperConsoleView.GetFloatNavigationEntities();
		if (floatNavigationEntities != null)
		{
			m_PointsNavigationBehaviour.AddEntities(floatNavigationEntities);
		}
		m_PointsNavigationBehaviour.AddEntity(m_ResourceMinersView);
		list.AddRange(m_PointsNavigationBehaviour.Entities.Select((IConsoleEntity entry) => (IConsoleNavigationEntity)entry));
		return list;
	}

	private static bool BlockHasValidEntities(IEnumerable<IConsoleNavigationEntity> entities)
	{
		return entities.Any((IConsoleNavigationEntity x) => x.IsValid());
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		m_ExplorationScanHintWrapperConsoleView.CreateInput(inputLayer, base.ViewModel.IsExplored.Not().ToReactiveProperty());
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OnCloseClickDelegate();
		}, 9, m_IsScanning.Not().ToReactiveProperty()), UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			CancelScan();
		}, 9, m_IsScanning), UIStrings.Instance.CommonTexts.Cancel, ConsoleHintsWidget.HintPosition.Right));
		m_CombatLogConsoleView.AddInputToExploration(inputLayer);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InteractCurrentFocusedEntity();
		}, 8, m_CanInteract), UIStrings.Instance.SettingsUI.MenuConfirm));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(ToggleTooltip, 19, m_CanShowInfo, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddAxis(SwitchBlock, 2, base.ViewModel.IsExplored.And(m_HasMultipleBlocks).ToReactiveProperty(), repeat: true), UIStrings.Instance.CommonTexts.Select));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			OpenColonyProjects();
		}, 10, base.ViewModel.HasColony, InputActionEventType.ButtonJustReleased), UIStrings.Instance.ColonyProjectsTexts.OpenProjectsButton));
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			ShowResources();
		}, 11, InputActionEventType.ButtonJustReleased), UIStrings.Instance.GlobalMap.ShowResources));
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
			if (BlockHasValidEntities(value2))
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
				m_ExplorationColonyEventsWrapperConsoleView.ScrollList(entity3);
			}
		}
		else
		{
			m_ExplorationColonyTraitsWrapperConsoleView.ScrollList(entity2);
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
		IEnumerable<IConsoleNavigationEntity> navigationEntities = m_ExplorationSpaceResourcesWrapperConsoleView.GetNavigationEntities();
		if (navigationEntities.Any())
		{
			m_ResourcesNavigationBehavior.SetEntitiesHorizontal(navigationEntities.ToList());
		}
		if (m_ResourcesMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void OpenColonyProjects()
	{
		base.ViewModel.OpenColonyProjects();
	}

	private void ShowResources()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_ResourcesMode.Value = true;
		SetResourcesNavigation();
		GamePad.Instance.PushLayer(m_ResourcesInputLayer);
		m_ResourcesNavigationBehavior.FocusOnFirstValidEntity();
	}

	private void CloseResources()
	{
		m_ResourcesNavigationBehavior.UnFocusCurrentEntity();
		m_ResourcesMode.Value = false;
		TooltipHelper.HideTooltip();
		GamePad.Instance.PopLayer(m_ResourcesInputLayer);
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}
}
