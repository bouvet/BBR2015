//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Data.Entity.Core.Objects;
//using System.Data.Entity.Infrastructure;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace Database
//{
//    //http://www.thomaslevesque.com/2010/10/03/entity-framework-using-include-with-lambda-expressions/
//    public static class DbQueryExtensions
//    {
//        public static DbQuery<T> Include<T>(this DbQuery<T> query, Expression<Func<T, object>> selector)
//        {
//            string propertyName = GetPropertyName(selector);
//            return query.Include(propertyName);
//        }
 
//        private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
//        {
//            MemberExpression memberExpr = expression.Body as MemberExpression;
//            if (memberExpr == null)
//                throw new ArgumentException("Expression body must be a member expression");
//            return memberExpr.Member.Name;
//        }
//        //public static DbQuery<T> Include<T>(this DbQuery<T> query, Expression<Func<T, object>> selector)
//        //{
//        //    string path = new PropertyPathVisitor().GetPropertyPath(selector);
//        //    return query.Include(path);
//        //}

//        //class PropertyPathVisitor : ExpressionVisitor
//        //{
//        //    private Stack<string> _stack;

//        //    public string GetPropertyPath(Expression expression)
//        //    {
//        //        _stack = new Stack<string>();
//        //        Visit(expression);
//        //        return _stack
//        //            .Aggregate(
//        //                new StringBuilder(),
//        //                (sb, name) =>
//        //                    (sb.Length > 0 ? sb.Append(".") : sb).Append(name))
//        //            .ToString();
//        //    }

//        //    protected override Expression VisitMember(MemberExpression expression)
//        //    {
//        //        if (_stack != null)
//        //            _stack.Push(expression.Member.Name);
//        //        return base.VisitMember(expression);
//        //    }

//        //    protected override Expression VisitMethodCall(MethodCallExpression expression)
//        //    {
//        //        if (IsLinqOperator(expression.Method))
//        //        {
//        //            for (int i = 1; i < expression.Arguments.Count; i++)
//        //            {
//        //                Visit(expression.Arguments[i]);
//        //            }
//        //            Visit(expression.Arguments[0]);
//        //            return expression;
//        //        }
//        //        return base.VisitMethodCall(expression);
//        //    }

//        //    private static bool IsLinqOperator(MethodInfo method)
//        //    {
//        //        if (method.DeclaringType != typeof(Queryable) && method.DeclaringType != typeof(Enumerable))
//        //            return false;
//        //        return Attribute.GetCustomAttribute(method, typeof(ExtensionAttribute)) != null;
//        //    }
//        //}
//    }
//}
