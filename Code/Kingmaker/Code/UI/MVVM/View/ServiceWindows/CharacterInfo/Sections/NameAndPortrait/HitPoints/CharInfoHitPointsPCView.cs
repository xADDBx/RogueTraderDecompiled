using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait.HitPoints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait.HitPoints;

public class CharInfoHitPointsPCView : ViewBase<CharInfoHitPointsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MainHPField;

	[SerializeField]
	private TextMeshProUGUI m_SecondaryHPField;

	[SerializeField]
	private Image m_CurrentHPBar;

	[SerializeField]
	private Image m_TempHpBar;

	[SerializeField]
	private Image m_MaxHPBar;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.HpText.Subscribe(delegate(string value)
		{
			TextMeshProUGUI mainHPField = m_MainHPField;
			string text2 = (m_SecondaryHPField.text = value);
			mainHPField.text = text2;
		}));
		AddDisposable(base.ViewModel.CurrentHpRatio.Subscribe(delegate(float value)
		{
			m_CurrentHPBar.fillAmount = value;
		}));
		AddDisposable(base.ViewModel.TempHpRatio.Subscribe(delegate(float value)
		{
			m_TempHpBar.fillAmount = value;
		}));
		AddDisposable(base.ViewModel.MaxHpRatio.Subscribe(delegate(float value)
		{
			m_MaxHPBar.fillAmount = value;
		}));
		SetTooltips();
	}

	private void SetTooltips()
	{
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
