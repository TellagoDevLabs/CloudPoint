using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;
using System.Threading;

namespace SPSM.Common.Data
{
    public static class DataHelper
    {
        public static string GetValue(object value)
        {
            if (value != null)
            {
                return value.ToString();
            }

            return string.Empty;
        }

        public static string GetValueAsText(this SPField field, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            object fieldValue = field.GetFieldValue(value.ToString());

            var urlValue = fieldValue as SPFieldUrlValue;
            if (urlValue != null)
            {
                return urlValue.Url;
            }

            var imageFieldValue = fieldValue as ImageFieldValue;
            if (imageFieldValue != null)
            {
                return imageFieldValue.ImageUrl;
            }

            if (value is double)
            {
                return SPFieldNumber.GetFieldValueAsText((double)value, Thread.CurrentThread.CurrentUICulture, false, SPNumberFormatTypes.Automatic);
            }

            return field.GetFieldValueAsText(value);
        }

        public static DateTime? GetValueAsDateTime(object oValue)
        {
            DateTime? value = null;

            if (oValue != null)
                value = Convert.ToDateTime(oValue);

            return value;
        }

        public static bool GetValueAsBoolean(object oValue)
        {
            bool value = false;

            if (oValue != null)
                value = Convert.ToBoolean(oValue.ToString());

            return value;
        }

        public static int GetValueAsInteger(object oValue)
        {
            int value = 0;

            if (oValue != null)
                value = Convert.ToInt32(oValue.ToString());

            return value;
        }

        public static Guid GetValueAsGuid(object oValue)
        {
            Guid value = Guid.Empty;

            if (oValue != null)
                value = new Guid(oValue.ToString());

            return value;
        }

        public static string GetValueFromHyperLink(object oValue, bool getHyperlinkDescription)
        {
            string value = string.Empty;

            if (oValue != null)
            {
                SPFieldUrlValue urlValue = new SPFieldUrlValue(oValue.ToString());
                if (!getHyperlinkDescription)
                    value = urlValue.Url;
                else
                    value = urlValue.Description;
            }

            return value;
        }

        public static string GetValueFromLookup(object oValue, bool getLookupDescription)
        {
            string value = string.Empty;

            if (oValue != null)
            {
                SPFieldLookupValue lookupValue = new SPFieldLookupValue(oValue.ToString());
                if (getLookupDescription)
                    value = lookupValue.LookupValue;
                else
                    value = lookupValue.LookupId.ToString();
            }

            return value;
        }

        public static IList<string> GetValueFromLookupMulti(object oValue)
        {
            IList<string> value = new List<string>();

            if (oValue != null)
            {
                SPFieldLookupValueCollection lookupValue = new SPFieldLookupValueCollection(oValue.ToString());
                if (lookupValue.Count > 0)
                {
                    foreach (SPFieldLookupValue val in lookupValue)
                    {
                        value.Add(val.LookupValue);
                    }
                }
            }

            return value;
        }

        public static string GetValueFromImageField(object oValue)
        {
            string value = null;

            if (oValue != null)
            {
                ImageFieldValue img = oValue as ImageFieldValue;
                value = img.ImageUrl;
            }

            return value;
        }
    }
}
