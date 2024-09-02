using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickBuffView : TooltipBrickFeatureView, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Duration;

	[Header("SourceBlock")]
	[SerializeField]
	private GameObject m_SourcePanel;

	[SerializeField]
	private TextMeshProUGUI m_SourceName;

	[SerializeField]
	private TextMeshProUGUI m_StackText;

	[Header("DOTBlock")]
	[SerializeField]
	private GameObject m_DOTPanel;

	[SerializeField]
	private TextMeshProUGUI m_DOTDescription;

	[SerializeField]
	private TextMeshProUGUI m_DOTDamage;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable((base.ViewModel as TooltipBrickBuffVM)?.Duration.Subscribe(delegate(string t)
		{
			m_Duration.text = t;
			if (t == string.Empty)
			{
				base.gameObject.SetActive(value: false);
			}
		}));
		if (!(base.ViewModel is TooltipBrickBuffVM tooltipBrickBuffVM))
		{
			return;
		}
		m_SourcePanel.Or(null)?.SetActive(!string.IsNullOrWhiteSpace(tooltipBrickBuffVM.SourceName));
		if (m_SourceName != null)
		{
			m_SourceName.text = tooltipBrickBuffVM.SourceName;
		}
		if (m_StackText != null)
		{
			m_StackText.text = tooltipBrickBuffVM.Stack;
		}
		m_DOTPanel.SetActive(tooltipBrickBuffVM.IsDOT);
		if (tooltipBrickBuffVM.IsDOT)
		{
			m_DOTDescription.text = tooltipBrickBuffVM.DOTDesc;
			AddDisposable(tooltipBrickBuffVM.DOTDamage.Subscribe(delegate(string value)
			{
				m_DOTDamage.text = value;
				base.gameObject.SetActive(!string.IsNullOrEmpty(value));
			}));
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TooltipBrickBuffVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TooltipBrickBuffVM;
	}
}
