using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockImagePCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockImageVM>
{
	[SerializeField]
	private Image m_Image;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (base.ViewModel.Image == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		m_Image.sprite = base.ViewModel.Image;
		m_Image.color = Color.white;
	}

	protected override void DestroyViewImplementation()
	{
		m_Image.sprite = null;
		m_Image.color = new Color(0f, 0f, 0f, 0f);
		base.DestroyViewImplementation();
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return null;
	}
}
