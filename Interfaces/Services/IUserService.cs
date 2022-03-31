using Entities.DataTransferObjects.User;
using Entities.ViewModels.User;
using System.Threading.Tasks;

namespace Interfaces.Services
{
    public interface IUserService
    {
        Task<string> Register(RegisterInputDTO registerInputModel);

        Task<LoginViewModel> Login(LoginInputDTO user);

        Task Logout();
    }
}
