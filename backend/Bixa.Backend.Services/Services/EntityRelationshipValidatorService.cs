using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace Bixa.Backend.Services.Services;

public class EntityRelationshipValidatorService : IEntityRelationshipValidatorService
{
    private readonly AppDbContext _dbContext;

    public EntityRelationshipValidatorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> HasLinkedRecords<TEntity>(int entityId, params string[] excludedTables) where TEntity : class
    {
        var principalEntityType = _dbContext.Model.FindEntityType(typeof(TEntity));
        if (principalEntityType == null)
            return Fail($"Entity type {typeof(TEntity).Name} not found in model.");

        if (!HasValidPrimaryKey(principalEntityType))
            return Fail($"{typeof(TEntity).Name} must have a single integer primary key.");

        var excluded = new HashSet<string>(excludedTables, StringComparer.OrdinalIgnoreCase);
        var linkedTables = new List<string>();

        foreach (var foreignKey in _dbContext.Model.GetEntityTypes().SelectMany(et => et.GetForeignKeys()))
        {
            if (foreignKey.PrincipalEntityType != principalEntityType)
                continue;

            var referencingEntity = foreignKey.DeclaringEntityType;
            if (referencingEntity == principalEntityType)
                continue;

            var referencingTable = referencingEntity.GetTableName() ?? referencingEntity.DisplayName();
            if (excluded.Contains(referencingTable))
                continue;

            if (!IsValidForeignKey(foreignKey))
                continue;

            var foreignKeyPropertyName = foreignKey.Properties[0].Name;
            var ignoredProps = new[] { "" };
            if (ignoredProps.Contains(foreignKeyPropertyName, StringComparer.OrdinalIgnoreCase))
                continue;

            var dbSet = GetDbSetInstance(referencingEntity.ClrType);
            if (dbSet == null)
                continue;

            var hasReference = await HasAnyReferenceAsync(foreignKey, referencingEntity.ClrType, dbSet, entityId);
            if (hasReference)
                linkedTables.Add(referencingTable);
        }

        if (linkedTables.Any())
        {
            var tables = string.Join(", ", linkedTables);
            return Result.Fail($"Cannot delete. It is linked to existing records in: {tables}", ErrorTypeEnum.Conflict);
        }
        return Result.Success();
    }

    private static bool HasValidPrimaryKey(IEntityType entityType)
    {
        var pk = entityType.FindPrimaryKey();
        return pk is not null &&
               pk.Properties.Count == 1 &&
               pk.Properties[0].ClrType == typeof(int);
    }

    private static bool IsValidForeignKey(IForeignKey foreignKey)
    {
        return foreignKey.Properties.Count == 1 &&
               (foreignKey.Properties[0].ClrType == typeof(int) || foreignKey.Properties[0].ClrType == typeof(int?));
    }

    private static MethodInfo? GetAnyAsyncMethod()
    {
        return typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m =>
                m.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync) &&
                m.IsGenericMethod &&
                m.GetGenericArguments().Length == 1)
            .Select(m => new
            {
                Method = m,
                Params = m.GetParameters()
            })
            .Where(x =>
                x.Params.Length >= 2 &&
                x.Params[0].ParameterType.IsGenericType &&
                x.Params[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>) &&
                x.Params[1].ParameterType.IsGenericType &&
                x.Params[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>))
            .Select(x => x.Method)
            .FirstOrDefault();
    }

    private static Result Fail(string message)
    {
        return Result.Fail($"Validation Error: {message}", ErrorTypeEnum.Database);
    }

    private object? GetDbSetInstance(Type clrType)
    {
        var property = _dbContext.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                 p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                                 p.PropertyType.GenericTypeArguments[0] == clrType);

        return property?.GetValue(_dbContext);
    }

    private async Task<bool> HasAnyReferenceAsync(IForeignKey foreignKey, Type entityType, object dbSetInstance, int entityId)
    {
        var dbSet = (IQueryable)dbSetInstance;

        var param = Expression.Parameter(entityType, "e");
        var property = Expression.Property(param, foreignKey.Properties[0].Name);

        Expression idConstant = Expression.Constant(entityId);
        if (foreignKey.Properties[0].ClrType == typeof(int?))
            idConstant = Expression.Convert(idConstant, typeof(int?));

        var body = Expression.Equal(property, idConstant);
        var lambdaType = typeof(Func<,>).MakeGenericType(entityType, typeof(bool));
        var lambda = Expression.Lambda(lambdaType, body, param);

        var anyAsyncMethod = GetAnyAsyncMethod()?.MakeGenericMethod(entityType);
        if (anyAsyncMethod == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Could not locate AnyAsync method via reflection.");
            return false;
        }

        var parameters = anyAsyncMethod.GetParameters().Length == 3
            ? new object[] { dbSet, lambda, CancellationToken.None }
            : new object[] { dbSet, lambda };

        var task = (Task<bool>)anyAsyncMethod.Invoke(null, parameters)!;
        return await task;
    }
}