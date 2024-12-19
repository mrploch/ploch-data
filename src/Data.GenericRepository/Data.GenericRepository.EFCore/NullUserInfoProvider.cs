using System.Security.Claims;
using Ploch.Common.AppServices;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Default implementation of <see cref="IUserInfoProvider" /> that always returns <c>null</c>.
/// </summary>
public class NullUserInfoProvider : IUserInfoProvider
{
    /// <summary>
    ///     Returns <c>null</c> for the current user info.
    /// </summary>
    /// <returns>
    ///     <c>null</c>
    /// </returns>
    public ClaimsPrincipal? GetCurrentUserInfo() => null;
}
