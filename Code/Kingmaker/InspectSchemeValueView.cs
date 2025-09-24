using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker;

public class InspectSchemeValueView : ViewBase<InspectSchemeValueVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate, IMonoBehaviour
{
	[FormerlySerializedAs("ArmourAndShieldValueType")]
	[SerializeField]
	private ArmourAndShieldValueType m_ArmourAndShieldValueType;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private OwlcatMultiButton m_NavigationEntity;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Value.text = base.ViewModel.Value;
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
		m_Value.text = string.Empty;
	}

	public ArmourAndShieldValueType GetArmourAndShieldValue()
	{
		return m_ArmourAndShieldValueType;
	}

	public void SetFocus(bool value)
	{
		m_NavigationEntity.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_NavigationEntity.IsValid();
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return this;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}
}
