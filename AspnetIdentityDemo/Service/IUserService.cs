using AspIdentityShared;
using AspnetIdentityDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AspnetIdentityDemo.Service
{
    public interface IUserService
    {
        Task<UserManagementResponse> RegisterUserAsync(RegisterViewModel model);

        Task<UserManagementResponse> LoginUserAsync(LoginViewModel model);

        Task<UserManagementResponse> ConfirmEmailAsync(string userId, string token);

        Task<UserManagementResponse> ForgotPasswordAsync(string email);

        Task<UserManagementResponse> ResetPasswordAsync(ResetPassword model);
    }

    public class UserService : IUserService
    {

        private UserManager<IdentityUser> _userManager;
        private IConfiguration _configuration;
        private IMailService _mailservice;

        public UserService(UserManager<IdentityUser> userManager,IConfiguration configuration,IMailService mailservice)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mailservice = mailservice;
        }

        public async Task<UserManagementResponse> LoginUserAsync(LoginViewModel model)
        {

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return new UserManagementResponse
                {
                    Message = "There is no user with this Email Address",
                    IsSuccess = false,
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!result)
                return new UserManagementResponse
                {
                    Message = "Invalid Password",
                    IsSuccess = false,
                };

            var claims = new[]
            {
                new Claim("Email",model.Email),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings: Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new UserManagementResponse
            {
                Message = tokenAsString,
                IsSuccess = true,
                ExpireDate = token.ValidTo
            };
            
        }

        public async Task<UserManagementResponse> RegisterUserAsync(RegisterViewModel model)
        {
            if (model == null)
                throw new NullReferenceException("Register Model is null");

            if (model.Password != model.ConfirmPassword)
                return new UserManagementResponse
                {
                    Message = "Confirm Password doesn't match Password",
                    IsSuccess = false,
                };

            var identityUser = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email,
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);


            if(result.Succeeded)
            {
                //TODO:Send a confirmation Email
                var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                var encodingEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodingEmailToken);

                string url = $"{_configuration["AppUrl"]}/api/auth/confirmemail?userid={identityUser.Id}&token={validEmailToken}";

                await _mailservice.SendEmailAsync(identityUser.Email, "Confirm your Email", "<h1>Welcome to authentication Demo</h1>" + $"<p>Please Confirm your Email by <a href='{url}'>Clicking here</a></p>");

                return new UserManagementResponse
                {
                    Message = "User Created successfully",
                    IsSuccess = true
                };
            }

            return new UserManagementResponse
            {
                Message = "User did not create",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<UserManagementResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new UserManagementResponse()
                {
                    IsSuccess = true,
                    Message = "User not found"
                };
            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new UserManagementResponse
                {
                    Message = "Email Confirmed Successfully!",
                    IsSuccess = true,
                   
                };
            return new UserManagementResponse
            {
                IsSuccess = false,
                Message = "Email did not Confirm",
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<UserManagementResponse> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new UserManagementResponse
                {
                    IsSuccess = false,
                    Message = "No User with this Mail",
                };
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodingEmailToken = Encoding.UTF8.GetBytes(token);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodingEmailToken);


            string url = $"{_configuration["AppUrl"]}/ResetPassword?email={email}&token={validEmailToken}";

            await _mailservice.SendEmailAsync(email, "Reset Password", "<h1>Follow Instruction to reset your password<h1>" +
                $"<p>To reset your password <a href='{url}'>Click here</p>");

            return new UserManagementResponse
            {
                IsSuccess = true,
                Message = "Reset Password Url has been sent to the email successfully!"
            };
        }

        public async Task<UserManagementResponse> ResetPasswordAsync(ResetPassword model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new UserManagementResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };

            if(model.NewPassword != model.ConfirmPassword)
            
                return new UserManagementResponse
                {
                    IsSuccess = false,
                    Message = "Password do not match",
                };


                    var decodedToken = WebEncoders.Base64UrlDecode(model.token);
                    string normalToken = Encoding.UTF8.GetString(decodedToken);
                var result = await _userManager.ResetPasswordAsync(user, normalToken, model.NewPassword);

                if (result.Succeeded)
                    return new UserManagementResponse
                    {
                        Message = "Password has been reset successfully",
                        IsSuccess = true
                    };

                return new UserManagementResponse
                {
                    Message = "Something went wrong",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description),
                };
            }
        }
    }

