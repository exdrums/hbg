using API.Identity.AuditLogging.Constants;

namespace API.Identity.AuditLogging.Configuration
{
    public class AuditHttpSubjectOptions
    {
        public string SubjectIdentifierClaim { get; set; } = ClaimsConsts.Sub;

        public string SubjectNameClaim { get; set; } = ClaimsConsts.Name;
    }
}