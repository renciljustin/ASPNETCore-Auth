using System;

namespace API.Core.Models
{
    public class ModelBase
    {
        public DateTime? CreationTime { get; set; }
        public DateTime? LastModified { get; set; }
        public bool? Flag { get; set; }
    }
}