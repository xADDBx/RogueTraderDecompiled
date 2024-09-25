using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Alignments;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;

public class CharInfoAlignmentHistoryVM : CharInfoComponentVM
{
	public List<CharInfoSoulMarkShiftRecordVM> SoulMarkShiftsHistory;

	public CharInfoAlignmentHistoryVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		SoulMarkShiftsHistory = (from s in SoulMarkShiftExtension.AppliedShifts()
			select new CharInfoSoulMarkShiftRecordVM(s)).ToList();
	}
}
