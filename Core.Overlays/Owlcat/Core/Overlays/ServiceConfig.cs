using System;
using System.Collections.Generic;

namespace Owlcat.Core.Overlays;

[Serializable]
internal class ServiceConfig
{
	[Serializable]
	public class OverlayEntry
	{
		public string Name;

		public List<string> HiddenGraphs = new List<string>();
	}

	public List<OverlayEntry> OverlayData = new List<OverlayEntry>();

	public string LastSelectedName;

	public bool Darken = true;
}
