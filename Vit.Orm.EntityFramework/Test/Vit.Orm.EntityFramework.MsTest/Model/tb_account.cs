using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vit.Orm.EntityFramework.MsTest.Model
{  
    [Table("tb_account")]
    public class tb_account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string account { get; set; }
        public string pwd { get; set; }

        public string name { get; set; }
    }
}
