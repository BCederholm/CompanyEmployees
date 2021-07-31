using Entities.DataTransferObjects;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IAuthenticationManager // CodeMaze 27
    {
        Task<bool> ValidateUser(UserForAuthenticationDto userForAuth);
        Task<string> CreateToken();
    }
}
