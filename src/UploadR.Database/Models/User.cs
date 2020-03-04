using System;
using System.Collections.Generic;
using UploadR.Database.Enums;

namespace UploadR.Database.Models
{
    public sealed class User
    {
        /// <summary>
        ///     Id of the user.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     Date time this user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Unique email of this user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Whether this user has a verified account or not.
        /// </summary>
        public bool Verified { get; set; }

        /// <summary>
        ///     Whether this user's account is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        ///     User's API token hash.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///     User's account type.
        /// </summary>
        public AccountType Type { get; set; }

        /// <summary>
        ///     Uploads made by this account.
        /// </summary>
        public ICollection<Upload> Uploads { get; set; }
    }
}
