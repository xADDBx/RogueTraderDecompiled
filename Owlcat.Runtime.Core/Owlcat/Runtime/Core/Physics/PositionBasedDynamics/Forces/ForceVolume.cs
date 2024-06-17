using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;

public class ForceVolume
{
	public ForceVolumeType VolumeType;

	public ForceEmissionType EmissionType;

	public float4x4 LocalToWorldVolume;

	public float4x4 LocalToWorldEmitter;

	public AxisDirection Axis;

	public DirectionType DirectionType;

	public float4 VolumeParameters;

	public float3 Direction;

	public float Intensity;

	public float DirectionLerp;

	public static int GetPackedEnumValues(ForceVolume forceVolume)
	{
		return (int)forceVolume.VolumeType | ((int)forceVolume.EmissionType << 2) | ((int)forceVolume.Axis << 4) | ((int)forceVolume.DirectionType << 6);
	}

	public static void UnpackEnumValues(int packedEnumValues, out ForceVolumeType volumeType, out ForceEmissionType emissionType, out AxisDirection axis, out DirectionType directionType)
	{
		volumeType = (ForceVolumeType)(packedEnumValues & 3);
		emissionType = (ForceEmissionType)((packedEnumValues >> 2) & 3);
		axis = (AxisDirection)((packedEnumValues >> 4) & 3);
		directionType = (DirectionType)((packedEnumValues >> 6) & 3);
	}
}
