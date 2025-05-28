using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using TtWork.Abp.Core.Oss;

namespace TtWork.Abp.Oss.UpYun
{
    //目录条目类

    public class UpYunClient : IOssClient
    {
        private readonly IUpyunApi _upyunApi;

        public string Domain { get; set; }
        public string BucketName { get; set; }
        public string UserName { get; set; }
        
        private string Password;
        private bool upAuth = false;
        private string DL = "/";
        private Hashtable tmp_infos = new Hashtable();
        private string file_secret;
        private string content_md5;
        private bool auto_mkdir = false;

        public string version()
        {
            return "1.0.1";
        }

        /**
        * 初始化 UpYun 存储接口
        * @param $bucketname 空间名称
        * @param $username 操作员名称
        * @param $password 密码
        * return UpYun object
        */
        public UpYunClient(ISettingManager settingManager, IUpyunApi upyunApi)
        {
            _upyunApi = upyunApi;
            BucketName = settingManager.GetSettingValue(OssSetting.Upyun.BucketName);
            UserName = settingManager.GetSettingValue(OssSetting.Upyun.UserName);
            Password = settingManager.GetSettingValue(OssSetting.Upyun.Password);
            Domain = settingManager.GetSettingValue(OssSetting.Upyun.DomainHost);
        }

        /**
        * 是否启用 又拍签名认证
        * @param upAuth {默认 false 不启用(直接使用basic auth)，true 启用又拍签名认证}
        * return null;
        */
        public void SetAuthType(bool upAuth)
        {
            this.upAuth = upAuth;
        }

        private void UpyunAuth(Hashtable headers, string method, string uri, HttpRequestMessage request, byte[] postData)
        {
            DateTime dt = DateTime.UtcNow;
            string date = dt.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", CultureInfo.CreateSpecificCulture("en-US"));
            // request.Date = dt;
            request.Headers.Add("Date", date);
            //headers.Add("Date", date);
            string auth;
            auth = Md5(method + '&' + uri + '&' + date + '&' + postData.Length + '&' + Md5(this.Password));
            headers.Add("Authorization", "UpYun " + this.UserName + ':' + auth);
        }

        private string Md5(string str)
        {
            var m = new MD5CryptoServiceProvider();
            var s = m.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(s).Replace("-", "").ToLower();
        }

        private async Task<bool> Delete(string path, Hashtable headers = null)
        {
            var resp = await NewWorker(HttpMethod.Delete, DL + BucketName + path, null, headers);
            return resp.StatusCode == HttpStatusCode.OK;
        }

        private async Task<HttpResponseMessage> NewWorker(HttpMethod method, string Url, byte[] postData, Hashtable headers = null)
        {
            headers ??= new Hashtable();
            var request = new HttpRequestMessage(method, Url);
            request.Content = new ByteArrayContent(postData);
            if (auto_mkdir)
            {
                headers.Add("mkdir", "true");
                auto_mkdir = false;
            }

            if (postData != null)
            {
                // request.ContentLength = postData.Length;
                // request.KeepAlive = true;
                if (content_md5 != null)
                {
                    request.Headers.Add("Content-MD5", content_md5);
                    content_md5 = null;
                }

                if (file_secret != null)
                {
                    request.Headers.Add("Content-Secret", file_secret);
                    file_secret = null;
                }
            }

            if (upAuth)
            {
                UpyunAuth(headers, method.ToString(), Url, request, postData);
            }
            else
            {
                request.Headers.Add("Authorization", "Basic " +
                                                     Convert.ToBase64String(new ASCIIEncoding().GetBytes(this.UserName + ":" + this.Password)));
            }

            foreach (DictionaryEntry var in headers)
            {
                if (var.Value != null) request.Headers.Add(var.Key.ToString(), var.Value.ToString());
            }

            var response = await _upyunApi.SendAsync(request);

            tmp_infos = new Hashtable();
            foreach (var hl in response.Headers)
            {
                string name = hl.Key;
                if (name.Length > 7 && name.Substring(0, 7) == "x-upyun")
                {
                    tmp_infos.Add(name, hl.Value);
                }
            }

            return response;
        }

        /**
        * 获取总体空间的占用信息
        * return 空间占用量，失败返回 null
        */
        public async Task<long> GetFolderUsage(string url)
        {
            Hashtable headers = new Hashtable();
            long size;
            byte[] a = null;
            var resp = await NewWorker(HttpMethod.Get, DL + this.BucketName + url + "?usage", a, headers);
            try
            {
                string strhtml = await resp.Content.ReadAsStringAsync();
                size = long.Parse(strhtml);
            }
            catch (Exception)
            {
                size = 0;
            }

            return size;
        }

        /**
           * 获取某个子目录的占用信息
           * @param $path 目标路径
           * return 空间占用量，失败返回 null
           */
        public async Task<long> getBucketUsage()
        {
            return await GetFolderUsage("/");
        }

        /**
        * 创建目录
        * @param $path 目录路径
        * return true or false
        */
        public async Task<bool> MkDir(string path, bool auto_mkdir)
        {
            this.auto_mkdir = auto_mkdir;
            var headers = new Hashtable {{"folder", "create"}};
            var a = new byte[0];
            var resp = await NewWorker(HttpMethod.Post, DL + this.BucketName + path, a, headers);
            return resp.StatusCode == HttpStatusCode.OK;
        }

        /**
        * 删除目录
        * @param $path 目录路径
        * return true or false
        */
        public async Task<bool> RmDir(string path)
        {
            var headers = new Hashtable();
            return await Delete(path, headers);
        }

        /**
        * 读取目录列表
        * @param $path 目录路径
        * return array 数组 或 null
        */
        public async Task<ArrayList> ReadDir(string url)
        {
            var headers = new Hashtable();
            var resp = await NewWorker(HttpMethod.Get, DL + this.BucketName + url, null, headers);
            string strhtml = await resp.Content.ReadAsStringAsync();
            strhtml = strhtml.Replace("\t", "\\");
            strhtml = strhtml.Replace("\n", "\\");
            string[] ss = strhtml.Split('\\');
            int i = 0;
            ArrayList AL = new ArrayList();
            while (i < ss.Length)
            {
                FolderItem fi = new FolderItem(ss[i], ss[i + 1], int.Parse(ss[i + 2]), int.Parse(ss[i + 3]));
                AL.Add(fi);
                i += 4;
            }

            return AL;
        }


        /**
        * 上传文件
        * @param $file 文件路径（包含文件名）
        * @param $datas 文件内容 或 文件IO数据流
        * return true or false
        */
        public virtual async Task<bool> writeFile(string path, byte[] data, bool auto_mkdir)
        {
            this.auto_mkdir = auto_mkdir;
            var resp = await NewWorker(HttpMethod.Post, DL + BucketName + path, data);
            return resp.StatusCode == HttpStatusCode.OK;
        }

        /**
        * 删除文件
        * @param $file 文件路径（包含文件名）
        * return true or false
        */
        public async Task<bool> DeleteFile(string path)
        {
            return await Delete(path);
        }

        /**
        * 读取文件
        * @param $file 文件路径（包含文件名）
        * @param $output_file 可传递文件IO数据流（默认为 null，结果返回文件内容，如设置文件数据流，将返回 true or false）
        * return 文件内容 或 null
        */
        public async Task<byte[]> ReadFile(string path)
        {
            var resp = await NewWorker(HttpMethod.Get, DL + this.BucketName + path, null);
            return await resp.Content.ReadAsByteArrayAsync();
        }

        /**
        * 设置待上传文件的 Content-MD5 值（如又拍云服务端收到的文件MD5值与用户设置的不一致，将回报 406 Not Acceptable 错误）
        * @param $str （文件 MD5 校验码）
        * return null;
        */
        public void SetContentMD5(string str)
        {
            content_md5 = str;
        }

        /**
        * 设置待上传文件的 访问密钥（注意：仅支持图片空！，设置密钥后，无法根据原文件URL直接访问，需带 URL 后面加上 （缩略图间隔标志符+密钥） 进行访问）
        * 如缩略图间隔标志符为 ! ，密钥为 bac，上传文件路径为 /folder/test.jpg ，那么该图片的对外访问地址为： http://空间域名/folder/test.jpg!bac
        * @param $str （文件 MD5 校验码）
        * return null;
        */
        public void SetFileSecret(string str)
        {
            file_secret = str;
        }

        /**
        * 获取文件信息
        * @param $file 文件路径（包含文件名）
        * return array('type'=> file | folder, 'size'=> file size, 'date'=> unix time) 或 null
        */
        public async Task<Hashtable> GetFileInfo(string file)
        {
            var resp = await NewWorker(HttpMethod.Head, DL + this.BucketName + file, null);
            Hashtable ht;
            try
            {
                ht = new Hashtable();
                ht.Add("type", this.tmp_infos["x-upyun-file-type"]);
                ht.Add("size", this.tmp_infos["x-upyun-file-size"]);
                ht.Add("date", this.tmp_infos["x-upyun-file-date"]);
            }
            catch (Exception)
            {
                ht = new Hashtable();
            }

            return ht;
        }

        //获取上传后的图片信息（仅图片空间有返回数据）
        public object GetWritedFileInfo(string key)
        {
            return tmp_infos == new Hashtable() ? "" : tmp_infos[key];
        }

        //计算文件的MD5码
        public static string Md5File(string pathName)
        {
            var strResult = "";
            var strHashData = "";

            byte[] arrbytHashValue;
            FileStream oFileStream = null;

            var oMD5Hasher = new MD5CryptoServiceProvider();

            try
            {
                oFileStream = new System.IO.FileStream(pathName, System.IO.FileMode.Open,
                    System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream); //计算指定Stream 对象的哈希值
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = System.BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return strResult.ToLower();
        }
    }
}