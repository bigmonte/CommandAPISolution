using Microsoft.AspNetCore.Identity;
using UsersServer.Models;

namespace UsersServer.Cryptography.Services
{
    public interface IHasher : IPasswordHasher<User>
    { 
        string CreateHash(string text, BaseCryptoItem.HashAlgorithm algorithm);
        string CreateHash(string text, string salt, BaseCryptoItem.HashAlgorithm algorithm);
        bool MatchesHash(string text, string hash);
    }
}