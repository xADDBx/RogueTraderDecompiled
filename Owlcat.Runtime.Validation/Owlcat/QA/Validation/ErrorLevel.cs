using System;

namespace Owlcat.QA.Validation;

[Flags]
public enum ErrorLevel
{
	Unprioritized = 1,
	Blocker = 2,
	Critical = 4,
	Normal = 8,
	Minor = 0x10,
	Trivial = 0x20,
	Performance = 0x40
}
