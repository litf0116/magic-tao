using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
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

    // per-key 信号量，按 phone+purpose 隔离并发，解决 TOCTOU 频率限制竞争
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _sendLocks = new();

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
        var lockKey = $"{phoneNumber}_{purpose}";
        var semaphore = _sendLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            // 以下临界区：频率检查 → 发送短信 → 入库，保持原子性
            var recentCode = await _smsVerificationCodeRepository.GetAll()
                .Where(x => x.PhoneNumber == phoneNumber && x.Purpose == purpose)
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync();

            if (recentCode != null && (DateTime.Now - recentCode.CreationTime).TotalSeconds < 60)
            {
                throw new UserFriendlyException("发送过于频繁，请稍后再试");
            }

            var todayCount = await _smsVerificationCodeRepository.GetAll()
                .Where(x => x.PhoneNumber == phoneNumber && x.CreationTime >= DateTime.Now.Date)
                .CountAsync();

            if (todayCount >= 10)
            {
                throw new UserFriendlyException("今日发送次数已达上限");
            }

            var code = GenerateCode();
            var message = $"您的验证码为 {code}，5分钟内有效，请勿泄露给他人。";

            // 先发送短信，成功后再入库，避免发送失败留下脏数据
            await _smsSender.SendAsync(phoneNumber, message);

            var entity = new SmsVerificationCode(phoneNumber, code, purpose, tenantId);
            await _smsVerificationCodeRepository.InsertAsync(entity);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<bool> VerifyCodeAsync(string phoneNumber, string code, SmsCodePurpose purpose)
    {
        var entity = await _smsVerificationCodeRepository.GetAll()
            .Where(x => x.PhoneNumber == phoneNumber && x.Purpose == purpose)
            .OrderByDescending(x => x.CreationTime)
            .FirstOrDefaultAsync();

        if (entity == null || entity.IsUsed || entity.IsExpired || entity.Code != code)
        {
            return false;
        }

        // 原子性更新：只有 IsUsed == false 的行才会被更新，防止并发双花
        var affected = await _smsVerificationCodeRepository.GetAll()
            .Where(x => x.Id == entity.Id && !x.IsUsed)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.IsUsed, true));

        return affected > 0;
    }

    private static string GenerateCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}