using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

[KnowledgeDatabaseID("05d2d66406b0cbe4fb2b26db672f8898")]
public class ScriptZonePolygon : PolygonComponent, IScriptZoneShape
{
	[Serializable]
	public class PolygonMeshEditorSettings
	{
		public LayerMask RaycastLayers = 2097409;

		public float MeshHeight = 0.05f;

		public float Resolution = 1f;

		public float RaycastHeightOffset = 1f;

		public float OutlineResolution = 1f;

		public float OutlineWidth = 0.05f;

		public float OutlineHeight = 0.05f;
	}

	[SerializeField]
	[HideInInspector]
	private PolygonMeshEditorSettings _editorSettings;

	public MeshFilter DecalMeshObject;

	public LineRenderer OutlineLineRenderer;

	public PolygonMeshEditorSettings EditorSettings => _editorSettings;

	public NodeList CoveredNodes => NodeList.Empty;

	public override bool NeedsMeshCreation => true;

	public bool Contains(Vector3 point, IntRect size, Vector3 forward = default(Vector3))
	{
		using (ProfileScope.New("ScriptZonePolygon.Contains"))
		{
			if (size.Width <= 1 && size.Height <= 1)
			{
				return ContainsPointXZ(point);
			}
			foreach (CustomGridNodeBase occupiedNode in GridAreaHelper.GetOccupiedNodes(point.GetNearestNodeXZUnwalkable(), size))
			{
				if (ContainsPointXZ(occupiedNode.Vector3Position))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool Contains(CustomGridNodeBase node, IntRect size, Vector3 forward = default(Vector3))
	{
		return Contains(node.Vector3Position, size);
	}

	public virtual Bounds GetBounds()
	{
		return base.TransformedBound;
	}

	public Vector3 Center()
	{
		return base.TransformedPoints.Aggregate((Vector3 a, Vector3 b) => a + b) / base.TransformedPoints.Length;
	}
}
