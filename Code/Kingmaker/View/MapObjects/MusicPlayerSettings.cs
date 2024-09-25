using System;
using Kingmaker.Visual.Sound;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class MusicPlayerSettings
{
	public bool AutoPlay;

	[AkEventReference]
	public string Start;

	[AkEventReference]
	public string[] StartEvents;

	[AkEventReference]
	public string Stop;

	[AkEventReference]
	public string[] StopEvents;
}
