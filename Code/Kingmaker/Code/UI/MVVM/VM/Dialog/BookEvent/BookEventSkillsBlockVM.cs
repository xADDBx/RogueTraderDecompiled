using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;

public class BookEventSkillsBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public List<CharInfoStatVM> Skills;

	public StatType StatType;

	public string SkillName => LocalizedTexts.Instance.Stats.GetText(StatType);

	public BookEventSkillsBlockVM(IEnumerable<BaseUnitEntity> units, StatType statType)
	{
		StatType = statType;
		Skills = new List<CharInfoStatVM>();
		foreach (BaseUnitEntity unit in units)
		{
			CharInfoStatVM charInfoStatVM = new CharInfoStatVM(unit.Stats.GetStat(statType), showPermanentValue: false);
			AddDisposable(charInfoStatVM);
			Skills.Add(charInfoStatVM);
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
