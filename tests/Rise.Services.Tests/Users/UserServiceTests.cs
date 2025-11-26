using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Tests.Fakers;
using Rise.Services.Tests.Helpers;
using Rise.Services.Users;
using Rise.Shared.Users;
using Rise.Tests.Shared;

namespace Rise.Services.Tests.Users;

public class UserServiceTests
{
    private EFFixture _fixture;

    public UserServiceTests(EFFixture fixture)
    {
        _fixture = fixture;
    }

    private IUserService CreateUserService(User? loggedInUser, ApplicationDbContext dbcontext)
    {
        if (_fixture is null)
            throw new ArgumentNullException(nameof(_fixture));

        return new UserService(
            dbcontext,
            new FakeSessionContextProvider(ServicesData.GetValidClaimsPrincipal(loggedInUser))
        );
    }
}
