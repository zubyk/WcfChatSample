using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WcfChatSample.Service;

namespace WcfChatSample.Server.DB
{
    [Table("Messages")]
    internal class MessageEntity : IDbMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "integer")]
        public int Id { get; set; }

        [Required]
        [Column("Date", TypeName = "datetime")]
        public DateTime Date { get; set; }

        [MaxLength(50)]
        [Column("Username", TypeName = "nvarchar")]
        public string Username { get; set; }

        [Column("Text", TypeName = "nvarchar")]
        public string Text { get; set; }
    }
}
