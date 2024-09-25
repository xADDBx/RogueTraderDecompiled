using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.Utility;

public static class GlossaryPointsUtils
{
	public static List<GlossaryPoint> GetLinksCoordinatesDictionary(TextMeshProUGUI text)
	{
		text.ForceMeshUpdate();
		List<GlossaryPoint> list = new List<GlossaryPoint>();
		if (text.textInfo == null)
		{
			return list;
		}
		for (int i = 0; i < text.textInfo.linkCount; i++)
		{
			TMP_LinkInfo tMP_LinkInfo = text.textInfo.linkInfo[i];
			int linkTextfirstCharacterIndex = tMP_LinkInfo.linkTextfirstCharacterIndex;
			int num = tMP_LinkInfo.linkTextfirstCharacterIndex + tMP_LinkInfo.linkTextLength;
			TMP_CharacterInfo firstLetter = text.textInfo.characterInfo[linkTextfirstCharacterIndex];
			TMP_CharacterInfo lastLetter = text.textInfo.characterInfo[num];
			float height;
			float width;
			Vector2 middlePoint;
			if (firstLetter.lineNumber == lastLetter.lineNumber)
			{
				CalculateGlossaryPointCoordinates(firstLetter, lastLetter, out height, out width, out middlePoint);
				list.Add(new GlossaryPoint(middlePoint, new Vector2(width + 10f, height + 14f), Vector3.zero, Vector2.zero, tMP_LinkInfo.GetLinkID()));
				continue;
			}
			int num2 = 1;
			for (int j = linkTextfirstCharacterIndex; j < num; j++)
			{
				if (text.textInfo.characterInfo[j].lineNumber != firstLetter.lineNumber)
				{
					num2 = j;
					break;
				}
			}
			TMP_CharacterInfo lastLetter2 = text.textInfo.characterInfo[num2 - 1];
			TMP_CharacterInfo firstLetter2 = text.textInfo.characterInfo[num2];
			CalculateGlossaryPointCoordinates(firstLetter, lastLetter2, out height, out width, out middlePoint);
			CalculateGlossaryPointCoordinates(firstLetter2, lastLetter, out var height2, out var width2, out var middlePoint2);
			list.Add(new GlossaryPoint(middlePoint, new Vector2(width + 10f, height + 14f), middlePoint2, new Vector2(width2 + 10f, height2 + 14f), tMP_LinkInfo.GetLinkID()));
		}
		return list;
	}

	private static void CalculateGlossaryPointCoordinates(TMP_CharacterInfo firstLetter, TMP_CharacterInfo lastLetter, out float height, out float width, out Vector2 middlePoint)
	{
		height = Mathf.Abs(firstLetter.topRight.y - firstLetter.bottomRight.y);
		width = lastLetter.topLeft.x - firstLetter.topLeft.x;
		middlePoint = new Vector2(firstLetter.topLeft.x + width / 2f, firstLetter.topRight.y - height / 2f);
	}
}
