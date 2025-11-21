namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Exceptions;

public class DataPathEvaluationException : Exception
{
    public string? PathExpression { get; }

    public DataPathEvaluationException(string message) : base(message)
    {
    }

    public DataPathEvaluationException(string message, string pathExpression)
        : base(message)
    {
        PathExpression = pathExpression;
    }


    public DataPathEvaluationException(string message, string pathExpression, Exception innerException)
        : base(message, innerException)
    {
        PathExpression = pathExpression;
    }
}