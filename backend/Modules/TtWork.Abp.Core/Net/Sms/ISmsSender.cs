using System.Threading.Tasks;

namespace TtWork.Abp.Core.Net.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}