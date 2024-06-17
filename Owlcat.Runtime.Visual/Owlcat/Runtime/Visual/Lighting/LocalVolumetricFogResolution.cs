using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Lighting;

[Serializable]
public enum LocalVolumetricFogResolution
{
	[InspectorName("32x32x32")]
	Resolution32 = 0x20,
	[InspectorName("64x64x64")]
	Resolution64 = 0x40,
	[InspectorName("128x128x128")]
	Resolution128 = 0x80,
	[InspectorName("256x256x256")]
	Resolution256 = 0x100
}
