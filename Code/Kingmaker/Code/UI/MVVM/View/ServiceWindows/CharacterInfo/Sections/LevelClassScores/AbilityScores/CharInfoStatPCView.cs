using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;

public class CharInfoStatPCView : ViewBase<CharInfoStatVM>
{
	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_LongName;

	[SerializeField]
	private TextMeshProUGUI m_ShortName;

	[Header("Values")]
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private bool m_AlwaysShowSign;

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		SetLabels();
		AddDisposable(base.ViewModel.IsValueEnabled.Subscribe(delegate
		{
			SetValue();
		}));
		AddDisposable(base.ViewModel.StatValue.Subscribe(delegate
		{
			SetValue();
		}));
		AddDisposable(base.ViewModel.StringValue.Subscribe(delegate
		{
			SetValue();
		}));
		SetTooltip();
	}

	private void SetLabels()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		if (m_LongName != null)
		{
			m_LongName.text = base.ViewModel.Name.Value;
			m_LongName.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		}
		if (m_ShortName != null)
		{
			m_ShortName.text = base.ViewModel.ShortName;
			m_ShortName.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		}
	}

	private void SetValue()
	{
		if (!base.ViewModel.IsValueEnabled.Value)
		{
			m_Value.text = "-";
		}
		else if (string.IsNullOrEmpty(base.ViewModel.StringValue.Value))
		{
			m_Value.text = (m_AlwaysShowSign ? UIUtility.AddSign(base.ViewModel.StatValue.Value) : base.ViewModel.StatValue.ToString());
		}
		else
		{
			m_Value.text = base.ViewModel.StringValue.Value;
		}
	}

	private void SetTooltip()
	{
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
