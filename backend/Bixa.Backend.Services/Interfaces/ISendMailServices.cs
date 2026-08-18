namespace Bixa.Backend.Services.Interfaces
{
    public interface ISendMailServices
    {
        public Task<bool> SendMailRetrievePassword(string destinatario, string Tokken, string Ci);

        public Task<bool> SendMailNewUser(string destinatario, string tempPassword);

        public Task<bool> SendMailSolicitudCorreccion(string destinatario, string empleadoNombre, string empleadoCi, string comentario);

        public Task<bool> SendMailSolicitudPendienteAprobacion(string destinatario);

        public Task<bool> SendMailSolicitudFirmadaCompleta(string destinatario);

        public Task<bool> SendMailHcActualizado(string destinatario, string empleadoNombre, string empleadoCi);

        public Task<bool> SendMailNuevoMensajeChat(string destinatario, string empleadoNombre, string empleadoCi);
    }
}