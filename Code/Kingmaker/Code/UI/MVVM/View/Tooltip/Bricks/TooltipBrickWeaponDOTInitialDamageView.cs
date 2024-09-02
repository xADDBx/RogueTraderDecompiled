using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickWeaponDOTInitialDamageView : TooltipBaseBrickView<TooltipBrickWeaponDOTInitialDamageVM>, IWidgetView
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private TextMeshProUGUI m_DamageTitle;

	[SerializeField]
	private TextMeshProUGUI m_DamageValue;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Name;
		m_DamageValue.text = base.ViewModel.Damage;
		m_DamageTitle.text = UIStrings.Instance.Tooltips.Damage.Text;
		m_Description.text = UIStrings.Instance.Tooltips.InitialDamage.Text;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TooltipBrickWeaponDOTInitialDamageVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TooltipBrickWeaponDOTInitialDamageVM;
	}
}
