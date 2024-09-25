using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

public class BookEventSkillsBlockPCView : ViewBase<BookEventSkillsBlockVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_SkillName;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private BookEventSkillPCView m_BookEventSkillViewPrefab;

	private bool m_IsInit;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_SkillName.text = base.ViewModel.SkillName;
		DrawEntities();
		HighlightMax();
	}

	private void HighlightMax()
	{
		int maxSkillValue = base.ViewModel.Skills.Max((CharInfoStatVM skill) => skill.StatValue.Value);
		CharInfoStatVM item = base.ViewModel.Skills.FirstOrDefault((CharInfoStatVM skill) => skill.StatValue.Value == maxSkillValue);
		int index = base.ViewModel.Skills.IndexOf(item);
		((BookEventSkillPCView)m_WidgetList.Entries[index]).Highlight();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Skills.ToArray(), m_BookEventSkillViewPrefab);
	}

	public void SelectSkill(int? index)
	{
		for (int i = 0; i < m_WidgetList.Entries.Count; i++)
		{
			((BookEventSkillPCView)m_WidgetList.Entries[i]).SetSelected(i == index);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as BookEventSkillsBlockVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is BookEventSkillsBlockVM;
	}
}
