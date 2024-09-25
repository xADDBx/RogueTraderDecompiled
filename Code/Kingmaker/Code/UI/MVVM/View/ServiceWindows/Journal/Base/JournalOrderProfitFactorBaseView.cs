using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalOrderProfitFactorBaseView : ViewBase<JournalOrderProfitFactorVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	[SerializeField]
	private Image m_Arrow;

	[SerializeField]
	private Sprite m_GreenArrow;

	[SerializeField]
	private Sprite m_RedArrow;

	[SerializeField]
	private OwlcatSelectable m_OwlcatSelectable;

	[SerializeField]
	private Image m_BackgroundImage;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite i)
		{
			m_Icon.sprite = i;
		}));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(float c)
		{
			m_Count.text = c.ToString();
		}));
		AddDisposable(base.ViewModel.ArrowDirection.Subscribe(delegate(int value)
		{
			m_OwlcatSelectable.SetFocus(value != 0);
			m_Arrow.gameObject.SetActive(value != 0);
			if (value != 0)
			{
				m_Arrow.sprite = ((value == 1) ? m_GreenArrow : m_RedArrow);
			}
		}));
		AddDisposable(m_BackgroundImage.SetTooltip(new TooltipTemplateProfitFactor(base.ViewModel.ProfitFactorVM), new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
