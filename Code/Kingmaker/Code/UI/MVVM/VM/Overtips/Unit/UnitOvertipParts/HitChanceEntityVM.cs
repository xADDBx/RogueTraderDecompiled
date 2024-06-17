using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;

public class HitChanceEntityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly float Chance;

	public readonly int Index;

	public readonly bool IsLast;

	public HitChanceEntityVM(int index, float chance, bool isLast)
	{
		Index = index;
		Chance = chance;
		IsLast = isLast;
	}

	protected override void DisposeImplementation()
	{
	}
}
