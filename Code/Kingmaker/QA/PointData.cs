using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.QA;

[JsonObject]
public class PointData
{
	[JsonProperty]
	public string Name;

	[JsonProperty]
	public string Area;

	[JsonProperty]
	public string Point;

	[JsonProperty(IsReference = false)]
	public Vector3 Position;

	public PointData()
	{
	}

	public PointData(MegatronPoint Point)
	{
		Name = Point.Area.AreaName;
		Area = Point.Area.AssetGuid;
		this.Point = Point.UID;
		Position = Point.transform.position;
	}
}
