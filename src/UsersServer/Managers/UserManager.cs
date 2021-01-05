using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UsersServer.Contracts;
using UsersServer.Cryptography.Services;
using UsersServer.Data;
using UsersServer.Models;

namespace UsersServer.Managers
{
    public class UserManager : UserManager<User>, IUserManager
    {
        private readonly IHasher _hasher;
        private readonly IUsersRepo _usersRepo;
        public UserManager(IUsersRepo store, IOptions<IdentityOptions> optionsAccessor,
            IHasher passwordHasher, IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : base(store,
            optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services,
            logger)
        {
            _hasher = passwordHasher;
            _usersRepo = store;
        }

        public override async Task<IdentityResult> CreateAsync(User user)
        {
            ThrowIfDisposed();
            var result = await ValidateUserAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }
            if (Options.Lockout.AllowedForNewUsers && SupportsUserLockout)
            {
                // await GetUserLockoutStore().SetLockoutEnabledAsync(user, true, CancellationToken);
            }
            await UpdateNormalizedUserNameAsync(user);
            await UpdateNormalizedEmailAsync(user);

            return  await _usersRepo.CreateAsync(user, CancellationToken);
        }

        public override async Task<bool> CheckPasswordAsync(User user, string password)
        {
            ThrowIfDisposed();
            var passwordStore = GetPasswordStore();

            var result = await VerifyPasswordAsync(passwordStore, user, password);
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                //Remove the IPasswordStore parameter so we can call the protected, not private, method
                await UpdatePasswordHash(user, password, validatePassword: false);
                await UpdateUserAsync(user);
            }

            var success = result != PasswordVerificationResult.Failed;
            if (!success)
            {
                var userId = user != null ? GetUserIdAsync(user).Result : "(null)";
                Logger.LogWarning(0, "Invalid password for user {userId}.", userId);
            }
            return success;
        }

        protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<User> store, User user, string password)
        {
            string existingHash;

            if (user != null)
                existingHash = await store.GetPasswordHashAsync(user, CancellationToken);
            else
                existingHash = "not a real hash";

            if (existingHash == null)
            {
                return PasswordVerificationResult.Failed;
            }
            return PasswordHasher.VerifyHashedPassword(user, existingHash, password);
        }

        public override async Task<User> FindByNameAsync(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            User user;

            if (Store is IUsersRepo repo)
            {
                user = await repo.FindByNameAsync(userName, CancellationToken);
            }
            else
            {
                userName = NormalizeName(userName);
                user = await Store.FindByNameAsync(userName, CancellationToken);
            }
            
            return user;
        }
        

        public override async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            ThrowIfDisposed();
            var passwordStore = GetPasswordStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (await VerifyPasswordAsync(passwordStore, user, currentPassword) != PasswordVerificationResult.Failed)
            {
                var result = await UpdatePasswordHash(user, newPassword, validatePassword: false);
                if (!result.Succeeded)
                {
                    return result;
                }

                return await UpdateUserAsync(user);
            }
            Logger.LogWarning(2, "Change password failed for user {userId}.", await GetUserIdAsync(user));
            return IdentityResult.Failed(ErrorDescriber.PasswordMismatch());
        }
        

        private IUserPasswordStore<User> GetPasswordStore()
        {
            if (Store is IUserPasswordStore<User> passwordStore)
            {
                return passwordStore;
            }

            return null;
        }

        public override async Task<User> FindByEmailAsync(string email)
        {
            User user = null;

            if (Store is IUsersRepo repo)
            {
                user = await repo.FindByEmailAsync(email, CancellationToken);
            }
            else
            {
                user = await Store.FindByNameAsync(email, CancellationToken);
            }
            
            return user;
        }
        
    }
}