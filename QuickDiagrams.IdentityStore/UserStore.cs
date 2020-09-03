using Dapper;
using Microsoft.AspNetCore.Identity;
using QuickDiagrams.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.IdentityStore
{
    public class UserStore
        : IUserStore<ApplicationUser>
        , IUserEmailStore<ApplicationUser>
        , IUserPhoneNumberStore<ApplicationUser>
        , IUserTwoFactorStore<ApplicationUser>
        , IUserPasswordStore<ApplicationUser>
        , IUserRoleStore<ApplicationUser>
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public UserStore(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var selectRoleCmd = new CommandDefinition
                        (
                            commandText:
                                @"SELECT [Id] FROM [ApplicationRole]
                                WHERE [NormalizedName] = @NormalizedName",
                            parameters: new { NormalizedName = roleName.ToUpper() },
                            cancellationToken: cancellationToken
                        );

                        var roleId = await connection.ExecuteScalarAsync<int?>(selectRoleCmd);
                        if (!roleId.HasValue)
                        {
                            var insertRoleCmd = new CommandDefinition
                            (
                                commandText:
                                    @"INSERT INTO [ApplicationRole] ([Name], [NormalizedName])
                                    VALUES (@Name, @NormalizedName)",
                                parameters: new { Name = roleName, NormalizedName = roleName.ToUpper() },
                                cancellationToken: cancellationToken
                            );

                            await connection.ExecuteAsync(insertRoleCmd);

                            roleId = await connection.ExecuteScalarAsync<int?>(selectRoleCmd);
                        }

                        var insertCmd = new CommandDefinition
                        (
                            commandText:
                                @"IF NOT EXISTS(
                                    SELECT 1 FROM [ApplicationUserRole]
                                    WHERE [UserId] = @UserId AND [RoleId] = @RoleId
                                )
                                INSERT INTO [ApplicationUserRole] ([UserId], [RoleId])
                                VALUES(@UserId, @RoleId)",
                            parameters: new { UserId = user.Id, RoleId = roleId },
                            cancellationToken: cancellationToken
                        );

                        await connection.ExecuteAsync(insertCmd);

                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw ex;
                    }
                }
            }
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var insertCmd = new CommandDefinition
                        (
                            commandText:
                                @"INSERT INTO [ApplicationUser] ([UserName], [NormalizedUserName], [Email],
                                [NormalizedEmail], [EmailConfirmed], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed],
                                [TwoFactorEnabled])
                                VALUES (@UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, @PasswordHash,
                                @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled)",
                            parameters: user,
                            cancellationToken: cancellationToken
                        );

                        await connection.ExecuteAsync(insertCmd);

                        var selectCmd = new CommandDefinition
                        (
                            commandText:
                                @"SELECT [Id] FROM [ApplicationUser]
                                WHERE [NormalizedUserName] = @NormalizedUserName AND [NormalizedEmail] = @NormalizedEmail",
                            parameters: user,
                            cancellationToken: cancellationToken
                        );

                        var newId = await connection.ExecuteScalarAsync<int?>(selectCmd);
                        if (newId.HasValue && newId.Value > 0)
                        {
                            user.Id = newId.Value;

                            await transaction.CommitAsync(cancellationToken);
                            return IdentityResult.Success;
                        }
                        else
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return IdentityResult.Failed(new IdentityError()
                            {
                                Code = "User_Lookup_Error",
                                Description = "Failed to lookup newly added user."
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw ex;
                    }
                }
            }
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var cmd = new CommandDefinition
                        (
                            commandText:
                                @"DELETE FROM [ApplicationUser]
                                WHERE [Id] = @Id",
                            parameters: user,
                            cancellationToken: cancellationToken
                        );

                        await connection.ExecuteAsync(cmd);
                        await transaction.CommitAsync(cancellationToken);

                        return IdentityResult.Success;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw ex;
                    }
                }
            }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                        [EmailConfirmed], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled]
                        FROM [ApplicationUser]
                        WHERE [NormalizedEmail] = @NormalizedEmail",
                    parameters: new { NormalizedEmail = normalizedEmail },
                    cancellationToken: cancellationToken
                );

                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>(cmd);
            }
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                        [EmailConfirmed], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled]
                        FROM [ApplicationUser]
                        WHERE [Id] = @Id",
                    parameters: new { Id = userId },
                    cancellationToken: cancellationToken
                );

                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>(cmd);
            }
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
                        [EmailConfirmed], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled]
                        FROM [ApplicationUser]
                        WHERE [NormalizedUserName] = @NormalizedUserName",
                    parameters: new { NormalizedUserName = normalizedUserName },
                    cancellationToken: cancellationToken
                );

                return await connection.QuerySingleOrDefaultAsync<ApplicationUser>(cmd);
            }
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT r.[Name] FROM [ApplicationRole] r
                        INNER JOIN [ApplicationUserRole] ur ON ur.[RoleId] = r.Id
                        WHERE ur.UserId = @UserId",
                    parameters: new { UserId = user.Id },
                    cancellationToken: cancellationToken
                );

                return (await connection.QueryAsync<string>(cmd)).ToList();
            }
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT u.[Id], u.[UserName], u.[NormalizedUserName], u.[Email], u.[NormalizedEmail],
                        u.[EmailConfirmed], u.[PasswordHash], u.[PhoneNumber], u.[PhoneNumberConfirmed],
                        u.[TwoFactorEnabled] FROM [ApplicationUser] u
                        INNER JOIN [ApplicationUserRole] ur ON ur.[UserId] = u.[Id]
                        INNER JOIN [ApplicationRole] r ON r.[Id] = ur.[RoleId]
                        WHERE r.[NormalizedName] = @NormalizedName",
                    parameters: new { NormalizedName = roleName.ToUpper() },
                    cancellationToken: cancellationToken
                );

                return (await connection.QueryAsync<ApplicationUser>(cmd)).ToList();
            }
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var selectRoleCmd = new CommandDefinition
                        (
                            commandText:
                                @"SELECT [Id] FROM [ApplicationRole]
                                WHERE [NormalizedName] = @NormalizedName",
                            parameters: new { NormalizedName = roleName.ToUpper() },
                            cancellationToken: cancellationToken
                        );

                var roleId = await connection.ExecuteScalarAsync<int?>(selectRoleCmd);
                if (!roleId.HasValue)
                    return false;

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT COUNT(1) FROM [ApplicationUserRole]
                        WHERE [UserId] = @UserId AND [RoleId] = @RoleId",
                    parameters: new { UserId = user.Id, RoleId = roleId },
                    cancellationToken: cancellationToken
                );

                return (await connection.ExecuteScalarAsync<int>(cmd) > 0);
            }
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var selectRoleCmd = new CommandDefinition
                        (
                            commandText:
                                @"SELECT [Id] FROM [ApplicationRole]
                                WHERE [NormalizedName] = @NormalizedName",
                            parameters: new { NormalizedName = roleName.ToUpper() },
                            cancellationToken: cancellationToken
                        );

                        var roleId = await connection.ExecuteScalarAsync<int?>(selectRoleCmd);
                        if (roleId.HasValue)
                        {
                            var deleteCmd = new CommandDefinition
                            (
                                commandText:
                                    @"DELETE FROM [ApplicationUserRole]
                                    WHERE [UserId] = @UserId AND [RoleId] = @RoleId",
                                parameters: new { UserId = user.Id, RoleId = roleId },
                                cancellationToken: cancellationToken
                            );

                            await connection.ExecuteAsync(deleteCmd);

                            await transaction.CommitAsync(cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw ex;
                    }
                }
            }
        }

        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var cmd = new CommandDefinition
                        (
                            commandText:
                                @"UPDATE [ApplicationRole] SET
                                [UserName] = @UserName,
                                [NormalizedUserName] = @NormalizedUserName,
                                [Email] = @Email,
                                [NormalizedEmail] = @NormalizedEmail,
                                [EmailConfirmed] = @EmailConfirmed,
                                [PasswordHash] = @PasswordHash,
                                [PhoneNumber] = @PhoneNumber,
                                [PhoneNumberConfirmed] = @PhoneNumberConfirmed,
                                [TwoFactorEnabled] = @TwoFactorEnabled
                                WHERE [Id] = @Id",
                            parameters: user,
                            cancellationToken: cancellationToken
                        );

                        await connection.ExecuteAsync(cmd);
                        await transaction.CommitAsync(cancellationToken);

                        return IdentityResult.Success;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw ex;
                    }
                }
            }
        }
    }
}