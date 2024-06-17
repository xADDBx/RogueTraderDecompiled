using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.QA.Arbiter.Profiling;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>
	{
		new CheatMethodInfoInternal(new Action(Counters.Log), "void Log()", "log_prof_counters", "Takes current profiling counters and log them to console", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void")
	};

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>();

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>();

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>();
}
