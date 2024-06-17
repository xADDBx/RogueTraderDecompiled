using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.BugReport;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Kingmaker.UI.MVVM.View.BugReport;

public class BugDuplicateItemView : ViewBase<BugDuplicateItemVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	protected OwlcatMultiButton m_FocusButton;

	[SerializeField]
	protected TextMeshProUGUI m_TitleText;

	[SerializeField]
	protected TextMeshProUGUI m_StatusText;

	[SerializeField]
	protected TextMeshProUGUI m_BuildStatusText;

	[SerializeField]
	protected TextMeshProUGUI m_DistanceText;

	[SerializeField]
	protected TextMeshProUGUI m_AssigneeText;

	[SerializeField]
	protected Image m_SpacingImage;

	[SerializeField]
	protected Image m_TypeIcon;

	[SerializeField]
	protected Image m_PriorityIcon;

	[Header("Colors")]
	[SerializeField]
	private Color m_SpacingTaskColor;

	[SerializeField]
	private Color m_SpacingBugTaskColor;

	[SerializeField]
	private Color m_NumberTaskColor;

	[Header("Sprites")]
	[SerializeField]
	private Sprite m_TaskIcon;

	[SerializeField]
	private Sprite m_BugIcon;

	[SerializeField]
	private List<Sprite> m_PriorityIcons;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Button.OnLeftClickAsObservable().Subscribe(OpenUrl);
		m_Button.OnConfirmClickAsObservable().Subscribe(OpenUrl);
		m_SpacingImage.color = (base.ViewModel.IsTask ? m_SpacingTaskColor : m_SpacingBugTaskColor);
		string text = HexRGB.ColorToHex(m_NumberTaskColor);
		string text2 = (base.ViewModel.IsFixed ? ("<s>" + base.ViewModel.NumberTask + "</s>") : base.ViewModel.NumberTask);
		m_TitleText.text = "<color=" + text + ">" + text2 + "</color> " + base.ViewModel.Title;
		m_StatusText.text = base.ViewModel.Status;
		m_BuildStatusText.text = base.ViewModel.BuildStatus;
		TextMeshProUGUI distanceText = m_DistanceText;
		int distance = base.ViewModel.Distance;
		distanceText.text = distance.ToString();
		m_AssigneeText.text = base.ViewModel.Creator ?? "";
		m_TypeIcon.sprite = (base.ViewModel.IsTask ? m_TaskIcon : m_BugIcon);
		m_PriorityIcon.sprite = m_PriorityIcons[base.ViewModel.PriorityType - 1];
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void OpenUrl()
	{
		Application.OpenURL(base.ViewModel.Url);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as BugDuplicateItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is BugDuplicateItemVM;
	}

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}
}
