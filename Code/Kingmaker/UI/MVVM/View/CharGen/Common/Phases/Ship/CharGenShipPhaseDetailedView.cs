using System;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Ship;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Ship;

public class CharGenShipPhaseDetailedView : CharGenPhaseDetailedView<CharGenShipPhaseVM>
{
	[Header("Ship Name")]
	[SerializeField]
	protected ScrambledTMP m_ShipName;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	[Header("Selector")]
	[SerializeField]
	protected CharGenShipPhaseSelectorView m_CharGenShipPhaseSelectorView;

	private CharGenShipDollRoom ShipRoom => UIDollRooms.Instance.Or(null)?.CharGenShipDollRoom;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_InfoView.Bind(base.ViewModel.InfoVM);
		m_CharGenShipPhaseSelectorView.Bind(base.ViewModel.ShipSelectionGroup);
		AddDisposable(base.ViewModel.SelectedShipEntity.Subscribe(HandleSelectedShip));
		AddDisposable(base.ViewModel.ShipName.Subscribe(delegate(string value)
		{
			m_ShipName.SetText(string.Empty, value);
		}));
		AddDisposable(base.ViewModel.InterruptHandler.Subscribe(base.ViewModel.ShowChangeNameMessageBox));
	}

	protected override void DestroyViewImplementation()
	{
		TooltipHelper.HideInfo();
		base.DestroyViewImplementation();
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter)
	{
	}

	protected void GenerateRandomName()
	{
		base.ViewModel.SetName(base.ViewModel.GetRandomName());
	}

	private void HandleSelectedShip(CharGenShipItemVM shipItemVM)
	{
		if (shipItemVM == null)
		{
			return;
		}
		try
		{
			ShipRoom.SetupShip(shipItemVM.ChargenUnit.Unit);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}
}
