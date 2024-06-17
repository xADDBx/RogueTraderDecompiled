using TMPro;
using UnityEngine;

namespace Kingmaker.UI.TMPExtention.CurvedTextMeshPro;

[ExecuteInEditMode]
public abstract class TextProOnACurve : MonoBehaviour
{
	private TMP_Text m_TextComponent;

	private bool m_forceUpdate;

	private void Awake()
	{
		m_TextComponent = base.gameObject.GetComponent<TMP_Text>();
	}

	private void OnEnable()
	{
		m_forceUpdate = true;
	}

	private void OnValidate()
	{
		m_forceUpdate = true;
	}

	private void Reset()
	{
		m_forceUpdate = true;
	}

	public void ForceUpdate()
	{
		m_forceUpdate = true;
	}

	protected void Update()
	{
		if (!m_forceUpdate && !m_TextComponent.havePropertiesChanged)
		{
			return;
		}
		m_forceUpdate = false;
		m_TextComponent.ForceMeshUpdate();
		TMP_TextInfo textInfo = m_TextComponent.textInfo;
		int characterCount = textInfo.characterCount;
		if (characterCount == 0)
		{
			return;
		}
		float x = m_TextComponent.bounds.min.x;
		float x2 = m_TextComponent.bounds.max.x;
		for (int i = 0; i < characterCount; i++)
		{
			if (textInfo.characterInfo[i].isVisible)
			{
				int vertexIndex = textInfo.characterInfo[i].vertexIndex;
				int materialReferenceIndex = textInfo.characterInfo[i].materialReferenceIndex;
				Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
				Vector3 vector = new Vector2((vertices[vertexIndex].x + vertices[vertexIndex + 2].x) / 2f, textInfo.characterInfo[i].baseLine);
				vertices[vertexIndex] += -vector;
				vertices[vertexIndex + 1] += -vector;
				vertices[vertexIndex + 2] += -vector;
				vertices[vertexIndex + 3] += -vector;
				float zeroToOnePos = (vector.x - x) / (x2 - x);
				Matrix4x4 matrix4x = ComputeTransformationMatrix(vector, zeroToOnePos, textInfo, i);
				vertices[vertexIndex] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex]);
				vertices[vertexIndex + 1] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 1]);
				vertices[vertexIndex + 2] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 2]);
				vertices[vertexIndex + 3] = matrix4x.MultiplyPoint3x4(vertices[vertexIndex + 3]);
			}
		}
		m_TextComponent.UpdateVertexData();
	}

	protected abstract Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, float zeroToOnePos, TMP_TextInfo textInfo, int charIdx);
}
