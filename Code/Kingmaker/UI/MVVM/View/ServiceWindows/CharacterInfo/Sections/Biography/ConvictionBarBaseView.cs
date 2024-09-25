using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Biography;

public abstract class ConvictionBarBaseView : CharInfoComponentView<ConvictionBarVM>
{
	[SerializeField]
	protected RectTransform m_Container;

	[SerializeField]
	protected RectTransform m_Cursor;

	[Header("Buttons")]
	[SerializeField]
	protected OwlcatMultiButton m_RightButtonRadical;

	[SerializeField]
	protected OwlcatMultiButton m_LeftButtonPuritan;

	[SerializeField]
	protected OwlcatMultiButton m_CurrentCursor;

	[Header("Labels")]
	[SerializeField]
	protected TextMeshProUGUI m_RightLabel;

	[SerializeField]
	protected TextMeshProUGUI m_LeftLabel;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_RightLabel, m_LeftLabel);
		}
		base.BindViewImplementation();
		SetupLabels();
		float halfContainerLength = m_Container.sizeDelta.x / 2f;
		AddDisposable(base.ViewModel.CurrentRelativeValue.Subscribe(delegate(float value)
		{
			m_Cursor.anchoredPosition = new Vector2(halfContainerLength * value, m_Cursor.anchoredPosition.y);
		}));
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}

	private void SetupLabels()
	{
		m_RightLabel.text = UIStrings.Instance.Alignment.RadicalTitle;
		m_LeftLabel.text = UIStrings.Instance.Alignment.PuritanTitle;
	}
}
