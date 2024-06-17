using System;

namespace Owlcat.Runtime.Visual.Lighting;

[Flags]
public enum LightLayerEnum
{
	Nothing = 0,
	LightLayerDefault = 1,
	LightLayer1 = 2,
	LightLayer2 = 4,
	LightLayer3 = 8,
	LightLayer4 = 0x10,
	LightLayer5 = 0x20,
	LightLayer6 = 0x40,
	LightLayer7 = 0x80,
	Everything = 0xFF
}
