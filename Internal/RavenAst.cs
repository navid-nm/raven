namespace Raven.Internal;

public class RavenAst
{
    public List<RavenFunction> Functions { get; } = [];
    public List<RavenPrintStatement> Statements { get; } = [];
}
