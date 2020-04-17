using System;

namespace UploadR.Database.Models
{
    public class EntityBase
    {
        /// <summary>
        ///     Id of the upload.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     Id of the author's entity.
        /// </summary>
        public Guid AuthorGuid { get; set; }
        
        /// <summary>
        ///     Author of the entity.
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        ///     Date time this entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Last date time this entity was queried.
        /// </summary>
        public DateTime LastSeen { get; set; }
        
        /// <summary>
        ///     Gets the amount of time before this entity expires.
        ///     After it expires, it will be marked as removed.
        /// </summary>
        /// <remarks>
        ///     <see cref="TimeSpan.Zero"/> means the entity doesn't expire.
        /// </remarks>
        public TimeSpan ExpiryTime { get; set; }
        
        /// <summary>
        ///     Whether this entity is considered removed or not.
        /// </summary>
        public bool Removed { get; set; }
        
        /// <summary>
        ///     Password of the entity, if set.
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        ///     Amount of time this entity was seen.
        /// </summary>
        public long SeenCount { get; set; }
    }
}