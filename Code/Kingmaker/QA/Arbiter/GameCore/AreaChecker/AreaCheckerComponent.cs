using System.Linq;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.QA.Arbiter.Tasks;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;

namespace Kingmaker.QA.Arbiter.GameCore.AreaChecker;

[ComponentName("Arbiter/Area Checker")]
[AllowedOn(typeof(BlueprintArbiterInstruction))]
[AllowMultipleComponents]
[TypeId("61a12f3449764a98bbb3774f0e76c3f2")]
public class AreaCheckerComponent : BlueprintComponent, IArbiterCheckerComponent
{
	[ValidateNotNull]
	public BlueprintAreaReference Area;

	public BlueprintAreaPresetReference OverrideAreaPreset;

	public bool OverrideTimeOfDay;

	[ShowIf("OverrideTimeOfDay")]
	public TimeOfDay TimeOfDay;

	public bool MakeMapScreenshot;

	public ArbiterElementList AreaParts = new ArbiterElementList(typeof(AreaCheckerComponentPart));

	public BlueprintAreaPreset Preset => OverrideAreaPreset?.Get() ?? Area.Get().DefaultPreset;

	public int GetSampleId(ArbiterPointElement point)
	{
		int num = 0;
		foreach (ArbiterElement element in AreaParts.ElementList)
		{
			AreaCheckerComponentPart areaCheckerComponentPart = element as AreaCheckerComponentPart;
			int num2 = areaCheckerComponentPart.PointList.ElementList.IndexOf(point);
			if (num2 > -1)
			{
				return num2 + num;
			}
			num += areaCheckerComponentPart.PointList.ElementList.Count;
		}
		return -1;
	}

	public ArbiterPointElement GetPointById(int id)
	{
		foreach (ArbiterElement element in AreaParts.ElementList)
		{
			ArbiterPointElement arbiterPointElement = (element as AreaCheckerComponentPart).Points.FirstOrDefault((ArbiterPointElement x) => GetSampleId(x) == id);
			if (arbiterPointElement != null)
			{
				return arbiterPointElement;
			}
		}
		return null;
	}

	public BlueprintAreaEnterPointReference GetEnterPointById(int id)
	{
		foreach (ArbiterElement element in AreaParts.ElementList)
		{
			AreaCheckerComponentPart areaCheckerComponentPart = element as AreaCheckerComponentPart;
			if (areaCheckerComponentPart.Points.FirstOrDefault((ArbiterPointElement x) => GetSampleId(x) == id) != null)
			{
				return areaCheckerComponentPart.EnterPoint;
			}
		}
		return null;
	}

	public ArbiterTask GetArbiterTask(ArbiterStartupParameters arguments)
	{
		return new AreaCheckerTask(this, arguments);
	}
}
