using System;
using Kingmaker.Code.UI.MVVM.VM.PointMarkers;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.PointMarkers;

[Serializable]
public class PointMarkerRelationParams
{
	public bool IsUnit = true;

	[ShowIf("IsUnit")]
	public UnitRelation Relation;

	public bool IsAnotherEntity;

	[ShowIf("IsAnotherEntity")]
	public EntityPointMarkObjectType EntityPointMarkObjectType;

	public Sprite Icon;

	[Header("Colors")]
	public Color32 IconColor;

	public Color32 FrameColor;

	[Header("Scale")]
	public float Scale = 1f;
}
