﻿namespace BackendConnectionData.Model;

public class ResponseModel : ResponseModel<object>
{
    public ResponseModel()
    {
        Response = null;
    }
}

public class ResponseModel<TResponseType> where TResponseType : class
{
    public required bool Success { get; set; }
    public required int StatusCode { get; set; }
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public TResponseType? Response { get; set; }

    public static ResponseModel Parse(ResponseModel<TResponseType> model)
    {
        return new ResponseModel
        {
            Success = model.Success,
            StatusCode = model.StatusCode,
            Message = model.Message,
            Exception = model.Exception,
            Response = null
        };
    }
}