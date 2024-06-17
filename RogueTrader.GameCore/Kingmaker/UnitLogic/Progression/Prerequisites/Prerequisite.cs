using System;
using JetBrains.Annotations;
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

	public bool Meet([NotNull] IBaseUnitEntity unit)
	{
		bool flag = MeetsInternal(unit);
		if (!Not)
		{
			return flag;
		}
		return !flag;
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
