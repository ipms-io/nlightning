using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NLightning.Infrastructure.Repositories.Database.Helpers;

using Persistence.Contexts;

public static class PrimaryKeyHelper
{
    public static Expression<Func<TEntity,bool>>? GetPrimaryKeyExpression<TEntity>(object id, NLightningDbContext context)
        where TEntity : class
    {
        var keyProperties = context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties
            ?? throw new InvalidOperationException("Entity does not have a primary key defined.");
        
        object[] keyValuesToUse;
        
        if (keyProperties.Count > 1) // We're dealing with composite keys
        {
            if (id is not ITuple idTuple)
                throw new ArgumentException($"The provided id must be a tuple with {keyProperties.Count} items " +
                                            $"for entity {typeof(TEntity).Name}.", nameof(id));
                
            if (idTuple.Length != keyProperties.Count)
                throw new ArgumentException($"The number of items in the provided tuple ({idTuple.Length}) does not" +
                                            $" match the number of primary key properties ({keyProperties.Count}) " +
                                            $"for entity {typeof(TEntity).Name}.", nameof(id));

            keyValuesToUse = new object[keyProperties.Count];
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var value = idTuple[i];

                keyValuesToUse[i] = value ?? throw new ArgumentNullException(
                    nameof(id), $"Item {i} in the provided tuple cannot be null.");
            }
        }
        else // We're dealing with a single key
        {
            if (id is ITuple)
                throw new ArgumentException($"The provided id must not be a tuple for entity {typeof(TEntity).Name}.",
                                            nameof(id));
            
            keyValuesToUse =
            [
                id ?? throw new ArgumentNullException(nameof(id), "The provided id cannot be null.")
            ];
        }
        
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression? predicateBody = null;

        for (var i = 0; i < keyProperties.Count; i++)
        {
            var keyProperty = keyProperties[i];
            var keyValue = keyValuesToUse[i];
            object? correctlyTypedKeyValue;

            var propertyClrType = keyProperty.ClrType;
            if (keyValue.GetType() != propertyClrType)
            {
                try
                {
                    var underlyingType = Nullable.GetUnderlyingType(propertyClrType);
                    correctlyTypedKeyValue = Convert.ChangeType(keyValue, underlyingType ?? propertyClrType);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        $"Key value '{keyValue}' (type: {keyValue.GetType().Name}) for property '{keyProperty.Name}' " +
                        $"could not be converted to the expected type '{propertyClrType.Name}'.", ex);
                }
            }
            else
            {
                correctlyTypedKeyValue = keyValue;
            }
            
            var memberAccess = Expression.Property(parameter, keyProperty.Name);
            var constantValue = Expression.Constant(correctlyTypedKeyValue, propertyClrType);
            var equality = Expression.Equal(memberAccess, constantValue);

            predicateBody = predicateBody == null ? equality : Expression.AndAlso(predicateBody, equality);
        }

        // This should not be reached if keyProperties exist and keyValuesToUse is populated.
        return predicateBody == null ? null : Expression.Lambda<Func<TEntity, bool>>(predicateBody, parameter);
    }
}