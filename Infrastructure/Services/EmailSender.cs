using Microsoft.AspNetCore.Identity;
using MisFinanzas.Domain.Entities;
using System.Net;
using System.Net.Mail;

namespace MisFinanzas.Infrastructure.Services
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var subject = "Confirma tu cuenta - MisFinanzas";
            var htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #0a192f 0%, #1e3a5f 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='color: white; margin: 0;'>🎉 ¡Bienvenido a MisFinanzas!</h1>
                    </div>
                    <div style='background: #f7f7f7; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <h2 style='color: #333;'>Hola {user.Email}!</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            Gracias por registrarte en MisFinanzas. Para completar tu registro y comenzar a gestionar tus finanzas, 
                            necesitamos que confirmes tu dirección de correo electrónico.
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' 
                               style='background: linear-gradient(135deg, #0a192f 0%, #1e3a5f 100%); 
                                      color: white; 
                                      padding: 15px 30px; 
                                      text-decoration: none; 
                                      border-radius: 8px; 
                                      font-weight: bold;
                                      display: inline-block;'>
                                Confirmar mi cuenta
                            </a>
                        </div>
                        <p style='color: #999; font-size: 14px; margin-top: 30px;'>
                            Si no creaste esta cuenta, puedes ignorar este correo.
                        </p>
                    </div>
                </div>
            ";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var subject = "Restablecer contraseña - MisFinanzas";
            var htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #0a192f 0%, #1e3a5f 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='color: white; margin: 0;'>🔐 Restablecer Contraseña</h1>
                    </div>
                    <div style='background: #f7f7f7; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <h2 style='color: #333;'>Hola {user.Email}!</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            Recibimos una solicitud para restablecer tu contraseña. Haz clic en el botón de abajo para crear una nueva contraseña.
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background: linear-gradient(135deg, #0a192f 0%, #1e3a5f 100%); 
                                      color: white; 
                                      padding: 15px 30px; 
                                      text-decoration: none; 
                                      border-radius: 8px; 
                                      font-weight: bold;
                                      display: inline-block;'>
                                Restablecer mi contraseña
                            </a>
                        </div>
                        <p style='color: #999; font-size: 14px; margin-top: 30px;'>
                            Si no solicitaste restablecer tu contraseña, ignora este correo. Tu contraseña no cambiará.
                        </p>
                        <p style='color: #999; font-size: 12px; margin-top: 10px;'>
                            Este enlace expirará en 24 horas por seguridad.
                        </p>
                    </div>
                </div>
            ";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var subject = "Código de restablecimiento - MisFinanzas";
            var htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='color: white; margin: 0;'>🔑 Código de Restablecimiento</h1>
                    </div>
                    <div style='background: #f7f7f7; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <h2 style='color: #333;'>Hola {user.Email}!</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            Tu código de restablecimiento de contraseña es:
                        </p>
                        <div style='background: white; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                            <h1 style='color: #667eea; margin: 0; font-size: 32px; letter-spacing: 4px;'>{resetCode}</h1>
                        </div>
                        <p style='color: #999; font-size: 14px; margin-top: 30px;'>
                            Ingresa este código en la página de restablecimiento de contraseña.
                        </p>
                        <p style='color: #999; font-size: 12px; margin-top: 10px;'>
                            Si no solicitaste este código, ignora este correo.
                        </p>
                    </div>
                </div>
            ";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        private async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "MisFinanzas";

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email settings are not configured. Email not sent to {Email}", email);
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUsername, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", email);
                throw; // Re-lanzar para que Identity maneje el error
            }
        }
    }
}