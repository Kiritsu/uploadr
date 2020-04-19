using System.Collections.Generic;

namespace UploadR.Models
{
    public class UploadBulkDeleteModel
    {
        /// <summary>
        ///     List of failed deletions.
        /// </summary>
        public List<string> FailedDeletes { get; set; }
        
        /// <summary>
        ///     List of succeeded deletions.
        /// </summary>
        public List<string> SucceededDeletes { get; set; }
    }
}