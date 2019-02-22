using System.Threading.Tasks;

namespace TaskZero.Console
{
    public interface IMessageSender
    {
        Task<string> Post<T>(string path, T data);
    }
}
