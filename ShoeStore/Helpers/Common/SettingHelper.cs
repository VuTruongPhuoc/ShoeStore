using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShoeStore.Models;
using ShoeStore.Data;

namespace ShoeStore.Common
{
    public class SettingHelper
    {
        private static ShoeStoreContext db = new ShoeStoreContext();

        //public static string GetValue(string key)
        //{
        //    var item = db.SingleOrDefault(x => x.SettingKey == key);
        //    if (item != null)
        //    {
        //        return item.SettingValue;
        //    }
        //    return "";
        //}
    }
}