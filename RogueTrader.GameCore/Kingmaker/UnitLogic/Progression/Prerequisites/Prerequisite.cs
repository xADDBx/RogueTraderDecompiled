using System;
using Code.GameCore.ElementsSystem;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Progression.Prerequisites;

[Serializable]
[TypeId("51cef589bd0c4e019f005529dbae7c99")]
public abstract class Prerequisite : Element, IFeaturePrerequisite
{
	public bool Not;

	protected abstract bool MeetsInternal([NotNull] IBaseUnitEntity unit);

	protected abstract string GetCaptionInternal();

	public virtual bool IsRelyingOnFeature(BlueprintFact featureToAnalyze)
	{
		return false;
	}

	public bool Meet([NotNull] IBaseUnitEntity unit)
	{
		return Meet(null, unit);
	}

	public bool Meet([CanBeNull] ElementsList list, [NotNull] IBaseUnitEntity unit)
	{
		using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(list, this);
		try
		{
			bool flag = MeetsInternal(unit);
			bool flag2 = (Not ? (!flag) : flag);
			elementsDebugger?.SetResult(flag2 ? 1 : 0);
			return flag2;
		}
		catch (Exception exception)
		{
			Element.LogException(exception);
			elementsDebugger?.SetException(exception);
			throw;
		}
	}

	public override string GetCaption()
	{
		if (!Not)
		{
			return GetCaptionInternal();
		}
		return "!(" + GetCaptionInternal() + ")";
	}
}
