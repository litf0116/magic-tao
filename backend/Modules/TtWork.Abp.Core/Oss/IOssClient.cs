using System.Threading.Tasks;

namespace TtWork.Abp.Core.Oss
{
    public interface IOssClient
    {
        string Domain { get; set; }
        string BucketName { get; set; }
        
        string UserName { get; set; }

        Task<bool> writeFile(string path, byte[] data, bool auto_mkdir);
    }
}