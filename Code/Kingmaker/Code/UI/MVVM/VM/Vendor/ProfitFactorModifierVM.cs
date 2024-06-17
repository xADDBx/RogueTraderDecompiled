using System;
using Kingmaker.Code.Globalmap.Colonization;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class ProfitFactorModifierVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ProfitFactorModifierType Type;

	public readonly bool IsNegative;

	public readonly ReactiveProperty<float> ModifierValue = new ReactiveProperty<float>(0f);

	private readonly ProfitFactorModifier m_Modifier;

	public ProfitFactorModifier Modifier => m_Modifier;

	public ProfitFactorModifierVM(ProfitFactorModifier modifier)
	{
		m_Modifier = modifier;
		Type = modifier.ModifierType;
		IsNegative = modifier.IsNegative;
		ModifierValue.Value = modifier.Value;
	}

	protected override void DisposeImplementation()
	{
	}
}
