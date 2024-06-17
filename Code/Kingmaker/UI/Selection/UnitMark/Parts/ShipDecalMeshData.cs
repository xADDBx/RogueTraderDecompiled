using System;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark.Parts;

[Serializable]
public class ShipDecalMeshData
{
	public bool IsDirect;

	public MeshRenderer Renderer;

	public Material NormalMaterial;

	public Material SelectedMaterial;

	public void SetSelected(bool isSelected)
	{
		Renderer.material = (isSelected ? SelectedMaterial : NormalMaterial);
	}
}
