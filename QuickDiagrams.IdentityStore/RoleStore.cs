using Dapper;
using Microsoft.AspNetCore.Identity;
using QuickDiagrams.Storage;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.IdentityStore
{
    public class RoleStore
        : IRoleStore<ApplicationRole>
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public RoleStore(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
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
                                @"INSERT INTO [ApplicationRole] ([Name], [NormalizedName])
                                VALUES (@Name, @NormalizedName)",
                            parameters: role,
                            cancellationToken: cancellationToken
                        );

                        await connection.ExecuteAsync(insertCmd);

                        var selectCmd = new CommandDefinition
                        (
                            commandText:
                                @"SELECT [Id] FROM [ApplicationRole]
                                WHERE [Name] = @Name AND [NormalizedName] = @NormalizedName",
                            parameters: role,
                            cancellationToken: cancellationToken
                        );

                        var newId = await connection.ExecuteScalarAsync<int?>(selectCmd);
                        if (newId.HasValue && newId.Value > 0)
                        {
                            role.Id = newId.Value;

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

        public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
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
                                @"DELETE FROM [ApplicationRole]
                                WHERE [Id] = @Id",
                            parameters: role,
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

        public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT [Id], [Name], [NormalizedName] FROM [ApplicationRole]
                        WHERE [Id] = @Id",
                    parameters: new { Id = roleId },
                    cancellationToken: cancellationToken
                );

                return await connection.QuerySingleOrDefaultAsync<ApplicationRole>(cmd);
            }
        }

        public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var cmd = new CommandDefinition
                (
                    commandText:
                        @"SELECT [Id], [Name], [NormalizedName] FROM [ApplicationRole]
                        WHERE [NormalizedName] = @NormalizedName",
                    parameters: new { NormalizedName = normalizedRoleName },
                    cancellationToken: cancellationToken
                );

                return await connection.QuerySingleOrDefaultAsync<ApplicationRole>(cmd);
            }
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
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
                                [Name] = @Name,
                                [NormalizedName] = @NormalizedName
                                WHERE [Id] = @Id",
                            parameters: role,
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