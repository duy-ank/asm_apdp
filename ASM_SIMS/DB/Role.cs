using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASM_SIMS.DB
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Column("Name", TypeName = "Varchar(50)"), Required]
        public string Name { get; set; }
    }
}
