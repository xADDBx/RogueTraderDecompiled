using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using TMPro;
using UnityEngine;

namespace Kingmaker;

public class TooltipBrickShipInspectSchemeView : TooltipBaseBrickView<TooltipBrickShipInspectSchemeVM>
{
	[SerializeField]
	public TextMeshProUGUI m_Title;

	[SerializeField]
	protected List<InspectSchemeValueView> Values;

	[SerializeField]
	public TextMeshProUGUI m_ArmourTitle;

	[SerializeField]
	public TextMeshProUGUI m_ShieldsTitle;

	[SerializeField]
	public TextMeshProUGUI m_PortTitle;

	[SerializeField]
	public TextMeshProUGUI m_AftTitle;

	[SerializeField]
	public TextMeshProUGUI m_ForeTitle;

	[SerializeField]
	public TextMeshProUGUI m_StarboardTitle;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Title.text = base.ViewModel.Title;
		m_ArmourTitle.text = base.ViewModel.ArmorTitle;
		m_ShieldsTitle.text = base.ViewModel.ShieldsTitle;
		m_PortTitle.text = base.ViewModel.PortTitle;
		m_AftTitle.text = base.ViewModel.AftTitle;
		m_ForeTitle.text = base.ViewModel.ForeTitle;
		m_StarboardTitle.text = base.ViewModel.StarboardTitle;
		foreach (KeyValuePair<ArmourAndShieldValueType, InspectSchemeValueVM> kvp in base.ViewModel.BrickShipInspectSchemes)
		{
			InspectSchemeValueView inspectSchemeValueView = Values.FirstOrDefault((InspectSchemeValueView v) => v.GetArmourAndShieldValue() == kvp.Key);
			if (inspectSchemeValueView != null)
			{
				inspectSchemeValueView.Bind(kvp.Value);
			}
		}
	}
}
