namespace AguasNico_Api.Models.DTO;

public class BaseResponse<T>
{
    public T Data { get; set; }
    public string Message { get; set; }
    public ErrorResponse Error { get; set; }
    public bool Success => Error == null;

    public class ErrorResponse
    {
        public string Message { get; set; }
        public int Code { get; set; }
    }

    public BaseResponse<T> SetError(string error, int code = 400)
    {
        Error = new ErrorResponse { Message = error, Code = code };
        return this;
    }

    public BaseResponse<T> Attach<TResponse>(BaseResponse<TResponse> response)
    {
        if (Error == null && response.Error != null)
            Error = new ErrorResponse { Message = response.Error.Message, Code = response.Error.Code };

        return this;
    }
}

public class BaseResponse
{
    public object Data { get; set; }
    public string Message { get; set; }
    public ErrorResponse Error { get; set; }
    public bool Success => Error == null;

    public class ErrorResponse
    {
        public string Message { get; set; }
        public int Code { get; set; }
    }

    public BaseResponse SetError(string error, int code = 400)
    {
        Error = new ErrorResponse { Message = error, Code = code };
        return this;
    }
}

