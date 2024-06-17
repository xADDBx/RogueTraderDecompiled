using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat;

internal struct ShieldProps
{
	public ReactiveProperty<int> Current;

	public ReactiveProperty<float> Ratio;
}
