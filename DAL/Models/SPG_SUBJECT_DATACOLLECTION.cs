﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class SPG_SUBJECT_DATACOLLECTION
    {
        public int DATACOLLECT_ID { get; set; }
        public int? SUBJECTAREA_ID { get; set; }
        public string FIELD_NAME { get; set; }
        public string DISPLAY_NAME { get; set; }
        public string IS_LOOKUP { get; set; }
        public string DATA_TYPE { get; set; }
        public string DISPLAY_TYPE { get; set; }
        public string UOM { get; set; }
        public string ACTIVE_FLAG { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime? MODIFIED_DATE { get; set; }
        public string MODIFIED_BY { get; set; }
    }
}