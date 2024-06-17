using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Space.Base;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Space.Console;

public class ShipHealthAndRepairConsoleView : ShipHealthAndRepairBaseView, IStarSystemShipMovementHandler, ISubscriber
{
	[Header("Console Part")]
	[SerializeField]
	private ConsoleHint m_RepairHint;

	private readonly ReactiveProperty<bool> m_ShipIsMoving = new ReactiveProperty<bool>();

	public void AddInput(InputLayer inputLayer)
	{
		AddDisposable(m_RepairHint.Bind(inputLayer.AddButton(delegate
		{
			TryRepair();
		}, 10, base.ViewModel.CanRepair, InputActionEventType.ButtonJustLongPressed)));
		m_RepairHint.SetLabel(UIStrings.Instance.SystemMap.RepairHullSimple);
	}

	private void TryRepair()
	{
		if (!base.ViewModel.CanRepair.Value)
		{
			return;
		}
		using (GameLogContext.Scope)
		{
			int num = ((base.ViewModel.ScrapWeHave.Value < Game.Instance.Player.Scrap.ScrapNeededForFullRepair) ? base.ViewModel.ScrapWeHave.Value : (base.ViewModel.MaxShipHealth.Value - base.ViewModel.CurrentShipHealth.Value));
			num = ((num > 0) ? num : 0);
			UIUtility.ShowMessageBox(string.Format(UIStrings.Instance.SystemMap.RepairHullDescription, num, (base.ViewModel.ScrapWeHave.Value < Game.Instance.Player.Scrap.ScrapNeededForFullRepair) ? base.ViewModel.ScrapWeHave.Value : base.ViewModel.ScrapNeedForRepair.Value), DialogMessageBoxBase.BoxType.Dialog, OnRepair);
		}
	}

	private void OnRepair(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			if (base.ViewModel.ScrapWeHave.Value < Game.Instance.Player.Scrap.ScrapNeededForFullRepair)
			{
				base.ViewModel.RepairShipForAllScrap();
			}
			else
			{
				base.ViewModel.RepairShipFull();
			}
			SetButtonRepairHover(state: false, forceClose: true, needToFillBack: false, afterRepair: true);
		}
		else
		{
			SetButtonRepairHover(state: false, forceClose: true);
		}
	}

	public void HandleStarSystemShipMovementStarted()
	{
		m_ShipIsMoving.Value = true;
	}

	public void HandleStarSystemShipMovementEnded()
	{
		m_ShipIsMoving.Value = false;
	}
}
