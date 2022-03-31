using Entities.DataTransferObjects.User;
using Entities.EntityModels;
using Entities.Utils.Models;
using Entities.ViewModels.User;
using Interfaces;
using Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UserService : BaseService, IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettingsOptions,
            SignInManager<ApplicationUser> signInManager,
            IData repositoryWrapper)
            : base(repositoryWrapper, userManager)
        {
            this._userManager = userManager;
            this._jwtSettings = jwtSettingsOptions.Value;
            this._signInManager = signInManager;
        }

        public async Task<LoginViewModel> Login(LoginInputDTO userDto)
        {
            var currentUser = await this._userManager.Users.Where(em => em.Email == userDto.Email).FirstOrDefaultAsync();

            if (currentUser is null)
            {
                throw new ArgumentNullException("Invalid user!");
            }

            var result = await this._signInManager.CheckPasswordSignInAsync(currentUser, userDto.Password, false);

            if (!result.Succeeded)
            {
                throw new ArgumentException("Invalid User or Password");
            }

            var loginUserViewModel = new LoginViewModel()
            {
                Message = "Successfully logged in!",
                Token = await this.GenerateJwtTokenAsync(currentUser)
            };

            return loginUserViewModel;
        }

        public async Task Logout()
        {
            await this._signInManager.SignOutAsync();
        }

        public async Task<string> Register(RegisterInputDTO registerInputModel)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerInputModel.Email);

            if (existingUser != null)
            {
                throw new ArgumentException("User already exist");
            }

            var newUser = new ApplicationUser()
            {
                Email = registerInputModel.Email,
                UserName = registerInputModel.Email
            };

            var isCreated = await _userManager.CreateAsync(newUser, registerInputModel.Password);
            await _userManager.AddToRoleAsync(newUser, "Admin");

            if (!isCreated.Succeeded)
            {
                throw new ArgumentException("user is not created");
            }

            var jwtToken = await GenerateJwtTokenAsync(newUser);

            return jwtToken;
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser newUser)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var userRoles = await _userManager.GetRolesAsync(newUser);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.NameId, newUser.Id),
                    new Claim("Role", userRoles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Sub, newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Email, newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                Audience = _jwtSettings.Audience,
                Issuer = _jwtSettings.Issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var preparedToken = jwtTokenHandler.CreateToken(tokenDescription);
            var createdToken = jwtTokenHandler.WriteToken(preparedToken);

            return createdToken;
        }
    }
}