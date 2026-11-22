using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Soundcheck.Web.Models;

namespace Soundcheck.Web.Services;

public class ApplicationUserManager(
    IUserStore<ApplicationUser> userStore,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IEnumerable<IUserValidator<ApplicationUser>> userValidators,
    IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<UserManager<ApplicationUser>> logger)
    : UserManager<ApplicationUser>(userStore, optionsAccessor, passwordHasher, userValidators, passwordValidators,
        keyNormalizer, errors, services, logger)
{
    private const string DefaultRoleName = "Client";

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        var creationResult = await base.CreateAsync(user, password);
        if (!creationResult.Succeeded)
        {
            return creationResult;
        }

        return await AddToRoleAsync(user, DefaultRoleName);
    }
}
