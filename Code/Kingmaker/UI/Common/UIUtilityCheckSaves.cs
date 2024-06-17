using System;
using System.Collections;

namespace Kingmaker.UI.Common;

public class UIUtilityCheckSaves
{
	public static IEnumerator WaitForSaveUpdated(Action finishAction)
	{
		while (!Game.Instance.SaveManager.AreSavesUpToDate)
		{
			PFLog.UI.Log("[WaitForSaveUpdated] waiting for saves...");
			yield return null;
		}
		finishAction();
	}
}
