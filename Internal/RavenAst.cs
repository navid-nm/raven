namespace Raven.Internal;

public class RavenAst
{
    public List<RavenFunction> Functions { get; } = [];
    public List<RavenStatement> Statements { get; } = [];
}
