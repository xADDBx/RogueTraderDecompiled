using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Inspect;

public class InspectReactiveData
{
	public readonly ReactiveProperty<string> WoundsValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> WoundsAddValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> DeflectionValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> ArmorValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> DodgeValue = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> MovementPointsValue = new ReactiveProperty<string>();

	public readonly ReactiveCollection<ITooltipBrick> TooltipBrickBuffs = new ReactiveCollection<ITooltipBrick>();
}
