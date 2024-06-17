using System;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.InputSystems;

public class KeyboardAccessRunner : MonoBehaviour
{
	private static KeyboardAccessRunner s_ActiveRunner;

	private void OnEnable()
	{
		if (s_ActiveRunner == null)
		{
			s_ActiveRunner = this;
			return;
		}
		PFLog.Default.Error("Previous KeyboardAccessRunner was not disabled properly! Current: '" + base.transform.GetHierarchyPath("/") + "' Previous: '" + s_ActiveRunner.transform.GetHierarchyPath("/") + "'");
	}

	private void OnDisable()
	{
		if (s_ActiveRunner == this)
		{
			s_ActiveRunner = null;
			return;
		}
		PFLog.Default.Error("Another KeyboardAccessRunner was activated before disabling of current! Current: '" + base.transform.GetHierarchyPath("/") + "' Previous: '" + s_ActiveRunner.transform.GetHierarchyPath("/") + "'");
	}

	private void Update()
	{
		try
		{
			KeyboardAccess.Instance.Tick();
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex);
		}
	}
}
