using Domain.Entities.Personas;
using Shared.StaticShared.Enumerators;
using System;
using System.Linq.Expressions;
using System.Reflection;


namespace Shared.StaticShared
{
  /// <summary>
  /// Clase estatica contenedora de los metodos para la construccion de expresiones LINQ para la consulta a BD o conjutos queryables.
  /// </summary>
  public static class StaticOperations
  {

    public static Expression<Func<TItem, bool>> QueryableOfComparison<TItem, TValue>(PropertyInfo property, TValue value, TypesOfComparison comparison) 
    {
      if (comparison == TypesOfComparison.Equal) {
        return PropertyEquals<TItem, string>(property, value.ToString()?? "");
      }
      if (comparison == TypesOfComparison.NotEqual) { throw new NotImplementedException(); }
      if (comparison == TypesOfComparison.Contain && typeof(TValue)== typeof(string))
      {
        return PropertyContains<TItem>(property, value.ToString());
      }
      if (comparison == TypesOfComparison.StartsWith) { throw new NotImplementedException(); }
      if (comparison == TypesOfComparison.EndsWith) { throw new NotImplementedException(); }
      if (comparison == TypesOfComparison.GreaterThan) { throw new NotImplementedException(); }
      if (comparison == TypesOfComparison.GreaterThanOrEqual) { throw new NotImplementedException(); }
      if (comparison == TypesOfComparison.LessThan) { throw new NotImplementedException(); }
      if (comparison == TypesOfComparison.LessThanOrEqual) { throw new NotImplementedException(); }
      throw new NotImplementedException();
    }


    /// <summary>
    /// Metodo dado el property info de la columna de la tabla y un valor (del tipo indicado en el parametro Generic) arroja el expresion para ser utilizada
    /// en la query de consulta a BD, consultando los registros que tienen el value indicado en la columna indicada como parametro.
    /// </summary>
    /// <typeparam name="TItem">Nombre de la tabla/ conjunto queryable. </typeparam>
    /// <param name="property">PropertyInfo de la columna a evaluar.</param>
    /// <param name="value">Valor de la Columna.</param>
    /// <returns>Expression LINQ.</returns>
    public static Expression<Func<TItem, bool>> PropertyEquals<TItem, TValue>(PropertyInfo property, TValue value)
    {
      var param = Expression.Parameter(typeof(TItem));
      var body = Expression.Equal(Expression.Property(param, property), Expression.Constant(value));
      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }

    /// <summary>
    ///  Metodo que dado el property info de la columna de la tabla y un valor (del tipo indicado en el parametro Generic) retorna la expresion para ser utilizada
    /// en la query de consulta a BD. Consultando los registros que tienen un valor mayor al value indicado como parametro.
    /// </summary>
    /// <typeparam name="TItem">Tipo Queryable. </typeparam>
    /// <typeparam name="TValue">Tipo de datos del valor a filtrar.</typeparam>
    /// <param name="property"> Property del conjunto queryable. </param>
    /// <param name="value"> Valor a comparar.</param>
    /// <returns>Expresion de consulta. </returns>
    public static Expression<Func<TItem, bool>> PropertyGreaterThan<TItem, TValue>(PropertyInfo property, TValue value)
    {
      var param = Expression.Parameter(typeof(TItem));
      var col = Expression.MakeMemberAccess(param, property);
      var valueConst = Expression.Constant(value, typeof(TValue));
      var body = MyGreaterThan(col, valueConst);
      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }

    /// <summary>
    ///  Metodo que dado el property info de la columna de la tabla y un valor (del tipo indicado en el parametro Generic) retorna la expresion para ser utilizada
    /// en la query de consulta a BD. Consultando los registros que tienen un valor mayor O IGUAL al value indicado como parametro.
    /// </summary>
    /// <typeparam name="TItem">Tipo Queryable. </typeparam>
    /// <typeparam name="TValue">Tipo de datos del valor a filtrar.</typeparam>
    /// <param name="property"> Property del conjunto queryable. </param>
    /// <param name="value"> Valor a comparar.</param>
    /// <returns>Expresion de consulta. </returns>
    public static Expression<Func<TItem, bool>> PropertyGreaterThanOrEqual<TItem, TValue>(PropertyInfo property, TValue value)
    {
      var param = Expression.Parameter(typeof(TItem));
      var col = Expression.MakeMemberAccess(param, property);
      var valueConst = Expression.Constant(value, typeof(TValue));
      var body = MyGreaterThanOrEqual(col, valueConst);
      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }

    /// <summary>
    ///  Metodo que dado el property info de la columna de la tabla y un valor (del tipo indicado en el parametro Generic) retorna la expresion para ser utilizada
    /// en la query de consulta a BD. Consultando los registros que tienen un valor MENOR O IGUAL al value indicado como parametro.
    /// </summary>
    /// <typeparam name="TItem">Tipo Queryable. </typeparam>
    /// <typeparam name="TValue">Tipo de datos del valor a filtrar.</typeparam>
    /// <param name="property"> Property del conjunto queryable. </param>
    /// <param name="value"> Valor a comparar.</param>
    /// <returns>Expresion de consulta. </returns>
    public static Expression<Func<TItem, bool>> PropertyLessThanOrEqual<TItem, TValue>(PropertyInfo property, TValue value)
    {
      var param = Expression.Parameter(typeof(TItem));
      var col = Expression.MakeMemberAccess(param, property);
      var valueConst = Expression.Constant(value, typeof(TValue));
      var body = MyLessThanOrEqual(col, valueConst);
      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }

    /// <summary>
    ///  Metodo que dado el property info de la columna de la tabla y un valor (del tipo indicado en el parametro Generic) retorna la expresion para ser utilizada
    /// en la query de consulta a BD. Consultando los registros que tienen un valor MENOR al value indicado como parametro.
    /// </summary>
    /// <typeparam name="TItem">Tipo Queryable. </typeparam>
    /// <typeparam name="TValue">Tipo de datos del valor a filtrar.</typeparam>
    /// <param name="property"> Property del conjunto queryable. </param>
    /// <param name="value"> Valor a comparar.</param>
    /// <returns>Expresion de consulta. </returns>
    public static Expression<Func<TItem, bool>> PropertyLessThan<TItem, TValue>(PropertyInfo property, TValue value)
    {
      var param = Expression.Parameter(typeof(TItem));
      var col = Expression.MakeMemberAccess(param, property);
      var valueConst = Expression.Constant(value, typeof(TValue));
      var body = MyLessThan(col, valueConst);
      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }

    /// <summary>
    /// Metodo dado el property info de la columna de la tabla y un valor arroja el expresion para ser utilizado
    /// en la query de consulta a BD, consultando los registros que tienen el value indicado al inicio de los string en la columna indicada.
    /// </summary>
    /// <typeparam name="TItem">Nombre de la tabla/ conjunto queryable. </typeparam>
    /// <param name="propertyInfo">PropertyInfo de la columna a evaluar.</param>
    /// <param name="value">Valor de la Columna.</param>
    /// <returns>Expression LINQ.</returns>
    public static Expression<Func<TItem, bool>> PropertyStartsWith<TItem>(PropertyInfo propertyInfo, string value)
    {
      var param = Expression.Parameter(typeof(TItem));

      var m = Expression.MakeMemberAccess(param, propertyInfo);
      var c = Expression.Constant(value, typeof(string));
      var mi = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
      var body = Expression.Call(m, mi, c);

      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }

    /// <summary>
    /// Metodo dado el property info de la columna de la tabla y un valor arroja el expresion para ser utilizado
    /// en la query de consulta a BD, consultando los registros que tienen el value indicado al final de los string en la columna indicada.
    /// </summary>
    /// <typeparam name="TItem">Nombre de la tabla/ conjunto queryable. </typeparam>
    /// <param name="propertyInfo">PropertyInfo de la columna a evaluar.</param>
    /// <param name="value">Valor de la Columna.</param>
    /// <returns>Expression LINQ.</returns>
    public static Expression<Func<TItem, bool>> PropertyEndsWith<TItem>(PropertyInfo propertyInfo, string value)
    {
      var param = Expression.Parameter(typeof(TItem));

      var m = Expression.MakeMemberAccess(param, propertyInfo);
      var c = Expression.Constant(value, typeof(string));
      var mi = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
      var body = Expression.Call(m, mi, c);

      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }

    /// <summary>
    /// Metodo dado el property info de la columna de la tabla y un valor arroja el expresion para ser utilizado 
    /// en la query de consulta a BD, consultando los registros que tienen el value indicado contenido en la columna indicada
    /// </summary>
    /// <typeparam name="TItem">Nombre de la tabla/ conjunto queryable. </typeparam>
    /// <param name="propertyInfo">PropertyInfo de la columna a evaluar.</param>
    /// <param name="value">Valor de la Columna.</param>
    /// <returns>Expression LINQ.</returns>
    public static Expression<Func<TItem, bool>> PropertyContains<TItem>(PropertyInfo propertyInfo, string value)
    {
      var param = Expression.Parameter(typeof(TItem));

      var m = Expression.MakeMemberAccess(param, propertyInfo);
      var c = Expression.Constant(value, typeof(string));
      var mi = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
      var body = Expression.Call(m, mi, c);

      return Expression.Lambda<Func<TItem, bool>>(body, param);
    }


    public static Expression<Func<TItem, bool>> PropertySetContains<TItem, TPropItem>(IEnumerable<string> set, PropertyInfo property)
    {
      var parameter = Expression.Parameter(typeof(TItem), "p");
      var prop = Expression.Property(parameter, property);

      var containsExpression = Expression.Call(
            typeof(Enumerable),
            "Contains",
            new[] { typeof(TItem) },
            Expression.Constant(set),
            prop
        );
      var lambdaExpression = Expression.Lambda<Func<TItem, bool>>(containsExpression, parameter);
      return lambdaExpression;
    }

    private static Expression MyGreaterThan(Expression e1, Expression e2)
    {
      if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
      {
        e2 = Expression.Convert(e2, e1.Type);
      }
      else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
      {
        e1 = Expression.Convert(e1, e2.Type);
      }

      return Expression.GreaterThan(e1, e2);
    }

    private static Expression MyGreaterThanOrEqual(Expression e1, Expression e2)
    {
      if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
      {
        e2 = Expression.Convert(e2, e1.Type);
      }
      else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
      {
        e1 = Expression.Convert(e1, e2.Type);
      }

      return Expression.GreaterThanOrEqual(e1, e2);
    }

    private static Expression MyLessThan(Expression e1, Expression e2)
    {
      if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
      {
        e2 = Expression.Convert(e2, e1.Type);
      }
      else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
      {
        e1 = Expression.Convert(e1, e2.Type);
      }

      return Expression.LessThan(e1, e2);
    }

    private static Expression MyLessThanOrEqual(Expression e1, Expression e2)
    {
      if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
      {
        e2 = Expression.Convert(e2, e1.Type);
      }
      else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
      {
        e1 = Expression.Convert(e1, e2.Type);
      }

      return Expression.LessThanOrEqual(e1, e2);
    }

    private static bool IsNullableType(Type t)
    {
      return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static bool IsNumber(string Text)
    {
      long n;
      return long.TryParse(Text, out n);
    }
  }
}
