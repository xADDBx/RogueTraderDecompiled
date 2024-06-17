using Newtonsoft.Json;

namespace Kingmaker.QA;

[JsonObject]
public class ScreenshotData
{
	[JsonProperty]
	public PointData Point;

	[JsonProperty]
	public long Time;

	[JsonProperty]
	public int[] FPS;
}
