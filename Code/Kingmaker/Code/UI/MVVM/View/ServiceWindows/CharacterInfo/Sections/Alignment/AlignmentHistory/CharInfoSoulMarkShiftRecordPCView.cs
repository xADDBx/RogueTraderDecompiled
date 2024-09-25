using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;

public class CharInfoSoulMarkShiftRecordPCView : ViewBase<CharInfoSoulMarkShiftRecordVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	[Header("Colors")]
	[SerializeField]
	private Color m_FaithColor;

	[SerializeField]
	private Color m_CorruptionColor;

	[SerializeField]
	private Color m_HopeColor;

	private AccessibilityTextHelper m_TextHelper;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Description);
		}
		m_Description.text = base.ViewModel.Description?.Text + " " + GetDirectionInfo();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoSoulMarkShiftRecordVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoSoulMarkShiftRecordVM;
	}

	private string GetDirectionInfo()
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(GetDirectionColor()) + ">" + $"{UIUtility.GetSoulMarkDirectionText(base.ViewModel.Direction).Text} +{base.ViewModel.Amount}</color>";
	}

	private Color GetDirectionColor()
	{
		return base.ViewModel.Direction switch
		{
			SoulMarkDirection.Faith => m_FaithColor, 
			SoulMarkDirection.Corruption => m_CorruptionColor, 
			SoulMarkDirection.Hope => m_HopeColor, 
			_ => Color.magenta, 
		};
	}
}
