using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap;

public class PlanetTraitsView : ViewBase<TooltipBrickPlanetTraitsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	public void Initialize(string title, string description)
	{
		base.gameObject.SetActive(value: true);
		m_Description.text = title;
	}

	protected override void BindViewImplementation()
	{
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		WidgetFactory.DisposeWidget(this);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}
}
