﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public partial class SP_GETUPLOADEMAILResult
    {
        public int TemplateID { get; set; }
        public string FROMTIMECODE { get; set; }
        public string ONTIMECODE { get; set; }
        public string SubjectArea { get; set; }
        public int? SubjectAreaID { get; set; }
        public DateTime? GeneratedDate { get; set; }
        public string LockDate { get; set; }
        public string DATA_COLLECTION { get; set; }
        public int CONFIG_ID { get; set; }
        public string FILE_NAME { get; set; }
    }
}
