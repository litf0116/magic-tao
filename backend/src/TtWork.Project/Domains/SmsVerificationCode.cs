using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

public class SmsVerificationCode : AuditedEntity<long>
{
    public SmsVerificationCode()
    {
    }

    public SmsVerificationCode(string phoneNumber, string code, SmsCodePurpose purpose, int tenantId)
    {
        PhoneNumber = phoneNumber;
        Code = code;
        Purpose = purpose;
        TenantId = tenantId;
        IsUsed = false;
        ExpireTime = DateTime.Now.AddMinutes(5);
    }

    [StringLength(20)]
    public string PhoneNumber { get; set; }

    [StringLength(6)]
    public string Code { get; set; }

    public SmsCodePurpose Purpose { get; set; }

    public bool IsUsed { get; set; }

    public DateTime ExpireTime { get; set; }

    public int TenantId { get; set; }

    public bool IsExpired => DateTime.Now > ExpireTime;

    public bool IsValid(string phoneNumber, string code, SmsCodePurpose purpose)
    {
        return !IsUsed && !IsExpired &&
               PhoneNumber == phoneNumber &&
               Code == code &&
               Purpose == purpose;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
    }
}

public enum SmsCodePurpose
{
    Login = 1,
    BindPhone = 2,
    ResetPassword = 3
}