using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Compilation;
using System.ComponentModel;
using System.CodeDom;
using System.Web.UI;
using System.Diagnostics;
using SPSM.Common.Logging;
using Microsoft.SharePoint.Administration;

namespace SPSM.Common.Configuration
{
    [ExpressionPrefix("SPConfigStore")]
    public class ConfigStoreExpressionBuilder : ExpressionBuilder
    {
        private static Logger logger = new Logger();

        public static object GetEvalData(string expression, Type target, string entry)
        {
            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStoreExpressionBuilder-GetEvalData(): Entered with expression '{0}'.",
                expression);

            string[] aExpressionParts = expression.Split('|');
            string sCategory = aExpressionParts[0];
            string sTitle = aExpressionParts[1];

            if ((aExpressionParts.Length != 2) || (string.IsNullOrEmpty(sCategory) || string.IsNullOrEmpty(sTitle)))
            {
                throw new InvalidConfigurationException("Token passed to Config Store expression builder was in the wrong format - " +
                    "expressions should be in form Category|Item Title e.g. Search|SearchGoButtonText. Expression was :"+expression);
            }

            string sValue = ConfigStore.GetValue(sCategory, sTitle);

            logger.LogToOperations(SPSMCategories.ConfigStore, EventSeverity.Verbose, "ConfigStoreExpressionBuilder-GetEvalData(): Returning '{0}'.",
                sValue);

            return sValue;
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context)
        {
            return GetEvalData(entry.Expression, target.GetType(), entry.Name);
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
        object parsedData, ExpressionBuilderContext context)
        {
            Type type1 = entry.DeclaringType;
            PropertyDescriptor descriptor1 = TypeDescriptor.GetProperties(type1)[entry.PropertyInfo.Name];
            CodeExpression[] expressionArray1 = new CodeExpression[3];
            expressionArray1[0] = new CodePrimitiveExpression(entry.Expression.Trim());
            expressionArray1[1] = new CodeTypeOfExpression(type1);
            expressionArray1[2] = new CodePrimitiveExpression(entry.Name);
            return new CodeCastExpression(descriptor1.PropertyType, new CodeMethodInvokeExpression(new
           CodeTypeReferenceExpression(base.GetType()), "GetEvalData", expressionArray1));
        }

        public override bool SupportsEvaluate
        {
            get { return true; }
        }

    }
}
