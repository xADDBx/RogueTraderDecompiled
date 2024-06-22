using Kingmaker.Code.UI.MVVM.VM.Settings.Menu;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Menu;

public class SettingsMenuEntityPCView : SelectionGroupEntityView<SettingsMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[Header("Scale Settings")]
	[SerializeField]
	private float m_MaxWidth = 225f;

	[SerializeField]
	private float m_SetScaleWidth = 0.9f;

	[SerializeField]
	private float m_SetCharacterSpacing = -25f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Title.Subscribe(SetText));
	}

	private void SetText(string text)
	{
		m_Title.text = text;
		DelayedInvoker.InvokeInFrames(delegate
		{
			RectTransform rectTransform = m_Title.transform as RectTransform;
			if (!(rectTransform == null))
			{
				bool flag = rectTransform.sizeDelta.x <= m_MaxWidth;
				rectTransform.localScale = new Vector3(flag ? 1f : m_SetScaleWidth, 1f, 1f);
				m_Title.characterSpacing = (flag ? 0f : m_SetCharacterSpacing);
			}
		}, 1);
	}
}
