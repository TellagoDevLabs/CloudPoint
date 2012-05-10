using System;
using System.Collections.Generic;
namespace SPSM.Common.Configuration
{
    public interface IConfigRepository
    {
        IDictionary<string, string> GetAllConfigFromCategory(string category);
        string GetConfigValue(string category, string configKey);
    }
}
