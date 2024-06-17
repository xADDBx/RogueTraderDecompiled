using System.Collections.Generic;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;

public class StringSequentialSelectorVM : SequentialSelectorVM<StringSequentialEntity>
{
	private readonly ReactiveProperty<string> m_Value = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SecondaryValue = new ReactiveProperty<string>();

	public IReadOnlyReactiveProperty<string> Value => m_Value;

	public IReadOnlyReactiveProperty<string> SecondaryValue => m_SecondaryValue;

	public StringSequentialSelectorVM(bool cyclical = true)
		: base(cyclical)
	{
	}

	public StringSequentialSelectorVM(List<StringSequentialEntity> valueList, StringSequentialEntity current = null, bool cyclical = true)
		: base(valueList, current, cyclical)
	{
	}

	protected override void SetCurrentEntity()
	{
		m_Value.Value = ValueList[base.CurrentIndex.Value].Title;
		m_SecondaryValue.Value = ValueList[base.CurrentIndex.Value].Description;
	}
}
