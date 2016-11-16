using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfChatSample.Server.DB
{
    [Table("Users")]
    internal class UserEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "integer")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Username", TypeName = "nvarchar")]
        public string Username { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Password")]
        public byte[] Password { get; set; }

        [Column("IsAdmin", TypeName = "bit")]
        public bool IsAdmin { get; set; }
    }
}
