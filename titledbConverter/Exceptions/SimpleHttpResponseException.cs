using System.Net;

namespace titledbConverter.Exceptions;

public class SimpleHttpResponseException(HttpStatusCode statusCode, string content) : Exception(content)
{
    public HttpStatusCode StatusCode { get; private set; } = statusCode;
}