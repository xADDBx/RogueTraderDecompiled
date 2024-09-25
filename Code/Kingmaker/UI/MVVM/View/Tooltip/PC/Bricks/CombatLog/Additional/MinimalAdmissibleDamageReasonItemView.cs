using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog.Additional;

public class MinimalAdmissibleDamageReasonItemView : ViewBase<MinimalAdmissibleDamageReasonItemVM>, IWidgetView
{
	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_IconImage.color = Color.black;
		m_Text.text = base.ViewModel.Text;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as MinimalAdmissibleDamageReasonItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is MinimalAdmissibleDamageReasonItemVM;
	}
}
