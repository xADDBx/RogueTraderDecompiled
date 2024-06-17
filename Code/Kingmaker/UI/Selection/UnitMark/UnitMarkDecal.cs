using System;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

[Serializable]
public class UnitMarkDecal
{
	public GameObject GameObject;

	public MeshRenderer DecalMeshRenderer;

	public Material MaterialSizeStandard;

	public Material MaterailSizeBig;

	public void SetActive(bool state)
	{
		GameObject.SetActive(state);
	}

	public void SetBigSize(bool isBig)
	{
		DecalMeshRenderer.material = (isBig ? MaterailSizeBig : MaterialSizeStandard);
	}
}
