using AspIdentityShared;
using AspnetIdentityDemo.Models;
using AspnetIdentityDemo.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace AspnetIdentityDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserService _userservice;
        private IMailService _mailservice;
        private IConfiguration _configuration;

        public AuthController(IUserService userService,IMailService mailservice,IConfiguration configuration)
        {
            _userservice = userService;
            _mailservice = mailservice;
            _configuration = configuration;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userservice.RegisterUserAsync(model);

                if(result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid");
        }


        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userservice.LoginUserAsync(model);

                if (result.IsSuccess)
                {
                    await _mailservice.SendEmailAsync(model.Email, "Login Notice", "<h1>Hey!, new login to your account noticed</h1><p>New login to your account at " + DateTime.Now + "</p>");
                    return Ok(result);
                }                

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid");
        }

        [HttpGet("ConfirmEmail")]

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return NotFound();

            var result = await _userservice.ConfirmEmailAsync(userId, token);

            if(result.IsSuccess)
            {
                return Redirect($"{_configuration["AppUrl"]}/ConfirmEmail.html");
            }

            return BadRequest(result);
        }

        [HttpPost("ForgotPassword")]

        public async Task<IActionResult> ForgotPassword(string email)
        {
            if(string.IsNullOrEmpty(email))
                return NotFound();

            var result = await _userservice.ForgotPasswordAsync(email);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("ResetPassword")]

        public async Task<IActionResult> ResetPassword([FromForm]ResetPassword model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userservice.ResetPasswordAsync(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result); 
            }

            return BadRequest("Some properties are not valid");
        }

    }
}
