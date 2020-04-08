using System.Collections.Generic;

namespace UploadR.Models
{
    public class UploadBulkDeleteModel
    {
        public List<string> FailedDeletes { get; set; }
        public List<string> SucceededDeletes { get; set; }
    }
}