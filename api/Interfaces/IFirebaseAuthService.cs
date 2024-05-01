using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTO.Account;
using Firebase.Auth;

namespace api.Interfaces
{
    public interface IFirebaseAuthService
    {
        Task<UserDTO?> SignUp(string email, string password, string username);
        Task<UserDTO?> Login(string email, string password);
        User GetUser();
        void SignOut();
    }
}