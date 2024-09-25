using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

public class DismembermentSetBehaviour : MonoBehaviour
{
	public Material SliceMaterial;

	public List<Mesh> Meshes = new List<Mesh>();

	public List<DismembermentPieceDescriptor> Pieces = new List<DismembermentPieceDescriptor>();
}
