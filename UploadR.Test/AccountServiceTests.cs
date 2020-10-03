using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enums;
using UploadR.Services;

namespace UploadR.Test
{
    public class AccountServiceTests
    {
        private const string InvalidEmail = "Allan Good";
        private const string InUseEmail = "Kieran@cool.com";
        private const string NotInUseEmail = "unit@test.co.uk";
        
        private static readonly User User = new User
        {
            Guid = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString()
        };
        
        private static readonly User Admin = new User
        {
            Guid = Guid.NewGuid(),
            Type = AccountType.Admin,
            Email = InUseEmail
        };

        private static readonly Upload Upload = new Upload
        {
            Guid = Guid.NewGuid(),
            AuthorGuid = User.Guid
        };
        
        private AccountService _service;
        private IServiceProvider _provider;

        [SetUp]
        public async Task InitialiseAsync()
        {
             _provider = new ServiceCollection()
                .AddDbContext<UploadRContext>(builder => builder.UseInMemoryDatabase("uploadr"))
                .BuildServiceProvider();
            _service = new AccountService(_provider, new NullLogger<AccountService>());

            using var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            await context.Users.AddRangeAsync(User, Admin);
            await context.Uploads.AddAsync(Upload);
            await context.SaveChangesAsync();
        }
        
        [TearDown]
        public async Task TearDownAsync()
        {
            using var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();
            // we need to delete the db to make sure it's not shared between tests
            await context.Database.EnsureDeletedAsync();
        }
        
        [Test]
        public async Task TestResetUserTokenNotFoundAsync()
        {
            var result = await _service.ResetUserTokenAsync(Guid.NewGuid());
            Assert.AreEqual(ResultCode.NotFound, result);
        }
        
        [Test]
        public async Task TestResetUserTokenAsync()
        {
            var result = await _service.ResetUserTokenAsync(User.Guid);
            Assert.AreEqual(ResultCode.Ok, result);

            var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();
            var user = await context.Users.FindAsync(User.Guid);
            Assert.AreNotEqual(User.Token, user.Token);
        }
        
        [Test]
        public async Task TestToggleAcccountStateNotFoundAsync()
        {
            var result = await _service.ToggleAccountStateAsync(Guid.NewGuid(), true);
            Assert.AreEqual(ResultCode.NotFound, result);
        }
        
        [Test]
        public async Task TestToggleAccountStateAdminAsync()
        {
            var result = await _service.ToggleAccountStateAsync(Admin.Guid, true);
            Assert.AreEqual(ResultCode.Invalid, result);
        }
        
        [Test]
        public async Task TestToggleAccountStateBlockedAsync()
        {
            const bool blocked = true;
            
            var result = await _service.ToggleAccountStateAsync(User.Guid, blocked);
            Assert.AreEqual(ResultCode.Ok, result);

            var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();
            var user = await context.Users.FindAsync(User.Guid);
            Assert.AreEqual(blocked, user.Disabled);
        }
        
        [Test]
        public async Task TestToggleAccountStateUnblockedAsync()
        {
            const bool blocked = false;
            
            var result = await _service.ToggleAccountStateAsync(User.Guid, blocked);
            Assert.AreEqual(ResultCode.Ok, result);

            var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();
            var user = await context.Users.FindAsync(User.Guid);
            Assert.AreEqual(blocked, user.Disabled);
        }
        
        [Test]
        public async Task TestDeleteAccountNotFoundAsync()
        {
            var result = await _service.DeleteAccountAsync(Guid.NewGuid(), true);
            Assert.AreEqual(ResultCode.NotFound, result);
        }
        
        [Test]
        public async Task TestDeleteAccountNotCascadeAsync()
        {
            var result = await _service.DeleteAccountAsync(User.Guid, false);
            Assert.AreEqual(ResultCode.Ok, result);
            
            var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();
            var user = await context.Users.FindAsync(User.Guid);
            Assert.Null(user);
            var upload = await context.Uploads.FindAsync(Upload.Guid);
            Assert.NotNull(upload);
        }
        
        [Test]
        public async Task TestDeleteAccountCascadeAsync()
        {
            var result = await _service.DeleteAccountAsync(User.Guid, true);
            Assert.AreEqual(ResultCode.Ok, result);
            
            var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();
            var user = await context.Users.FindAsync(User.Guid);
            Assert.Null(user);
            var upload = await context.Uploads.FindAsync(Upload.Guid);
            Assert.Null(upload);
        }
        
        [Test]
        public async Task TestCreateAccountWithInvalidEmailAsync()
        {
            var result = await _service.CreateAccountAsync(InvalidEmail);
            Assert.AreEqual(ResultCode.Invalid, result);
        }
        
        [Test]
        public async Task TestCreateAccountEmailInUseAsync()
        {
            var result = await _service.CreateAccountAsync(InUseEmail);
            Assert.AreEqual(ResultCode.EmailInUse, result);
        }
        
        [Test]
        public async Task TestCreateAccountNotInUseEmailAsync()
        {
            var result = await _service.CreateAccountAsync(NotInUseEmail);
            Assert.AreEqual(ResultCode.Ok, result);

            using var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();

            var foundEmail = await context.Users.AnyAsync(user => user.Email == NotInUseEmail);
            Assert.True(foundEmail);
        }
        
        [Test]
        public async Task TestVerifyAccountWithEmptyTokenAsync()
        {
            var result = await _service.VerifyAccountAsync(string.Empty);
            Assert.False(result);
        }
        
        [Test]
        public async Task TestVerifyAccountWithNullTokenAsync()
        {
            var result = await _service.VerifyAccountAsync(null);
            Assert.False(result);
        }
        
        [Test]
        public async Task TestVerifyAccountUserNotFoundAsync()
        {
            var result = await _service.VerifyAccountAsync(Guid.NewGuid().ToString());
            Assert.False(result);
        }

        [Test]
        public async Task TestVerifyAccountVerifiedAccountAsync()
        {
            var result = await _service.VerifyAccountAsync(Admin.Token);
            Assert.False(result);
        }
        
        [Test]
        public async Task TestVerifyAccountUnverifiedAccountAsync()
        {
            var result = await _service.VerifyAccountAsync(User.Token);
            Assert.True(result);

            using var scope = _provider.CreateScope();
            await using var context = scope.ServiceProvider.GetService<UploadRContext>();

            var user = await context.Users.FindAsync(User.Guid);
            Assert.AreEqual(AccountType.User, user.Type);
        }
    }
}