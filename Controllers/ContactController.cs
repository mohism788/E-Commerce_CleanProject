using E_Commerce.DTOs;
using E_Commerce.Models;
using E_Commerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : BaseApiController
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailService emailService, IConfiguration configuration, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMessage([FromBody] ContactRequestDto contactRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Invalid contact form data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                var receiverEmail = _configuration["SmtpSettings:ReceiverEmail"];
                var subject = $"[Contact Form] {contactRequest.Subject}";
                var body = $@"
                    <h3>New Contact Form Submission</h3>
                    <p><strong>Name:</strong> {contactRequest.Name}</p>
                    <p><strong>Email:</strong> {contactRequest.Email}</p>
                    <p><strong>Subject:</strong> {contactRequest.Subject}</p>
                    <hr/>
                    <p><strong>Message:</strong></p>
                    <p style='white-space: pre-wrap;'>{contactRequest.Message}</p>
                ";

                // Send email to the site owner, with Reply-To set to the user's email
                await _emailService.SendEmailAsync(receiverEmail, subject, body, contactRequest.Email);

                // Send Auto-Reply to the Customer
                var autoReplySubject = "We received your message - Berserk Tech 🩸";
                var autoReplyBody = $@"
                    <div style='font-family: sans-serif; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #eee; padding: 20px; border-radius: 8px;'>
                        <h2 style='color: #e63946;'>Hello {contactRequest.Name},</h2>
                        <p>Thank you for reaching out to <strong>Berserk Tech</strong>.</p>
                        <p>We’ve received your message regarding <strong>""{contactRequest.Subject}""</strong> and our tacticians are already reviewing it.</p>
                        <p>We know the struggle of a build, and we'll get back to you as soon as possible.</p>
                        <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                        <p style='font-size: 12px; color: #888;'>This is an automated response. No need to reply to this email.</p>
                        <p style='font-weight: bold;'>Keep on Struggling,<br/>The Berserk Tech Team</p>
                    </div>
                ";
                await _emailService.SendEmailAsync(contactRequest.Email, autoReplySubject, autoReplyBody);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Your message has been sent successfully. Check your email for a confirmation!",
                    Data = "Success"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact email from {Email}", contactRequest.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
                {
                    Success = false,
                    Message = "Failed to send message. Please try again later.",
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
