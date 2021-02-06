using System;

namespace Bissues.Models
{
    /// <summary>
    /// Notification class will be autogenerated notifications to alert a AppUser that the Bissue belonging to them has been modified.
    /// </summary>
    public class Notification : BaseEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Is the message unread
        /// </summary>
        /// <value>Is the message unread</value>
        public bool IsUnread { get; set; } = true;

        //FK User and Bissue
        // /* FK AppUser the Notification belongs to */
        // /// <summary>
        // /// AppUser that created the bissue that was modified
        // /// </summary>
        // /// <value>AppUser that created the bissue that was modified</value>
        // public string AppUserId { get; set; }
        // public virtual AppUser Owner { get; set; }
        /* FK Bissue the Notification belongs to */
        /// <summary>
        /// The Bissue that was modified
        /// </summary>
        /// <value>The Bissue that was modified</value>
        public int BissueId { get; set; }
        public virtual Bissue Bissue { get; set; }
    }
}