using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column("Password")]
        public byte[] Password { get; set; }

        [Column("IsAdmin", TypeName = "bit")]
        public bool IsAdmin { get; set; }
    }
}
