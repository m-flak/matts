/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
**/
using FluentValidation;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using matts.Configuration;
using matts.Interfaces;
using matts.Constants;
using matts.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace matts.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IValidator<User> _validator;
    private readonly IValidator<UserRegistration> _validatorRegister;
    private readonly ILogger<AuthController> _logger;
    private readonly IOptions<JwtConfiguration> _options;
    private readonly IUserService _userService;
    private readonly ILinkedinOAuthService _linkedinService;

    public AuthController(
        ILogger<AuthController> logger, 
        IOptions<JwtConfiguration> options, 
        IValidator<User> validator, 
        IValidator<UserRegistration> validatorRegister, 
        IUserService userService,
        ILinkedinOAuthService linkedinService) 
    {
        _logger = logger;
        _options = options;
        _validator = validator;
        _validatorRegister = validatorRegister;
        _userService = userService;
        _linkedinService = linkedinService;
    }

    [AllowAnonymous]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        var validationResult = _validator.Validate(user);
        if (!validationResult.IsValid) 
        {
            return new BadRequestObjectResult(validationResult.Errors);
        }

        bool authenticated = await _userService.AuthenticateUser(user);
        if (!authenticated)
        {
            return Unauthorized();
        }

        var issuer = _options.Value.Issuer;
        var audience = _options.Value.Audience;
        var signingKey = _options.Value.SigningKey;

        if (issuer == null || audience == null || signingKey == null)
        {
            string sensitiveValue(string? value)
            {
                return (value == null) ? "null" : "***";
            }

            _logger.LogError("JWT Configuration Invalid! Issuer: {Issuer}, Audience: {Audience}, SigningKey: {SigningKey}",
                sensitiveValue(issuer),
                sensitiveValue(audience),
                sensitiveValue(signingKey)
            );

            // Throw so a nice scary 500 is generated
            throw new InvalidOperationException("One of the required JWT Configuration values is missing from configuration");
        }

        var claims = new List<Claim>()
        {
            new Claim("id", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role!),
            new Claim(JwtRegisteredClaimNames.Sub, user!.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.Role == UserRoleConstants.USER_ROLE_APPLICANT)
        {
            claims.Add(new Claim("applicantId", await _userService.GetUserApplicantId(user)));
        }
        else if (user.Role == UserRoleConstants.USER_ROLE_EMPLOYER)
        {
            claims.Add(new Claim("employerId", await _userService.GetUserEmployerId(user)));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(signingKey)), SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(tokenHandler.WriteToken(token));
    }

    [AllowAnonymous]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistration user)
    {
        var validationResult = _validatorRegister.Validate(user);
        if (!validationResult.IsValid)
        {
            return new BadRequestObjectResult(validationResult.Errors);
        }

        bool userCreated = await _userService.RegisterNewUser(user);
        if (!userCreated)
        {
            return BadRequest();
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Route("linkedin/callback")]
    public IActionResult LinkedinCallback(
        [Required][FromQuery] string code,
        [Required][FromQuery] string state,
        [Optional][FromQuery] string? error,
        [Optional][FromQuery] string? error_description
        )
    {
        if (error is not null)
        {
            var ex = new Exception($"{error} - {error_description}");
            _linkedinService.CancelFlow(state, ex);
            return Unauthorized();
        }

        if (!_linkedinService.IsFlowInProgress(state))
        {
            return Unauthorized();
        }

        _linkedinService.SaveClientAuthCode(state, code);
        return Ok();
    }
}
