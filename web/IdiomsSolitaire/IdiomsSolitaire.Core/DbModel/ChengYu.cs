using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IdiomsSolitaire.Core
{
    [Table("cy")]
    public class ChengYu
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }


        [Column("spell")]
        public string Spell { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("derivation")]
        public string Derivation { get; set; }

    }
}
