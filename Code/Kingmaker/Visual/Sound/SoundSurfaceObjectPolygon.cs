using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Visual.Sound;

[ClassInfoBox("Used to specify area on SoundSurfaceObject with different surface type\nShould be located on switch object")]
public class SoundSurfaceObjectPolygon : PolygonComponent
{
	public SurfaceType Switch;
}
