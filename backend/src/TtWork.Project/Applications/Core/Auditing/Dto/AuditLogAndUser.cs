using Abp.Auditing;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Applications.Auditing.Dto {
    public class AuditLogAndUser {
        public AuditLog AuditLog { get; set; }

        public User User { get; set; }
    }
}