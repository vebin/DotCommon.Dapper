﻿using System;
using System.Linq.Expressions;
using DotCommon.Dapper.Expressions.Sections;

namespace DotCommon.Dapper.Expressions.Translators
{
    public class SqlServerJoinTranslator: SqlServerQueryTranslator
    {
        private readonly JoinSectionParameter _parameter;
        public SqlServerJoinTranslator(TranslatorDelegate translatorDelegate, ISectionParameter parameter)
            : base(translatorDelegate)
        {
            _parameter = (JoinSectionParameter) parameter;
        }

        public override string Translate(LambdaExpression expr)
        {
            Visit(expr.Body);
            return SqlBuilder.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var join = " INNER JOIN";
            if (_parameter.JoinType == JoinType.LeftJoin)
            {
                @join = " LEFT JOIN";
            }
            else if (_parameter.JoinType == JoinType.RightJoin)
            {
                @join = " RIGHT JOIN";
            }
            string left = "", right = "";
            if (node.Left is MemberExpression)
            {
                var memberExprL = node.Left as MemberExpression;
                var typeAlias = TranslatorDelegate.GetTypeAlias(memberExprL.Member.DeclaringType);
                SqlBuilder.Append(
                    $" [{TranslatorDelegate.GetTableName(memberExprL.Member.DeclaringType)}] {typeAlias}");
                left =
                    $" [{typeAlias}].[{TranslatorDelegate.GetMemberMap(memberExprL.Member)}]";
            }
            SqlBuilder.Append($"{join}");

            if (node.Right is MemberExpression)
            {
                var memberExprR = node.Right as MemberExpression;
                var typeAlias = TranslatorDelegate.GetTypeAlias(memberExprR.Member.DeclaringType);
                SqlBuilder.Append(
                    $" [{TranslatorDelegate.GetTableName(memberExprR.Member.DeclaringType)}] {typeAlias}");
                right = $" [{typeAlias}].[{TranslatorDelegate.GetMemberMap(memberExprR.Member)}]";
            }
            if (node.NodeType != ExpressionType.Equal)
            {
                throw new ArgumentException("Join param invalid!");
            }

            SqlBuilder.Append($" ON{left} ={right}");
            return node;
        }
    }
}
