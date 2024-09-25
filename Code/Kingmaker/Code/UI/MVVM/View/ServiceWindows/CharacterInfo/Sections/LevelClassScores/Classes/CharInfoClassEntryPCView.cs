using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;

public class CharInfoClassEntryPCView : ViewBase<CharInfoClassEntryVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_ClassName;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		Clear();
		base.gameObject.SetActive(value: true);
		m_Level.text = base.ViewModel.Level.ToString();
		m_ClassName.text = base.ViewModel.ClassName;
		AddDisposable(m_ClassName.SetTooltip(base.ViewModel.Tooltip));
	}

	public void Hide()
	{
		Clear();
	}

	private void Clear()
	{
		m_Level.text = string.Empty;
		m_ClassName.text = string.Empty;
		base.gameObject.SetActive(value: false);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoClassEntryVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoClassEntryVM;
	}
}
