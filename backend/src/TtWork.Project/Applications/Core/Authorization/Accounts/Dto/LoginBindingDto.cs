using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace TtWork.Project.Applications.Core.Authorization.Accounts.Dto;

public class LoginBindingDto : EntityDto<long>
{
    public string LoginProvider { get; set; }

    public string ProviderKey { get; set; }

    public string DisplayName { get; set; }

    public string Icon { get; set; }

    public bool IsBound { get; set; }

    public DateTime? BoundTime { get; set; }
}

public class LoginBindingListOutput
{
    public List<LoginBindingDto> Items { get; set; } = new();
}

public class SendSmsCodeOutput
{
    public bool Success { get; set; }

    public string Message { get; set; }

    public int ExpireInSeconds { get; set; }
}

public class UnbindLoginInput
{
    public string LoginProvider { get; set; }
}