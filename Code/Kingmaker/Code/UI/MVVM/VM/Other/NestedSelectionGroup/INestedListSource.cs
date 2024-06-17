using System.Collections.Generic;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Other.NestedSelectionGroup;

public interface INestedListSource
{
	INestedListSource Source { get; }

	bool HasNesting { get; }

	List<NestedSelectionGroupEntityVM> ExtractNestedEntities();

	ReactiveProperty<NestedSelectionGroupEntityVM> GetSelectedReactiveProperty();
}
