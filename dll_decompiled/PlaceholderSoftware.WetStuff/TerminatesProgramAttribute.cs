using System;

namespace PlaceholderSoftware.WetStuff;

[Obsolete("Use [ContractAnnotation('=> halt')] instead")]
[AttributeUsage(AttributeTargets.Method)]
internal sealed class TerminatesProgramAttribute : Attribute
{
}
