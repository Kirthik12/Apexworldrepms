using System.Threading.Tasks;

namespace ApexWorld_Backend.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}