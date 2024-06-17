using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.Legacy.BugReportDrawing;

public class BugReportFromFallbackHandler : MonoBehaviour
{
	[FormerlySerializedAs("LastExceptionCallstackFull")]
	public Exception LastException;

	public void OnReport()
	{
		UnityEngine.Object.FindObjectOfType<BugReportCanvas>().Or(null)?.OnHotKeyBugReportOpen();
		EventBus.RaiseEvent(delegate(IBugReportDescriptionUIHandler h)
		{
			h.HandleException(LastException);
		});
	}
}
