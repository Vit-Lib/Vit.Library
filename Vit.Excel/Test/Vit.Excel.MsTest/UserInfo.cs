using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vit.Core.Module.Serialization;
using Vit.Core.Util.Common;
using Vit.Extensions.Json_Extensions;

namespace Vit.Excel.MsTest
{
    public class UserInfo
    {
        public int? id { get; set; }
        public string name { get; set; }
        public int? age { get; set; }
        public DateTime? birth { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is UserInfo user && id == user.id && name == user.name && age == user.age && birth == user.birth) 
            {
                return true;
            }
            return false;
        }

        public static bool AreEqual(object[] values1, object[] values2) 
        {
            if (values1 == null && values2 == null) return true;
            if(values1?.Length != values2?.Length) return false;
            for(var i = 0; i < values1.Length; i++)
            {
                if (!AreEqualByUnderlying(values1[i], values2[i]))
                    return false;
            }
            return true;
        }

        public static bool AreEqual(IDictionary<string, object> values1, IDictionary<string, object> values2)
        {
            if (values1 == null && values2 == null) return true;
            if (values1?.Keys.Count != values2?.Keys.Count) return false;
            foreach (var kv in values1)
            {
                if (!AreEqualByUnderlying(kv.Value, values2[kv.Key]))
                    return false;
            }
            return true;
        }


        public static bool AreEqualByUnderlying(object  value1, object value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            
             return GetUnderlyingValue(value1).Equals(GetUnderlyingValue(value2));
        }

        public static object GetUnderlyingValue(object value)
        {
            var type= value.GetType()?.GetUnderlyingTypeIfNullable();
            if (type.IsNumericType())
                return Convert.ChangeType(value, typeof(decimal));
            if (Type.GetTypeCode(type) == TypeCode.DateTime)
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            return value;
        }




        public static List<UserInfo> GenerateList(int count)
        {
            return Enumerable.Range(0, count).Select(i => new UserInfo
            {
                id = i,
                name = CommonHelp.NewGuid(),
                age = CommonHelp.Random(0, 100),
                birth = Json.Deserialize<DateTime>(Json.Serialize(DateTime.Now.AddSeconds(CommonHelp.Random(-10000, 10000))))
            }).ToList();
        }



    }
}
