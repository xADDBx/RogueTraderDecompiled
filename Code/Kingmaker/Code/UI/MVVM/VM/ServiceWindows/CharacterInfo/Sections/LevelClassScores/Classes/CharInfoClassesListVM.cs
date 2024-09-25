using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;

public class CharInfoClassesListVM : CharInfoComponentVM
{
	private List<ClassData> m_ClassList = new List<ClassData>();

	private bool IsMythic;

	public AutoDisposingList<CharInfoClassEntryVM> ClassVMs { get; } = new AutoDisposingList<CharInfoClassEntryVM>();


	public CharInfoClassesListVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, bool isMythic)
		: base(unit)
	{
		IsMythic = isMythic;
		RefreshData();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		PartUnitProgression progression = Unit?.Value.Progression;
		ExtractProgression(progression);
		RefreshClassesList();
	}

	private void ExtractProgression(PartUnitProgression progression)
	{
		m_ClassList.Clear();
		foreach (ClassData item in progression.Classes.Where((ClassData c) => progression.GetClassLevel(c.CharacterClass) > 0 && (!c.CharacterClass.IsMythic || (c.CharacterClass.IsMythic && IsMythic))))
		{
			m_ClassList.Add(progression.GetClassData(item.CharacterClass));
		}
	}

	private void RefreshClassesList()
	{
		ClassVMs.Clear();
		if (!m_ClassList.Any())
		{
			return;
		}
		foreach (ClassData @class in m_ClassList)
		{
			CharInfoClassEntryVM charInfoClassEntryVM = new CharInfoClassEntryVM(@class);
			AddDisposable(charInfoClassEntryVM);
			ClassVMs.Add(charInfoClassEntryVM);
		}
	}
}
