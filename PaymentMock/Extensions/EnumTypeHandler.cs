using System.Data;
using Dapper;

namespace PaymentMock.Extensions;

public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value.ToString();
    }

    public override T Parse(object value)
    {
        if (value is string s && Enum.TryParse<T>(s, true, out var result))
            return result;
        if (value is T enumValue)
            return enumValue;
        throw new InvalidOperationException($"Cannot convert {value} to {typeof(T).Name}");
    }
}
