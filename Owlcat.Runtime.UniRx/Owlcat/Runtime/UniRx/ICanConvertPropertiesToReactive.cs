using UniRx;

namespace Owlcat.Runtime.UniRx;

public interface ICanConvertPropertiesToReactive
{
	ReactiveCommand UpdateCommand { get; }
}
