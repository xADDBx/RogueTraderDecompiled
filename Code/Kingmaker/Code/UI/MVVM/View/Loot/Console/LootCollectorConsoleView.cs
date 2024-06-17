using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class LootCollectorConsoleView : LootCollectorView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_CollectAllHint;

	[SerializeField]
	private ConsoleHint m_CollectAllLongHint;

	[SerializeField]
	private ConsoleHint m_CloseHint;

	[SerializeField]
	private ConsoleHint[] m_ChangeViewHints;

	[SerializeField]
	private ConsoleHint m_AllToCargo;

	[SerializeField]
	private ConsoleHint m_AllToInventory;

	[SerializeField]
	private CanvasGroup m_AllToCargoBlink;

	[SerializeField]
	private CanvasGroup m_AllToInventoryBlink;

	[SerializeField]
	private OwlcatMultiButton m_ToCargoTitleFrame;

	[SerializeField]
	private OwlcatMultiButton m_ToInventoryTitleFrame;

	private SimpleConsoleNavigationEntity m_ToCargoTitle;

	private SimpleConsoleNavigationEntity m_ToInventoryTitle;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private BoolReactiveProperty m_IsFocused = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CollectAllHint.gameObject.SetActive(value: true);
		m_ToCargoTitle = new SimpleConsoleNavigationEntity(m_ToCargoTitleFrame, new TooltipTemplateHint(UIStrings.Instance.LootWindow.TrashLootObjectDescr.Text));
		m_ToInventoryTitle = new SimpleConsoleNavigationEntity(m_ToInventoryTitleFrame, new TooltipTemplateHint(UIStrings.Instance.LootWindow.ItemsLootObjectDescr.Text));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.Loot.LootUpdated, delegate
		{
			ForceScrollToObj(m_NavigationBehaviour.DeepestNestedFocus);
		}));
	}

	public void AddInput(InputLayer inputLayer)
	{
		AddDisposable(m_CollectAllHint.Bind(inputLayer.AddButton(CollectAll, 10, base.ViewModel.NoLoot.Not().And(base.ViewModel.ExtendedView.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
		m_CollectAllHint.SetLabel(UIStrings.Instance.LootWindow.CollectAll);
		AddDisposable(m_CollectAllLongHint.Bind(inputLayer.AddButton(CollectAllAndScrollToTop, 10, base.ViewModel.NoLoot.Not().And(base.ViewModel.ExtendedView).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
		m_CollectAllLongHint.SetLabel(UIStrings.Instance.LootWindow.CollectAll);
		AddDisposable(inputLayer.AddButton(delegate
		{
			ChangeView();
		}, 18));
		ConsoleHint[] changeViewHints = m_ChangeViewHints;
		foreach (ConsoleHint consoleHint in changeViewHints)
		{
			AddDisposable(consoleHint.Bind(inputLayer.AddButton(delegate
			{
			}, 18)));
		}
		AddDisposable(m_AllToCargo.Bind(inputLayer.AddButton(delegate
		{
			AddAllToCargo();
		}, 14, m_IsFocused)));
		AddDisposable(m_AllToInventory.Bind(inputLayer.AddButton(delegate
		{
			AddAllToInventory();
		}, 15, m_IsFocused)));
		AddDisposable(m_CloseHint.Bind(inputLayer.AddButton(delegate
		{
		}, 9, base.ViewModel.NoLoot)));
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(ForceScrollToObj));
	}

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		m_NavigationBehaviour.AddColumn(new IConsoleNavigationEntity[2]
		{
			m_ToCargoTitle,
			m_LootToCargo.SlotsGroup.GetNavigation()
		});
		m_NavigationBehaviour.AddColumn(new IConsoleNavigationEntity[2]
		{
			m_ToInventoryTitle,
			m_LootToInventory.SlotsGroup.GetNavigation()
		});
		return m_NavigationBehaviour;
	}

	protected override void Hide()
	{
		m_NavigationBehaviour?.Clear();
		base.Hide();
	}

	private void ForceScrollToObj(IConsoleEntity entity)
	{
		LootSlotConsoleView lootSlotConsoleView = entity as LootSlotConsoleView;
		m_IsFocused.Value = lootSlotConsoleView != null;
		if (m_IsFocused.Value)
		{
			RectTransform targetRect = lootSlotConsoleView.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void CollectAllAndScrollToTop(InputActionEventData data)
	{
		m_ScrollRect.ScrollToTop();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		CollectAll(data);
	}

	private void CollectAll(InputActionEventData data)
	{
		base.ViewModel.CollectAll();
		UISounds.Instance.Sounds.Buttons.LootCollectAllButtonClick.Play();
	}

	public override void AddAllToCargo()
	{
		m_AllToCargoBlink.gameObject.SetActive(value: true);
		m_AllToCargoBlink.alpha = 1f;
		m_AllToCargoBlink.DOFade(0f, 0.5f).SetLoops(1).SetEase(Ease.OutSine)
			.SetUpdate(isIndependentUpdate: true);
		base.AddAllToCargo();
	}

	public override void AddAllToInventory()
	{
		m_AllToInventoryBlink.gameObject.SetActive(value: true);
		m_AllToInventoryBlink.alpha = 1f;
		m_AllToInventoryBlink.DOFade(0f, 0.5f).SetLoops(1).SetEase(Ease.OutSine)
			.SetUpdate(isIndependentUpdate: true);
		base.AddAllToInventory();
	}
}
