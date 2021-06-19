using Dapper.Contrib.Extensions;

namespace Vit.Orm.Dapper.MsTest.Model
{
    [Table("tb_account")]
    public class Auth_Account
    {
        [Key]
        public int id { get; set; }
        public string account { get; set; }
        public string pwd { get; set; }

        public string name { get; set; }
    }   

}
