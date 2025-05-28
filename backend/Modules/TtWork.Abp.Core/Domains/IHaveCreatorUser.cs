using TtWork.Abp.Applications.Dtos;

namespace TtWork.Abp.Domains;

public interface IHaveCreatorUser {
    UserDtoBase CreatorUser { get; set; }
    public long? CreatorUserId { get; set; }
}

public interface IHaveUser {
    UserDtoBase User { get; set; }
    public long UserId { get; set; }
}