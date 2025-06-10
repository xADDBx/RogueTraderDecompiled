using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker;

public class TooltipBrickShortLabelView : TooltipBaseBrickView<TooltipBrickShortLabelVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Stat;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.StatShortName.Subscribe(delegate
		{
			m_Stat.text = base.ViewModel.StatShortName.Value;
		}));
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TooltipBrickShortLabelVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TooltipBrickShortLabelVM;
	}
}
