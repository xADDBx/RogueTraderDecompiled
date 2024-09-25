using System;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Area;

[Serializable]
public class CampingSettings
{
	public bool CampingAllowed = true;

	[InfoBox("Used only for camp visual and barks: if = 0 -> Treated as Dungeon")]
	public int HuntingDC = 15;

	public int DivineServiceDCBonus = 15;

	[FormerlySerializedAs("BuildingDC")]
	public int CamouflageDC = 10;

	public bool IsDungeon => HuntingDC <= 0;
}
