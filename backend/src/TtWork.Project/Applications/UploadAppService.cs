using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp;
using TtWork.Abp.Core;
using TtWork.Abp.Core.Oss;
using TtWork.Abp.Oss.UpYun;

namespace TtWork.Project.Applications {
    public class UploadAppService : AbpAppServiceBase {
        private readonly ISettingManager _settingManager;
        private readonly IOssClient _ossClient;

        public UploadAppService(SettingManager settingManager, IOssClient ossClient, ISettingManager settingManager1) {
            _ossClient = ossClient;
            _settingManager = settingManager1;
        }


        static string GetMd5(string str) {
            //创建MD5哈稀算法的默认实现的实例
            MD5 md5 = MD5.Create();
            //将指定字符串的所有字符编码为一个字节序列
            byte[] buffer = Encoding.Default.GetBytes(str);
            //计算指定字节数组的哈稀值
            byte[] bufferMd5 = md5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bufferMd5.Length; i++) {
                //x:表示将十进制转换成十六进制
                sb.Append(bufferMd5[i].ToString("x2"));
            }

            return sb.ToString();
        }

        [DontWrapResult]
        [HttpGet]
        public async Task<object> GetSignature(string data, string policy) {
            if (string.IsNullOrEmpty(data)) {
                throw new UserFriendlyException("上传签名参数data不能为空");
            }

            var password = GetMd5(await _settingManager.GetSettingValueAsync(OssSetting.Upyun.Password));

            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(password));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            return await Task.FromResult(new {
                signature = Convert.ToBase64String(hashBytes),
                bucket = await _settingManager.GetSettingValueAsync(OssSetting.Upyun.BucketName),
                @operator = await _settingManager.GetSettingValueAsync(OssSetting.Upyun.UserName),
                domainHost = await _settingManager.GetSettingValueAsync(OssSetting.Upyun.DomainHost),
                policy
            });
        }
    }
}