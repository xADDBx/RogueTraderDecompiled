using System;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.TMPExtention.CurvedTextMeshPro;

[ExecuteInEditMode]
public class TextProOnACircle : TextProOnACurve
{
	[SerializeField]
	[Tooltip("The radius of the text circle arc")]
	private float m_radius = 10f;

	[SerializeField]
	[Tooltip("How much degrees the text arc should span")]
	private float m_arcDegrees = 90f;

	[SerializeField]
	[Tooltip("The angular offset at which the arc should be centered, in degrees")]
	private float m_angularOffset = -90f;

	[SerializeField]
	[Tooltip("The maximum angular distance between letters, in degrees")]
	private int m_maxDegreesPerLetter = 360;

	[SerializeField]
	[Tooltip("Vertically mirror characters")]
	private bool m_mirrorCharacters;

	[SerializeField]
	[Tooltip("Calculate arc size by text width")]
	private bool m_shrinkEmptySpace;

	[Tooltip("Extra space between characters")]
	[SerializeField]
	private float m_extraSpacing;

	protected override Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, float zeroToOnePos, TMP_TextInfo textInfo, int charIdx)
	{
		int lineNumber = textInfo.characterInfo[charIdx].lineNumber;
		float lineHeight = textInfo.lineInfo[0].lineHeight;
		float num = lineHeight * (float)lineNumber;
		num = ((!m_mirrorCharacters) ? (m_radius - num) : (m_radius + num + lineHeight));
		float num2 = 0f;
		num2 = ((!m_shrinkEmptySpace) ? Mathf.Min(m_arcDegrees, textInfo.characterCount / textInfo.lineCount * m_maxDegreesPerLetter) : ((textInfo.lineInfo[lineNumber].length + m_extraSpacing) / num * 57.29578f));
		if (m_mirrorCharacters)
		{
			num2 *= -1f;
		}
		float f = (charMidBaselinePos.x / textInfo.lineInfo[lineNumber].length * num2 + m_angularOffset) * (MathF.PI / 180f);
		float num3 = Mathf.Cos(f);
		float num4 = Mathf.Sin(f);
		Vector2 vector = new Vector2(num3 * num, (0f - num4) * num);
		float num5 = (0f - Mathf.Atan2(num4, num3)) * 57.29578f - 90f;
		if (m_mirrorCharacters)
		{
			num5 -= 180f;
		}
		return Matrix4x4.TRS(new Vector3(vector.x, vector.y, 0f), Quaternion.AngleAxis(num5, Vector3.forward), Vector3.one);
	}
}
