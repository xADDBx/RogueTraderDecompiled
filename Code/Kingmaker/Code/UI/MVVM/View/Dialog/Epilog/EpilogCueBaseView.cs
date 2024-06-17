using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Epilog;

public class EpilogCueBaseView : ViewBase<CueVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	private DialogColors m_DialogColors;

	private bool m_IsInit;

	[Header("First letter")]
	[SerializeField]
	private TMP_FontAsset m_FirstLetterFont;

	[SerializeField]
	private Material m_FirstLetterFontMaterial;

	[SerializeField]
	private Color m_FirstLetterColor = Color.black;

	[SerializeField]
	private int m_FirstLetterSize = 170;

	[SerializeField]
	private int m_FirstLetterVOffset;

	public void Initialize(DialogColors dialogColors)
	{
		if (!m_IsInit)
		{
			m_DialogColors = dialogColors;
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Show();
		m_Text.text = UIUtility.GetBookFormat(base.ViewModel.GetCueText(m_DialogColors), m_FirstLetterFont, m_FirstLetterColor, m_FirstLetterSize, m_FirstLetterVOffset, m_FirstLetterFontMaterial);
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}
}
