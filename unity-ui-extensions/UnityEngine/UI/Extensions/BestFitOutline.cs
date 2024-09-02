using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Effects/Extensions/BestFit Outline")]
public class BestFitOutline : Shadow
{
	protected BestFitOutline()
	{
	}

	public override void ModifyMesh(Mesh mesh)
	{
		if (!IsActive())
		{
			return;
		}
		List<UIVertex> list = new List<UIVertex>();
		using (VertexHelper vertexHelper = new VertexHelper(mesh))
		{
			vertexHelper.GetUIVertexStream(list);
		}
		Text component = GetComponent<Text>();
		float num = 1f;
		if ((bool)component && component.resizeTextForBestFit)
		{
			num = (float)component.cachedTextGenerator.fontSizeUsedForBestFit / (float)(component.resizeTextMaxSize - 1);
		}
		int start = 0;
		int count = list.Count;
		ApplyShadowZeroAlloc(list, base.effectColor, start, list.Count, base.effectDistance.x * num, base.effectDistance.y * num);
		start = count;
		int count2 = list.Count;
		ApplyShadowZeroAlloc(list, base.effectColor, start, list.Count, base.effectDistance.x * num, (0f - base.effectDistance.y) * num);
		start = count2;
		int count3 = list.Count;
		ApplyShadowZeroAlloc(list, base.effectColor, start, list.Count, (0f - base.effectDistance.x) * num, base.effectDistance.y * num);
		start = count3;
		_ = list.Count;
		ApplyShadowZeroAlloc(list, base.effectColor, start, list.Count, (0f - base.effectDistance.x) * num, (0f - base.effectDistance.y) * num);
		using VertexHelper vertexHelper2 = new VertexHelper();
		vertexHelper2.AddUIVertexTriangleStream(list);
		vertexHelper2.FillMesh(mesh);
	}
}
