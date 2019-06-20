using System.Threading.Tasks;
using API.Core.Models;

namespace API.Core
{
    public interface IRefreshTokenRepository
    {
        RefreshToken CreateRefreshToken(string userId);
        Task<RefreshToken> FindByValueAsync(string value);
        void Refresh(RefreshToken refreshToken);
        void RevokeToken(RefreshToken refreshToken);
    }
}