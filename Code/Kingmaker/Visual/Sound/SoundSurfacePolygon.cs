using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Sound;

[ClassInfoBox("Used to override surface type of specified area\nShould be primary child of SoundSurface object on scene\nIf point is located in more than one polygons surface will be taken from the first child of SoundSurface (upper in hierarchy)")]
public class SoundSurfacePolygon : PolygonComponent
{
	[HideInInspector]
	[FormerlySerializedAs("SoundSwitch")]
	public string SwitchString;

	public SurfaceType Switch;
}
