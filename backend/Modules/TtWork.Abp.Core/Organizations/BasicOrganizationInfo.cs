namespace TtWork.Abp.Core.Organizations
{
    public class BasicOrganizationInfo
    {
        public long? OrganizationId { get; set; }
        public string DisplayName { get; set; }

        public BasicOrganizationInfo(long? id, string name)
        {
            OrganizationId = id;
            DisplayName = name;
        }
    }
}