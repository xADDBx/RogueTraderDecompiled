using System.Collections.Generic;
using Owlcat.Runtime.Visual.Lighting;
using UnityEngine;

namespace Owlcat.Runtime.Visual.IndirectRendering;

public interface IIndirectMesh
{
	bool IsDynamic { get; }

	int MaxDynamicInstances { get; }

	Vector3 Position { get; }

	Mesh Mesh { get; set; }

	List<Material> Materials { get; set; }

	LightLayerEnum RenderingLayerMask { get; set; }

	Vector2 ScaleRange { get; set; }

	Vector2 RotationRange { get; set; }

	void UpdateInstances();
}
