using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Common.Extensions
{
    public static class ExpressionExtension
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            ArgumentNullException.ThrowIfNull(expr1);
            ArgumentNullException.ThrowIfNull(expr2);

            var parameter = Expression.Parameter(typeof(T), "x");
            var left = ReplaceParameter(expr1.Body, expr1.Parameters[0], parameter);
            var right = ReplaceParameter(expr2.Body, expr2.Parameters[0], parameter);
            var body = Expression.AndAlso(left, right);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            ArgumentNullException.ThrowIfNull(expr1);
            ArgumentNullException.ThrowIfNull(expr2);

            var parameter = Expression.Parameter(typeof(T), "x");
            var left = ReplaceParameter(expr1.Body, expr1.Parameters[0], parameter);
            var right = ReplaceParameter(expr2.Body, expr2.Parameters[0], parameter);
            var body = Expression.OrElse(left, right);

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static Expression ReplaceParameter(Expression body, ParameterExpression source, ParameterExpression target)
            => new ReplaceParameterVisitor(source, target).Visit(body)!;

        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _source;
            private readonly ParameterExpression _target;

            public ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
            {
                _source = source;
                _target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _source ? _target : base.VisitParameter(node);
        }
    }
}
