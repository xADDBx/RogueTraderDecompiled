using System;
using System.Reflection;

namespace Owlcat.Runtime.Core.Logging;

public interface IStackTraceFormatter
{
	string ToString(MethodBase methodBase);

	string ToString(Type type);

	string ToString(ParameterInfo[] parameterInfos);

	string Format(string declaringType, string methodName, string parameters, string filename, int lineNumber);
}
