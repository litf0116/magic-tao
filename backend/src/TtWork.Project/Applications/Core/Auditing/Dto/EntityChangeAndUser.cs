using Abp.EntityHistory;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Applications.Auditing.Dto {
    /// <summary>
    /// A helper class to store an <see cref="EntityChange"/> and a <see cref="User"/> object.
    /// </summary>
    public class EntityChangeAndUser {
        public EntityChange EntityChange { get; set; }

        public User User { get; set; }
    }
}