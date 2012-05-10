using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;
using SPSM.Common.Data;

namespace SPSM.Common.Configuration
{
    public class ConfigRepository : IConfigRepository
    {
        private string webFullUrl;
        private const string configList = "SPSM-ConfigStore";
        public ConfigRepository(string webFullUrl)
        {
            this.webFullUrl = webFullUrl;
        }

        public IDictionary<string, string> GetAllConfigFromCategory(string category)
        {
            using (ClientContext ctx = new ClientContext(webFullUrl))
            {
                Web web = ctx.Web;

                List list = web.Lists.GetByTitle(configList);

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = String.Format(
                    @"<View>
                    <Query>
                        <Where>
                            <Eq>
                                <FieldRef Name='ConfigCategory'/>
                                <Value Type='Text'>{0}</Value>
                            </Eq>                                
                        </Where>
                    </Query>
                    <RowLimit>50</RowLimit>
                    </View>"
                    , category);
                ListItemCollection listItems = list.GetItems(camlQuery);
                ctx.Load(
                        listItems,
                        items => items
                            .Include(
                                item => item["Title"],
                                item => item["ConfigValue"]));

                ctx.ExecuteQuery();

                var configValues = new Dictionary<string, string>(listItems.Count);
                foreach (ListItem item in listItems)
                {
                    configValues.Add(DataHelper.GetValue(item["Title"]), DataHelper.GetValue(item["ConfigValue"]));
                }

                return configValues;
            }
        }

        public string GetConfigValue(string category, string configKey)
        {
            using (ClientContext ctx = new ClientContext(webFullUrl))
            {
                Web web = ctx.Web;

                List list = web.Lists.GetByTitle(configList);

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = String.Format(
                    @"<View>
                    <Query>
                        <Where>
                            <And>
                                <Eq>
                                    <FieldRef Name='ConfigCategory'/>
                                    <Value Type='Text'>{0}</Value>
                                </Eq> 
                                <Eq><FieldRef Name='Title' />
                                    <Value Type='Text'>{1}</Value></Eq>
                            </And>                               
                        </Where>
                    </Query>
                    <RowLimit>1</RowLimit>
                    </View>"
                    , category, configKey);
                ListItemCollection listItems = list.GetItems(camlQuery);
                ctx.Load(
                        listItems,
                        items => items
                            .Include(
                                item => item["ConfigValue"]));
                
                ctx.ExecuteQuery();
                
                var configValues = new Dictionary<string, string>(listItems.Count);
                if (listItems.Count > 0)
                {
                    var item = listItems[0];
                    return item["ConfigValue"].ToString();
                }
            }
            throw new InvalidConfigurationException(String.Format("Config Value not found with category '{0}' and key '{1}' in list '{2}' and web '{3}'", category, configKey, configList, webFullUrl));
        }
    }
}
