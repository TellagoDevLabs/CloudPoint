using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPSM.Common.Logging
{
    public enum Areas
    {
        SPSM
    }
    public enum SPSMCategories
    {
        Default,
        BaseWorkflow,
        FeaturedBlogs,
        NewsFeed,
        Media,
        ConfigStore,
        Search,
        Emailing,
        UserProfileService,
        MasterPage,
        HowDoI
    }
    public static class SPSMCategoriesExtensions
    {
        public static string ToLoggerString(this SPSMCategories category)
        {
            return String.Format("{0}/{1}", Areas.SPSM.ToString(), category.ToString());
        }
    }
}
