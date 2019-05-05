using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Proxy;
using DependencyInjectionWorkshop.Repo;
using System;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otpService;

        public AuthenticationService(IProfile profileRepo, IHash sha256Adapter, IOtp otpServiceProxy)
        {
            _profile = profileRepo;
            _hash = sha256Adapter;
            _otpService = otpServiceProxy;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            // Get DB Hash Password Using SP
            var passwordFromDb = _profile.GetPassword(accountId);

            // Hash Users Key in Password
            var hashedPassword = _hash.GetHash(password);

            // Get Otp From Api
            var currentOpt = _otpService.GetCurrentOtp(accountId);

            // 比對 hash password & otp
            var isValid = passwordFromDb == hashedPassword.ToString() && otp == currentOpt;

            return isValid;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}