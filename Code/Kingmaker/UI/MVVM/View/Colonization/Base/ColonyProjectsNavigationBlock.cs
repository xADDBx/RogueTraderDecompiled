using System;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

[Serializable]
public struct ColonyProjectsNavigationBlock
{
	public GameObject Container;

	public WidgetListMVVM WidgetList;

	public ColonyProjectRank Rank;
}
