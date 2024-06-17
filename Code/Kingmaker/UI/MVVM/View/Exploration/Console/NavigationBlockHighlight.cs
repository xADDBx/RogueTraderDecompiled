using System;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Console;

[Serializable]
internal struct NavigationBlockHighlight
{
	public NavigationBlock NavigationBlock;

	public RectTransform HighlightContainer;
}
