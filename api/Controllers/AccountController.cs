using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Account;
using api.Interfaces;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IFirebaseAuthService _firebaseAuthService;
        public AccountController(IFirebaseAuthService firebaseAuthService)
        {
            _firebaseAuthService = firebaseAuthService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            UserDTO? user = await _firebaseAuthService.SignUp(registerDTO.Email, registerDTO.Password, registerDTO.Username);

            if(user == null) return StatusCode(500, "Could not register new user.");
            return Ok(user);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            UserDTO? user = await _firebaseAuthService.Login(loginDTO.Email, loginDTO.Password);

            if(user == null) return StatusCode(400, "Wrong credentials");
            return Ok(user);
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SignOut(){
            
            string jwtToken = HttpContext.Request.Headers["Authorization"].ToString();

            jwtToken = jwtToken.Replace("Bearer ", "");

            string? userId = await _firebaseAuthService.GetUserId(jwtToken);

            if(userId == null) return Unauthorized();

            await _firebaseAuthService.SignOut(userId);
            return Ok();
        }
    }
}