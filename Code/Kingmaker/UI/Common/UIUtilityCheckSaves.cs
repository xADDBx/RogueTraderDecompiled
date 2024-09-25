using System;
using System.Collections;
using Kingmaker.Stores;
using UnityEngine;

namespace Kingmaker.UI.Common;

public class UIUtilityCheckSaves
{
	public static IEnumerator WaitForSaveUpdated(Action finishAction, bool waitForDlc = false)
	{
		while (!Game.Instance.SaveManager.AreSavesUpToDate)
		{
			PFLog.UI.Log("[WaitForSaveUpdated] waiting for saves...");
			yield return null;
		}
		if (waitForDlc)
		{
			yield return WaitForDlcStatusesReceived();
		}
		finishAction();
	}

	public static IEnumerator WaitForDlcStatusesReceived()
	{
		float startTime = Time.realtimeSinceStartup;
		while (StoreManager.IsEgsDlcStatusAwaiting)
		{
			PFLog.UI.Log("[WaitForSaveUpdated] waiting for dlc...");
			if (Time.realtimeSinceStartup - startTime > 5f)
			{
				PFLog.UI.Log("[WaitForSaveUpdated] timed out while waiting for dlc.");
				break;
			}
			yield return null;
		}
	}
}
