using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;

public class CharInfoClassEntryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public string ClassName { get; }

	public int Level { get; }

	public TooltipBaseTemplate Tooltip { get; }

	public CharInfoClassEntryVM(ClassData classData)
	{
		string name = classData.CharacterClass.Name;
		if (classData != null)
		{
			BlueprintArchetype blueprintArchetype = classData.Archetypes.FirstOrDefault();
			if (blueprintArchetype != null)
			{
				name = blueprintArchetype.Name;
			}
		}
		ClassName = name;
		Level = classData.Level;
		Tooltip = new TooltipTemplateClass(classData);
	}

	protected override void DisposeImplementation()
	{
	}
}
