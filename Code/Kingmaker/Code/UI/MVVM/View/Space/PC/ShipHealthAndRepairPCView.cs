using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Space.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class ShipHealthAndRepairPCView : ShipHealthAndRepairBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private Image m_HPBarTooltipObject;

	[SerializeField]
	private Image m_VoidshipHealthTextTooltipObject;

	[SerializeField]
	private OwlcatButton m_ButtonRepair;

	[Header("Dialog Box")]
	[SerializeField]
	private FadeAnimator m_DialogBoxAnimator;

	[SerializeField]
	private TextMeshProUGUI m_DialogBoxDescription;

	[SerializeField]
	private OwlcatButton m_DialogBoxOkButton;

	[SerializeField]
	private OwlcatButton m_DialogBoxAbortButton;

	[SerializeField]
	private TextMeshProUGUI m_DialogBoxOkLabel;

	[SerializeField]
	private TextMeshProUGUI m_DialogBoxAbortLabel;

	[SerializeField]
	private RectTransform TooltipPlace;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DialogBoxAnimator.gameObject.SetActive(value: false);
		m_DialogBoxOkLabel.text = UIStrings.Instance.CommonTexts.Accept;
		m_DialogBoxAbortLabel.text = UIStrings.Instance.CommonTexts.Cancel;
		AddDisposable(m_HPBarTooltipObject.SetGlossaryTooltip("HullIntegritySpace", new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true, isEncyclopedia: false, TooltipPlace)));
		AddDisposable(m_VoidshipHealthTextTooltipObject.SetGlossaryTooltip("HullIntegritySpace", new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true, isEncyclopedia: false, TooltipPlace)));
		AddDisposable(m_ButtonRepair.OnLeftClickAsObservable().Subscribe(delegate
		{
			OpenRepairDialog();
		}));
		AddDisposable(m_ButtonRepair.OnHoverAsObservable().Subscribe(delegate(bool state)
		{
			SetButtonRepairHover(state, forceClose: false, needToFillBack: true, afterRepair: false, m_DialogBoxAnimator.gameObject);
		}));
		AddDisposable(m_DialogBoxOkButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CloseRepairDialog(repair: true);
		}));
		AddDisposable(m_DialogBoxAbortButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CloseRepairDialog(repair: false);
		}));
		AddDisposable(base.ViewModel.IsLocked.Subscribe(delegate(bool val)
		{
			m_ButtonRepair.gameObject.SetActive(!val);
		}));
	}

	private void OpenRepairDialog()
	{
		if (base.ViewModel.CanRepair.Value && base.ViewModel.CurrentShipHealth.Value != base.ViewModel.MaxShipHealth.Value && base.ViewModel.ScrapWeHave.Value != 0)
		{
			m_DialogBoxAnimator.AppearAnimation();
			int num = ((base.ViewModel.ScrapWeHave.Value < Game.Instance.Player.Scrap.ScrapNeededForFullRepair) ? base.ViewModel.ScrapWeHave.Value : (base.ViewModel.MaxShipHealth.Value - base.ViewModel.CurrentShipHealth.Value));
			num = ((num > 0) ? num : 0);
			m_DialogBoxDescription.text = string.Format(UIStrings.Instance.SystemMap.RepairHullDescription, num, (base.ViewModel.ScrapWeHave.Value < Game.Instance.Player.Scrap.ScrapNeededForFullRepair) ? base.ViewModel.ScrapWeHave.Value : base.ViewModel.ScrapNeedForRepair.Value);
		}
	}

	private void CloseRepairDialog(bool repair)
	{
		if (repair)
		{
			if (base.ViewModel.ScrapWeHave.Value < Game.Instance.Player.Scrap.ScrapNeededForFullRepair)
			{
				base.ViewModel.RepairShipForAllScrap();
			}
			else
			{
				base.ViewModel.RepairShipFull();
			}
			m_DialogBoxAnimator.DisappearAnimation();
			SetButtonRepairHover(state: false, forceClose: true, needToFillBack: false, afterRepair: true, m_DialogBoxAnimator.gameObject);
		}
		else
		{
			m_DialogBoxAnimator.DisappearAnimation();
			SetButtonRepairHover(state: false, forceClose: true, needToFillBack: true, afterRepair: false, m_DialogBoxAnimator.gameObject);
		}
	}
}
