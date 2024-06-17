using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.Modding;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>
	{
		new CheatMethodInfoInternal(new Action(OwlcatModificationsManager.CheatReloadData), "void CheatReloadData()", "reload_modifications_data", "Reload data for all modifications", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void")
	};

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>();

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>();

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>();
}
