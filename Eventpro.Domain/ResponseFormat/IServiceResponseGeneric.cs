namespace Eventpro.Domain.ResponseFormat
{
    public interface IServiceResponse<T> : IServiceResponse
    {
        T? Data { get; set; }
    }
}
