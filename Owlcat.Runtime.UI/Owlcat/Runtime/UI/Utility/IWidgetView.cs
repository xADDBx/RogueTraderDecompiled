using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Owlcat.Runtime.UI.Utility;

public interface IWidgetView
{
	MonoBehaviour MonoBehaviour { get; }

	void BindWidgetVM(IViewModel vm);

	bool CheckType(IViewModel viewModel);
}
