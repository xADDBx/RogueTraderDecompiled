using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
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

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable((base.ViewModel as TooltipBrickBuffVM)?.Duration.Subscribe(delegate(string t)
		{
			m_Duration.text = t;
			if (t == "")
			{
				base.gameObject.SetActive(value: false);
			}
		}));
		if (base.ViewModel is TooltipBrickBuffVM tooltipBrickBuffVM)
		{
			m_SourcePanel.SetActive(!string.IsNullOrEmpty(tooltipBrickBuffVM.SourceName));
			m_SourceName.text = tooltipBrickBuffVM.SourceName;
			m_StackText.text = tooltipBrickBuffVM.Stack;
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
