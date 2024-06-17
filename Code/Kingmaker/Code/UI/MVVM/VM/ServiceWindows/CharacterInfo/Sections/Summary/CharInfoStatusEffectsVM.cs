using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Summary;

public class CharInfoStatusEffectsVM : CharInfoComponentVM
{
	public CharInfoFeatureGroupVM BuffsGroup;

	public bool NoBuffs => BuffsGroup.FeatureList.Count == 0;

	public CharInfoStatusEffectsVM(IReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		List<CharInfoFeatureVM> buffs = new List<CharInfoFeatureVM>();
		buffs = ExtractBuffs(Unit.Value, buffs);
		AddDisposable(BuffsGroup = new CharInfoFeatureGroupVM(buffs));
	}

	private List<CharInfoFeatureVM> ExtractBuffs(BaseUnitEntity unit, List<CharInfoFeatureVM> buffs)
	{
		foreach (Buff unitBuff in GetUnitBuffs(unit))
		{
			buffs.Add(new CharInfoFeatureVM(unitBuff, unit));
		}
		return buffs;
	}

	private List<Buff> GetUnitBuffs(BaseUnitEntity unit)
	{
		return unit.Buffs.Enumerable.Where((Buff b) => !b.Blueprint.IsHiddenInUI).ToList();
	}
}
