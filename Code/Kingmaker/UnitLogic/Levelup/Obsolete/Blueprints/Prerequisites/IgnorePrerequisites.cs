using System;
using Core.Cheats;
using Kingmaker.Utility.BuildModeUtils;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

public class IgnorePrerequisites : IDisposable
{
	private static int s_Ignore;

	[Cheat(Name = "ignore_prereq", Description = "When true, prerequisites will be ignored")]
	public static bool IgnorePrerequisitesAlways { get; set; }

	public static bool Ignore
	{
		get
		{
			if (s_Ignore <= 0)
			{
				if (BuildModeUtility.IsDevelopment)
				{
					return IgnorePrerequisitesAlways;
				}
				return false;
			}
			return true;
		}
	}

	public IgnorePrerequisites()
	{
		s_Ignore++;
	}

	public void Dispose()
	{
		s_Ignore--;
	}
}
