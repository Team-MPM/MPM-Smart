using System.Net;
using System.Net.Http.Json;

namespace Frontend.Services;

public class ResponseModel
{
    public bool Success { get; set; }
    public HttpStatusCode? StatusCode { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }

    public ResponseModel NoClientError(string? message = "No client")
    {
        Success = false;
        StatusCode = null;
        Message = message;
        return this;
    }

    public ResponseModel ServerError(HttpResponseMessage? response)
    {
        Success = false;
        StatusCode = response?.StatusCode;
        Message = response?.ReasonPhrase;
        return this;
    }

    public Task<ResponseModel> SuccessResultAsync(HttpResponseMessage response)
    {
        Success = true;
        Message = response.ReasonPhrase;
        StatusCode = response.StatusCode;
        return Task.FromResult(this);
    }

    public ResponseModel ExceptionError(Exception? exception)
    {
        Success = false;
        StatusCode = null;
        Message = exception?.Message;
        Exception = exception;
        return this;
    }
}

public class ResponseModel<T> : ResponseModel where T : class
{
    public T? Response { get; set; }

    public new ResponseModel<T> NoClientError(string? message = "No client")
    {
        Success = false;
        StatusCode = null;
        Message = message;
        return this;
    }

    public new ResponseModel<T> ServerError(HttpResponseMessage? response)
    {
        Success = false;
        StatusCode = response?.StatusCode;
        Message = response?.ReasonPhrase;
        return this;
    }

    public new async Task<ResponseModel<T>> SuccessResultAsync(HttpResponseMessage response)
    {
        Success = true;
        StatusCode = response.StatusCode;
        Message = response.ReasonPhrase;
        Response = await response.Content.ReadFromJsonAsync<T>();
        return this;
    }

    public new ResponseModel<T> ExceptionError(Exception? exception)
    {
        Success = false;
        StatusCode = null;
        Message = exception?.Message;
        Exception = exception;
        return this;
    }
}