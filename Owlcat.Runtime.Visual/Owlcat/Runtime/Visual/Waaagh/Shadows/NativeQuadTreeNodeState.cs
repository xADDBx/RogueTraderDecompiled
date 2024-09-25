namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

internal enum NativeQuadTreeNodeState : byte
{
	Free,
	PartiallyOccupied,
	Occupied,
	OccupiedInHierarchy
}
