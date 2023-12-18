using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Models;

namespace DAL.Models
{
    [MetadataType(typeof(SPG_SUBJECT_MASTER_Metadata))]
    public partial class SPG_SUBJECT_MASTER
    {
    }
    public class SPG_SUBJECT_MASTER_Metadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SUBJECT_MASTER_ID { get; set; }
        public string DIMENSION_TABLE { get; set; }
        public string MASTER_NAME { get; set; }
        public string DISPLAY_NAME { get; set; }
        public string FIELD_NAME { get; set; }
        public int? SUBJECTAREA_ID { get; set; }
        public string IS_SHOW { get; set; }
        public string IS_KEY { get; set; }

    }
}
