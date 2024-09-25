namespace Owlcat.Runtime.Core.Utility;

public enum Layers
{
	Default = 0,
	DefaultMask = 1,
	Water = 4,
	WaterMask = 16,
	Ground = 8,
	GroundMask = 256,
	Unit = 9,
	UnitMask = 512,
	Selectable = 10,
	SelectableMask = 1024,
	NavMeshIgnore = 12,
	NavMeshIgnoreMask = 4096,
	Sound = 13,
	SoundMask = 8192,
	NavMeshObstacle = 14,
	NavMeshObstacleMask = 16384,
	DollRoom = 15,
	DollRoomMask = 32768,
	ClothCollider = 17,
	ClothColliderMask = 131072,
	GroundDynamic = 18,
	GroundDynamicMask = 262144,
	NavMeshObstacleDynamic = 19,
	NavMeshObstacleDynamicMask = 524288,
	GroundNoCamera = 21,
	GroundNoCamerasMask = 2097152,
	CameraOnly = 22,
	CameraOnlyMask = 4194304,
	SphereBoundsUnit = 23,
	SphereBoundsUnitMask = 8388608,
	SphereBoundsObject = 24,
	SphereBoundsObjectMask = 16777216,
	SphereBoundsMask = 25165824,
	ThinCoverObstacle = 26,
	ThinCoverObstacleMask = 67108864,
	WeaponCollider = 29,
	WeaponColliderMask = 536870912,
	VertexLighted = 30,
	VertexLightedMask = 1073741824,
	EditorGizmos = 31,
	EditorGizmosMask = int.MinValue,
	PointerMask = 70014209,
	WalkableMask = 2359553,
	SoundSurfaceMask = 2359569
}
