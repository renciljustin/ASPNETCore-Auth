using System;
using System.Threading.Tasks;
using API.Core;
using API.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Persistence
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public RefreshToken  CreateRefreshToken(string userId)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Value = Guid.NewGuid().ToString(),
                ExpirationDate = DateTime.UtcNow.AddHours(12),
                TotalRefresh = 0,
                Revoked = false,
                CreationTime = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);

            return refreshToken;
        }

        public async Task<RefreshToken> FindByValueAsync(string value)
        {
            return await _context.RefreshTokens.SingleOrDefaultAsync(rt => !rt.Revoked && rt.Value == value);
        }

        public void Refresh(RefreshToken refreshToken)
        {
            refreshToken.ExpirationDate = DateTime.UtcNow.AddHours(12);
            refreshToken.TotalRefresh++;
            refreshToken.LastModified = DateTime.UtcNow;
        }

        public void RevokeToken(RefreshToken refreshToken)
        {
            refreshToken.Revoked = true;
            refreshToken.LastModified = DateTime.UtcNow;
        }
    }
}