using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPlanetResourceImageView : ViewBase<TooltipBrickPlanetResourceImageVM>
{
	[SerializeField]
	private Image m_ResourceImage;

	[SerializeField]
	private TextMeshProUGUI m_ResourceName;

	[SerializeField]
	private Image m_MinerImage;

	public void Initialize(Sprite sprite, string resourceName, bool hasMiner)
	{
		base.gameObject.SetActive(value: true);
		m_ResourceImage.sprite = sprite;
		m_ResourceName.text = resourceName;
		m_MinerImage.enabled = hasMiner;
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
