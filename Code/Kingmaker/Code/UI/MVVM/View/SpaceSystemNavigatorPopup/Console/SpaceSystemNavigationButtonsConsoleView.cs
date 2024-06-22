using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup.Base;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SpaceSystemNavigatorPopup.Console;

public class SpaceSystemNavigationButtonsConsoleView : SpaceSystemNavigationButtonsBaseView
{
	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_VisitHint;

	[SerializeField]
	private ConsoleHint m_TravelHint;

	[SerializeField]
	private ConsoleHint m_ScanHint;

	[SerializeField]
	private ConsoleHint m_UpgradeWayHint;

	[SerializeField]
	private ConsoleHint m_CreateWayHint;

	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_UpgradeWayNavigationBehaviour;

	private readonly BoolReactiveProperty m_IsVisitAvailable = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsScanHintAvailable = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsExitAvailable = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsTravelAvailable = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsUpgradeAvailable = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsUpgradeButtonLeftRightAvailable = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsCreateWayAvailable = new BoolReactiveProperty();

	private List<OwlcatButton> m_UpgradeButtonsEntities = new List<OwlcatButton>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CreateWayButton.OnFocusAsObservable().Subscribe(base.ShowCreateWayButtonHoverPanel));
		m_UpgradeButtons.ForEach(delegate(OwlcatButton b)
		{
			AddDisposable(b.OnFocusAsObservable().Subscribe(delegate(bool state)
			{
				ShowUpgradeWayButtonHoverPanel(b, state);
			}));
		});
		AddDisposable(EventBus.Subscribe(this));
		CheckButtons();
		BuildNavigation(GamePad.Instance.Layers.FirstOrDefault((InputLayer l) => l.ContextName == "SpaceGlobalMapInputLayer"));
	}

	private void CheckCurrentSystem()
	{
		SectorMapPassageEntity passage = base.ViewModel.GetPassage();
		m_EagleImage.gameObject.SetActive(!base.ViewModel.IsCurrentSystem && base.ViewModel.IsTravelNewSectorAvailable.Value);
		m_CreateWayButton.transform.parent.gameObject.SetActive(!base.ViewModel.IsCurrentSystem && passage == null && base.ViewModel.IsScannedFrom.Value);
		m_UpgradeButtons.FirstOrDefault()?.transform.parent.gameObject.SetActive(!base.ViewModel.IsCurrentSystem && passage != null && passage.CurrentDifficulty != 0 && base.ViewModel.IsScannedFrom.Value);
	}

	private void BuildNavigation(InputLayer inputLayer)
	{
		CreateInputImpl(inputLayer);
		AddDisposable(base.ViewModel.IsScanning.CombineLatest(base.ViewModel.IsTraveling, base.ViewModel.IsDialogActive, base.ViewModel.IsWayUpgrading, base.ViewModel.IsInformationWindowsInspectMode, base.ViewModel.IsWayCreating, (bool isScanning, bool isTraveling, bool isDialogActive, bool isWayUpgrading, bool isInfoWindowsInspectMode, bool isWayCreating) => new { isScanning, isTraveling, isDialogActive, isWayUpgrading, isInfoWindowsInspectMode, isWayCreating }).Subscribe(isLocked =>
		{
			bool locked = isLocked.isScanning || isLocked.isTraveling || isLocked.isDialogActive || isLocked.isWayUpgrading || isLocked.isInfoWindowsInspectMode || isLocked.isWayCreating;
			CheckButtons(locked);
		}));
	}

	private void CheckButtons(bool locked = false)
	{
		if (locked)
		{
			m_IsScanHintAvailable.Value = false;
			m_IsTravelAvailable.Value = false;
			m_IsCreateWayAvailable.Value = false;
			m_IsUpgradeAvailable.Value = false;
			m_IsVisitAvailable.Value = false;
			m_EagleImage.gameObject.SetActive(value: false);
			m_CreateWayButton.transform.parent.transform.gameObject.SetActive(base.ViewModel.IsWayCreating.Value);
			m_UpgradeButtons.FirstOrDefault()?.transform.parent.transform.gameObject.SetActive(base.ViewModel.IsWayUpgrading.Value);
			return;
		}
		if (base.ViewModel.IsTravelNewSectorAvailable.Value)
		{
			CheckUpgradeButtonsVisible();
		}
		m_IsScanHintAvailable.Value = !base.ViewModel.IsScannedFrom.Value && base.ViewModel.IsScanConsoleAvailable.Value;
		m_IsTravelAvailable.Value = base.ViewModel.IsTravelNewSectorAvailable.Value && base.ViewModel.IsTravelAvailable.Value;
		SectorMapPassageEntity passage = base.ViewModel.GetPassage();
		bool flag = base.ViewModel.IsTravelAvailable.Value && passage == null && !base.ViewModel.IsCurrentSystem && base.ViewModel.IsScannedFrom.Value && base.ViewModel.CurrentValueOfResources.Value >= base.ViewModel.CreateWayCost.Value;
		m_IsCreateWayAvailable.Value = flag;
		m_CreateWayButton.Interactable = flag;
		bool flag2 = passage != null && !base.ViewModel.IsCurrentSystem && base.ViewModel.IsTravelAvailable.Value && passage.CurrentDifficulty != 0 && base.ViewModel.IsScannedFrom.Value;
		m_IsUpgradeAvailable.Value = flag2;
		if (flag2)
		{
			CheckUpgradeButtonsVisible();
		}
		m_IsVisitAvailable.Value = base.ViewModel.IsVisitAvailable.Value;
		CheckCurrentSystem();
		if (m_IsVisitAvailable.Value)
		{
			m_CreateWayButton.transform.parent.transform.gameObject.SetActive(value: false);
		}
		if (flag2 || flag)
		{
			SetUpgradeWayNavigation();
			return;
		}
		ShowCreateWayButtonHoverPanel(state: false);
		ShowUpgradeWayButtonHoverPanel(null, state: false);
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		m_InputLayer = inputLayer;
		if (m_InputLayer != null)
		{
			AddDisposable(m_VisitHint.Bind(m_InputLayer.AddButton(delegate
			{
				HandleVisitSystem();
			}, 8, m_IsVisitAvailable)));
			m_VisitHint.SetLabel(UIStrings.Instance.ColonizationTexts.ColonyManagementVisitColonyButton);
			AddDisposable(m_TravelHint.Bind(m_InputLayer.AddButton(delegate
			{
				HandleTravelToSystem();
			}, 8, m_IsTravelAvailable)));
			AddDisposable(m_InputLayer.AddButton(delegate
			{
				base.ViewModel.CheckPingCoop();
			}, 8, m_IsTravelAvailable.Not().ToReactiveProperty()));
			m_TravelHint.SetLabel(UIStrings.Instance.GlobalMap.Travel);
			AddDisposable(m_ScanHint.Bind(m_InputLayer.AddButton(delegate
			{
				HandleScanSystem();
			}, 10, m_IsScanHintAvailable)));
			m_ScanHint.SetLabel(UIStrings.Instance.GlobalMap.ScanForRoutes);
			AddDisposable(m_UpgradeWayHint.Bind(m_InputLayer.AddButton(delegate
			{
				UpgradeWay();
			}, 10, m_IsUpgradeAvailable, InputActionEventType.ButtonJustLongPressed)));
			m_UpgradeWayHint.SetLabel(UIStrings.Instance.GlobalMap.UpgradeRoute);
			AddDisposable(m_CreateWayHint.Bind(m_InputLayer.AddButton(delegate
			{
				CreateWay();
			}, 10, m_IsCreateWayAvailable, InputActionEventType.ButtonJustLongPressed)));
			m_CreateWayHint.SetLabel(UIStrings.Instance.GlobalMap.CreateWay);
			SetUpgradeWayNavigation();
			AddDisposable(m_PreviousHint.Bind(m_InputLayer.AddButton(delegate
			{
				ChangeUpgradeButton(isPrev: true);
			}, 14, m_IsUpgradeAvailable.And(m_IsUpgradeButtonLeftRightAvailable).And(base.ViewModel.SomeServiceWindowIsOpened.Not()).ToReactiveProperty())));
			AddDisposable(m_NextHint.Bind(m_InputLayer.AddButton(delegate
			{
				ChangeUpgradeButton(isPrev: false);
			}, 15, m_IsUpgradeAvailable.And(m_IsUpgradeButtonLeftRightAvailable).And(base.ViewModel.SomeServiceWindowIsOpened.Not()).ToReactiveProperty())));
		}
	}

	private void HandleVisitSystem()
	{
		base.ViewModel?.VisitSystem();
	}

	private void HandleTravelToSystem()
	{
		m_UpgradeButtonsEntities.ForEach(delegate(OwlcatButton b)
		{
			b.SetFocus(value: false);
		});
		base.ViewModel?.SpaceSystemTravelToSystem();
	}

	private void UpgradeWay()
	{
		OwlcatButton value = m_UpgradeButtons.FirstOrDefault((OwlcatButton b) => b == m_UpgradeButtonsEntities.FirstOrDefault((OwlcatButton e) => e.IsFocus));
		UpgradeWay(delegate
		{
			HandleLowerSectorMapPassageDifficulty();
			SetUpgradeWayNavigation();
			CheckButtons();
		}, (SectorMapPassageEntity.PassageDifficulty)m_UpgradeButtons.IndexOf(value));
		m_UpgradeButtonsEntities.ForEach(delegate(OwlcatButton b)
		{
			b.SetFocus(value: false);
		});
	}

	private void CreateWay()
	{
		CreateWay(delegate
		{
			base.ViewModel.SpaceSystemCreateWay();
			SetUpgradeWayNavigation();
		});
		m_UpgradeButtonsEntities.ForEach(delegate(OwlcatButton b)
		{
			b.SetFocus(value: false);
		});
	}

	private void SetUpgradeWayNavigation()
	{
		List<OwlcatButton> list = new List<OwlcatButton> { m_CreateWayButton };
		list.AddRange(m_UpgradeButtons);
		m_UpgradeButtonsEntities.Clear();
		m_UpgradeButtonsEntities = list.Where((OwlcatButton e) => e.isActiveAndEnabled && e.Interactable).ToList();
		m_UpgradeButtonsEntities.LastOrDefault()?.SetFocus(value: true);
		m_IsUpgradeButtonLeftRightAvailable.Value = m_IsUpgradeAvailable.Value && m_UpgradeButtonsEntities.Count > 1;
	}

	private void ChangeUpgradeButton(bool isPrev)
	{
		int num = m_UpgradeButtonsEntities.IndexOf(m_UpgradeButtonsEntities.FirstOrDefault((OwlcatButton e) => e.IsFocus));
		m_UpgradeButtonsEntities[num].SetFocus(value: false);
		int index = (isPrev ? Mathf.Min(num + 1, m_UpgradeButtonsEntities.Count - 1) : Mathf.Max(num - 1, 0));
		m_UpgradeButtonsEntities[index].SetFocus(value: true);
	}

	private void HandleScanSystem()
	{
		m_IsScanHintAvailable.Value = false;
		m_IsVisitAvailable.Value = false;
		m_IsTravelAvailable.Value = false;
		base.ViewModel.ScanSystem();
	}

	protected override void WayIsOpen(bool open)
	{
		if (!open)
		{
			m_CreateWayButton.Interactable = base.ViewModel.CurrentValueOfResources.Value >= base.ViewModel.CreateWayCost.Value && base.ViewModel.IsScannedFrom.Value;
		}
		CheckButtons();
	}
}
