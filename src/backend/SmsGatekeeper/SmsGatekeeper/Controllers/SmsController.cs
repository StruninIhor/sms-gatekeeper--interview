using Microsoft.AspNetCore.Mvc;
using SmsGatekeeper.Models;
using SmsGatekeeper.Services;

namespace SmsGatekeeper.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SmsController(ISmsService smsService) : ControllerBase
    {
        private readonly ISmsService _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));

        [HttpPost]

        /// <summary>
        /// Returns whether this phoneNumber can send an SMS message.
        /// </summary>
        /// <param name="phoneNumber">Phone number in format "1NPANXXXXX", where 1 - country code, NPA - area code, NXX - exchange code, XXXX - subscriber number. Now only supports US/Canada</param>
        /// <returns></returns>
        public async Task<IActionResult> CanSendSms(SendMessageRequest request) =>
            await _smsService.CanSendSms(request.AccountId, request.PhoneNumber) ? Ok() : Problem(statusCode:  StatusCodes.Status429TooManyRequests);
    }
}
