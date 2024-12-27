namespace ProductService.Application.Interfaces
{
    public interface IUserServiceClient
    {
        Task<bool> IsUserValidAsync(int userId);
    }

}
