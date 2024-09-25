using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Bark.PC;

public class BarkBlockView<T> : ViewBase<T> where T : BaseBarkVM
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private RectTransform m_BarkContainer;

	[SerializeField]
	private Vector2 m_ContainerPaddings;

	[SerializeField]
	protected FadeAnimator FadeAnimator;

	[SerializeField]
	private float m_BaseFontSize = 20f;

	private static float FontMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	protected override void BindViewImplementation()
	{
		m_Text.fontSize = m_BaseFontSize * FontMultiplier;
		AddDisposable(base.ViewModel.Text.Subscribe(delegate(string text)
		{
			float num = UIUtility.CalculateBarkWidth(text, m_Text.fontSize) + m_ContainerPaddings.x;
			int num2 = (int)Mathf.Ceil((float)text.Length * m_Text.fontSize * 0.58f / num);
			float a = (float)(int)(m_Text.fontSize * 1.1f * ((float)text.Length * m_Text.fontSize * 0.58f / num + 1f)) + m_ContainerPaddings.y;
			num = Mathf.Max(num, 0f);
			a = Mathf.Max(a, 0f);
			m_BarkContainer.sizeDelta = new Vector2(num, a);
			if (text.Length <= 25 || num2 <= 3)
			{
				text = "<align=\"center\">" + text + "</align>";
			}
			m_Text.text = text;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
