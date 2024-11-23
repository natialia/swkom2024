using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dms_dal_new.Entities
{
    public class DocumentItem
    {
        // Parameterless constructor
        public DocumentItem() { }

        // Constructor with 'id'
        public DocumentItem(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public string? FileType { get; set; }
        public string? FileSize { get; set; }
        public string? OcrText { get; set; }
    }
}
