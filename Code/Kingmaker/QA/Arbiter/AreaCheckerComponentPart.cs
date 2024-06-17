using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.QA.Arbiter;

[TypeId("05c1a401230a49719bc119a38d101cfd")]
public class AreaCheckerComponentPart : ArbiterElement
{
	public BlueprintAreaEnterPointReference EnterPoint;

	public ArbiterElementList PointList = new ArbiterElementList(typeof(ArbiterPoint));

	public string StaticSceneName
	{
		get
		{
			BlueprintAreaEnterPoint blueprintAreaEnterPoint = SimpleBlueprintExtendAsObject.Or(EnterPoint.Get(), null);
			object obj = ((blueprintAreaEnterPoint == null) ? null : SimpleBlueprintExtendAsObject.Or(blueprintAreaEnterPoint.AreaPart, null)?.StaticScene?.SceneName);
			if (obj == null)
			{
				BlueprintAreaEnterPoint blueprintAreaEnterPoint2 = SimpleBlueprintExtendAsObject.Or(EnterPoint.Get(), null);
				obj = ((blueprintAreaEnterPoint2 == null) ? null : SimpleBlueprintExtendAsObject.Or(blueprintAreaEnterPoint2.Area, null)?.StaticScene?.SceneName) ?? "";
			}
			return (string)obj;
		}
	}

	public string AreaPartName
	{
		get
		{
			BlueprintAreaEnterPoint blueprintAreaEnterPoint = SimpleBlueprintExtendAsObject.Or(EnterPoint.Get(), null);
			if (blueprintAreaEnterPoint == null)
			{
				return null;
			}
			return SimpleBlueprintExtendAsObject.Or(blueprintAreaEnterPoint.AreaPart, null)?.name;
		}
	}

	public string AreaPartLightScenes => string.Join(", ", from x in ArbiterUtils.GetLightScenes(EnterPoint.Get())
		select x.SceneName);

	public IEnumerable<ArbiterPoint> Points => PointList.ElementList.Select((ArbiterElement x) => x as ArbiterPoint);

	public override string GetCaption()
	{
		string text = "No Enter Point";
		if (EnterPoint.Get() != null)
		{
			object obj = AreaPartName;
			if (obj == null)
			{
				BlueprintAreaEnterPoint blueprintAreaEnterPoint = SimpleBlueprintExtendAsObject.Or(EnterPoint.Get(), null);
				obj = ((blueprintAreaEnterPoint == null) ? null : SimpleBlueprintExtendAsObject.Or(blueprintAreaEnterPoint.Area, null)?.name) ?? "Bad Enterpoint! Enterpoint must have Area or AreaPart.";
			}
			text = (string)obj;
		}
		return text + " (" + StaticSceneName + ")";
	}
}
