using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Reflection;

namespace Core.Cheats;

public static class ArgumentConverter
{
	public delegate(bool accepted, string result) PreprocessDelegate(string value, string type);

	public delegate(bool accepted, object result) ConvertDelegate(string value, Type targetType);

	private static readonly Task<(ConvertDelegate[] converters, PreprocessDelegate[] preprocessors)> InternalsTask = Task.Run((Func<(ConvertDelegate[], PreprocessDelegate[])>)GetInternals);

	private static ConvertDelegate[] Converters => InternalsTask.Result.converters;

	private static PreprocessDelegate[] Preprocessors => InternalsTask.Result.preprocessors;

	private static (ConvertDelegate[] converters, PreprocessDelegate[] preprocessors) GetInternals()
	{
		List<(PreprocessDelegate, int)> list = new List<(PreprocessDelegate, int)>();
		List<(ConvertDelegate, int)> list2 = new List<(ConvertDelegate, int)>();
		Assembly[] assembliesSafe = AppDomain.CurrentDomain.GetAssembliesSafe();
		for (int i = 0; i < assembliesSafe.Length; i++)
		{
			Type type = assembliesSafe[i].GetType("CheatsCodeGen.AllCheats");
			if (!(type == null))
			{
				FieldInfo field = type.GetField("ArgConverters");
				if (field != null)
				{
					list2.AddRange((List<(ConvertDelegate, int)>)field.GetValue(null));
				}
				FieldInfo field2 = type.GetField("ArgPreprocessors");
				if (field2 != null)
				{
					list.AddRange((List<(PreprocessDelegate, int)>)field2.GetValue(null));
				}
			}
		}
		return (converters: (from m in list2
			orderby m.Item2
			select m.Item1).ToArray(), preprocessors: (from m in list
			orderby m.Item2
			select m.Item1).ToArray());
	}

	public static object Convert(string parameter, Type targetType, string name)
	{
		try
		{
			var (flag, obj) = Converters.Select((ConvertDelegate v) => v(parameter, targetType)).FirstOrDefault(((bool accepted, object result) v) => v.accepted);
			if (flag && (obj == null || targetType.IsInstanceOfType(obj)))
			{
				return obj;
			}
			if (targetType.IsInstanceOfType(parameter))
			{
				return parameter;
			}
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"Cant convert string '{parameter}' to {targetType} {name}", name, innerException);
		}
		throw new ArgumentException($"Cant convert string '{parameter}' to {targetType} {name}", name);
	}

	public static string Preprocess(string parameter, string targetType, string name)
	{
		try
		{
			var (flag, text) = Preprocessors.Select((PreprocessDelegate v) => v(parameter, targetType)).FirstOrDefault(((bool accepted, string result) v) => v.accepted);
			return flag ? text : parameter;
		}
		catch (Exception innerException)
		{
			throw new ArgumentException("Cant preprocess string '" + parameter + "' for argument " + name, name, innerException);
		}
	}
}
