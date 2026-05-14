namespace Bixa.Backend.Services.Interfaces
{
    public interface ISendMailServices
    {
        public Task<bool> SendMailRetrievePassword(string destinatario, string Tokken, string Ci);

        public Task<bool> SendMailNewUser(string destinatario, string tempPassword);
    }
}