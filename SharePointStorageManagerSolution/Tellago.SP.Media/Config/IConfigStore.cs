using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellago.SP.Media.Config
{
    public interface IConfigStore
    {
        string GetValue(string category, string key);
    }


}
