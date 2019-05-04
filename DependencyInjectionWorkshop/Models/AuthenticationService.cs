using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Proxy;
using DependencyInjectionWorkshop.Repo;
using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpServiceProxy _otpServiceProxy = new OtpServiceProxy();
        private readonly ProfileRepo _profileRepo = new ProfileRepo();
        private readonly FailedCounterProxy _failedCounterProxy = new FailedCounterProxy();
        private readonly LogAdapter _logAdapter = new LogAdapter();
        private readonly NotifyAdapter _notifyAdapter = new NotifyAdapter();

        public bool Verify(string accountId, string password, string otp)
        {
            // if locked, return.
            _profileRepo.CheckAccountIsLocked(accountId);

            // Get DB Hash Password Using SP
            var passwordFromDb = _profileRepo.GetPasswordFromDb(accountId);

            // Hash Users Key in Password
            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            // Get Otp From Api
            var currentOpt = _otpServiceProxy.GetCurrentOpt(accountId);

            // 比對 hash password & otp
            if (passwordFromDb == hashedPassword.ToString() && otp == currentOpt)
            {
                // Verify success reset count
                _failedCounterProxy.ResetFailedCounter(accountId);

                return true;
            }

            // Verify failed count add 1
            _failedCounterProxy.AddFailedCount(accountId);

            // 取得失敗次數，並用 NLog 記錄 log 資訊。
            var failedCount = _failedCounterProxy.GetFailedCount(accountId);
            _logAdapter.LogFailedCount(accountId, failedCount);

            // 比對失敗用 Slack 通知使用者
            _notifyAdapter.Notify(accountId, $"accountId:{accountId} verify failed.");

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}