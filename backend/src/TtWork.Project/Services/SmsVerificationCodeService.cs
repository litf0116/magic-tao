using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp.Core.Net.Sms;
using TtWork.Project.Domains;

namespace TtWork.Project.Services;

public class SmsVerificationCodeService : ISmsVerificationCodeService
{
    private readonly IRepository<SmsVerificationCode, long> _smsVerificationCodeRepository;
    private readonly IAbpSession _abpSession;
    private readonly ISmsSender _smsSender;

    public SmsVerificationCodeService(
        IRepository<SmsVerificationCode, long> smsVerificationCodeRepository,
        IAbpSession abpSession,
        ISmsSender smsSender)
    {
        _smsVerificationCodeRepository = smsVerificationCodeRepository;
        _abpSession = abpSession;
        _smsSender = smsSender;
    }

    public async Task SendCodeAsync(string phoneNumber, SmsCodePurpose purpose)
    {
        var tenantId = _abpSession.TenantId ?? 1;

        var recentCode = await _smsVerificationCodeRepository.GetAll()
            .Where(x => x.PhoneNumber == phoneNumber && x.Purpose == purpose)
            .OrderByDescending(x => x.CreationTime)
            .FirstOrDefaultAsync();

        if (recentCode != null && (DateTime.Now - recentCode.CreationTime).TotalSeconds < 60)
        {
            throw new UserFriendlyException("发送过于频繁，请稍后再试");
        }

        var todayCount = await _smsVerificationCodeRepository.GetAll()
            .Where(x => x.PhoneNumber == phoneNumber && x.CreationTime >= DateTime.Today)
            .CountAsync();

        if (todayCount >= 10)
        {
            throw new UserFriendlyException("今日发送次数已达上限");
        }

        var code = GenerateCode();

        var entity = new SmsVerificationCode(phoneNumber, code, purpose, tenantId);
        await _smsVerificationCodeRepository.InsertAsync(entity);

        var message = $"您的验证码为 {code}，5分钟内有效，请勿泄露给他人。";
        await _smsSender.SendAsync(phoneNumber, message);
    }

    public async Task<bool> VerifyCodeAsync(string phoneNumber, string code, SmsCodePurpose purpose)
    {
        var entity = await _smsVerificationCodeRepository.GetAll()
            .Where(x => x.PhoneNumber == phoneNumber && x.Purpose == purpose)
            .OrderByDescending(x => x.CreationTime)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return false;
        }

        if (entity.IsUsed)
        {
            return false;
        }

        if (entity.IsExpired)
        {
            return false;
        }

        if (entity.Code != code)
        {
            return false;
        }

        entity.MarkAsUsed();
        await _smsVerificationCodeRepository.UpdateAsync(entity);

        return true;
    }

    private static string GenerateCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}