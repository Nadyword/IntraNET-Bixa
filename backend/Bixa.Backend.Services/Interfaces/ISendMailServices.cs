namespace Bixa.Backend.Services.Interfaces
{
    public interface ISendMailServices
    {
        public Task<bool> SendMailRetrievePassword(string destinatario, string Tokken, string TaxId);
    }
}