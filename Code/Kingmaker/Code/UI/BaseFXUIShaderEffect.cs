using System.Collections.Generic;
using Kingmaker.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI;

[RequireComponent(typeof(Graphic))]
public abstract class BaseFXUIShaderEffect : BaseMeshEffect
{
	[SerializeField]
	private bool _randomizeOnEnableOnly = true;

	private float _randomSeedValue;

	private bool _needToSetRandomSeed;

	private static UIVertex vert = default(UIVertex);

	private static readonly List<UIVertex> vertexStream = new List<UIVertex>();

	protected override void OnEnable()
	{
		if (_randomizeOnEnableOnly)
		{
			RandomizeSeed();
		}
		if ((bool)base.graphic.canvas)
		{
			base.graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
		}
	}

	private void RandomizeSeed()
	{
		_randomSeedValue = Random.value * 100f;
		_needToSetRandomSeed = true;
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive())
		{
			return;
		}
		if (!_randomizeOnEnableOnly)
		{
			RandomizeSeed();
		}
		vh.GetUIVertexStream(vertexStream);
		UIUtility.FindMinMaxPositions(in vertexStream, out var min, out var _, out var width, out var height, useSorting: true);
		for (int i = 0; i < vh.currentVertCount; i++)
		{
			vh.PopulateUIVertex(ref vert, i);
			if (_needToSetRandomSeed)
			{
				vert.uv1.z = _randomSeedValue;
			}
			vert.uv0.z = (vert.position.x - min.x) / width;
			vert.uv0.w = (vert.position.y - min.y) / height;
			vh.SetUIVertex(vert, i);
		}
		_needToSetRandomSeed = false;
	}
}
