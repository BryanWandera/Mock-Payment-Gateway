using System.Data;
using System.Reflection;
using Dapper;
using MySql.Data.MySqlClient;
using PaymentMock.Repositories.Interfaces;

namespace PaymentMock.Repositories.Implementations;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly string ConnectionString;
    protected readonly string TableName;

    protected BaseRepository(string connectionString, string tableName)
    {
        ConnectionString = connectionString;
        TableName = tableName;
    }

    protected IDbConnection CreateConnection() => new MySqlConnection(ConnectionString);

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(
            $"SELECT * FROM {TableName} WHERE Id = @Id", new { Id = id });
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        using var connection = CreateConnection();
        var result = await connection.QueryAsync<T>($"SELECT * FROM {TableName}");
        return result.ToList();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        using var connection = CreateConnection();
        var properties = GetWritableProperties(entity);
        var columns = string.Join(", ", properties.Select(p => p.Name));
        var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        var sql = $"INSERT INTO {TableName} ({columns}) VALUES ({parameters})";
        await connection.ExecuteAsync(sql, BuildParameters(entity, properties));
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        using var connection = CreateConnection();
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && p.CanRead && !IsNavigationProperty(p))
            .ToList();
        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
        var sql = $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";
        var parameters = BuildParameters(entity, properties);
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
            parameters["Id"] = idProperty.GetValue(entity)!;
        await connection.ExecuteAsync(sql, parameters);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        using var connection = CreateConnection();
        var affected = await connection.ExecuteAsync(
            $"DELETE FROM {TableName} WHERE Id = @Id", new { Id = id });
        return affected > 0;
    }

    protected async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<TResult>(sql, param);
    }

    protected async Task<TResult?> QueryFirstOrDefaultAsync<TResult>(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TResult>(sql, param);
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    private static List<PropertyInfo> GetWritableProperties(T entity)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && !IsNavigationProperty(p))
            .ToList();
        var idProperty = properties.FirstOrDefault(p => p.Name == "Id");
        if (idProperty != null)
        {
            var idValue = idProperty.GetValue(entity);
            if (idValue == null || string.IsNullOrEmpty(idValue.ToString()))
                properties = properties.Where(p => p.Name != "Id").ToList();
        }
        return properties;
    }

    private static Dictionary<string, object?> BuildParameters(T entity, List<PropertyInfo> properties)
    {
        var parameters = new Dictionary<string, object?>();
        foreach (var property in properties)
        {
            var value = property.GetValue(entity);
            parameters[property.Name] = property.PropertyType.IsEnum ? value?.ToString() : value;
        }
        return parameters;
    }

    private static bool IsNavigationProperty(PropertyInfo property) =>
        property.PropertyType.IsClass &&
        property.PropertyType != typeof(string) &&
        !property.PropertyType.IsEnum;
}
