using System;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class ReasonBuffItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public Sprite Icon;

	public string Name;

	public ReasonBuffItemVM(NullifyInformation.BuffInformation buff)
	{
		Icon = buff.Icon;
		Name = buff.Name;
	}

	protected override void DisposeImplementation()
	{
	}
}
