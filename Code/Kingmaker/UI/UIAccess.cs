using Kingmaker.UI.Common;
using Kingmaker.UI.Selection;
using UnityEngine;

namespace Kingmaker.UI;

public static class UIAccess
{
	public static GameObject SoundGameObject;

	public static MultiplySelection MultiSelection
	{
		get
		{
			if (!UIUtility.IsGlobalMap())
			{
				return MultiplySelection.Instance;
			}
			return null;
		}
	}

	public static SelectionManagerBase SelectionManager
	{
		get
		{
			if (!UIUtility.IsGlobalMap())
			{
				return SelectionManagerBase.Instance;
			}
			return null;
		}
	}
}
