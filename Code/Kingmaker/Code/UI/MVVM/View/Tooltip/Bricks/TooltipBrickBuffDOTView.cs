using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickBuffDOTView : TooltipBaseBrickView<TooltipBrickBuffDOTVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private TextMeshProUGUI m_DamageText;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Title.text = UIStrings.Instance.Tooltips.Damage.Text;
		m_Description.text = UIStrings.Instance.Tooltips.EveryRound.Text;
		m_DamageText.text = base.ViewModel.Damage;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TooltipBrickBuffDOTVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TooltipBrickBuffDOTVM;
	}
}
