using System;
using Kingmaker.Code.UI.MVVM.VM.PointMarkers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.PointMarkers;

[Serializable]
public class PointMarkerRelationParams
{
	public UnitRelation Relation;

	public Sprite Icon;

	[Header("Colors")]
	public Color32 IconColor;

	public Color32 FrameColor;

	[Header("Scale")]
	public float Scale = 1f;
}
