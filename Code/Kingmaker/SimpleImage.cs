using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker;

[ExecuteInEditMode]
public class SimpleImage : Graphic
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		zero.x = 0f;
		zero.y = 0f;
		zero2.x = 1f;
		zero2.y = 1f;
		zero.x -= base.rectTransform.pivot.x;
		zero.y -= base.rectTransform.pivot.y;
		zero2.x -= base.rectTransform.pivot.x;
		zero2.y -= base.rectTransform.pivot.y;
		zero.x *= base.rectTransform.rect.width;
		zero.y *= base.rectTransform.rect.height;
		zero2.x *= base.rectTransform.rect.width;
		zero2.y *= base.rectTransform.rect.height;
		vh.Clear();
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.position = new Vector2(zero.x, zero.y);
		simpleVert.color = color;
		simpleVert.uv0 = Vector2.zero;
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector2(zero.x, zero2.y);
		simpleVert.color = color;
		simpleVert.uv0 = Vector2.up;
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector2(zero2.x, zero2.y);
		simpleVert.color = color;
		simpleVert.uv0 = Vector2.one;
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector2(zero2.x, zero.y);
		simpleVert.color = color;
		simpleVert.uv0 = Vector2.right;
		vh.AddVert(simpleVert);
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}
}
