using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Proxy;
using DependencyInjectionWorkshop.Repo;
using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profileRepo;
        private readonly IHash _sha256Adapter;
        private readonly IOpt _otpServiceProxy;
        private readonly IFailedCounter _failedCounterProxy;
        private readonly ILogger _logAdapter;
        private readonly INotification _notifyAdapter;

        public AuthenticationService(IProfile profileRepo, IHash sha256Adapter, IOpt otpServiceProxy, IFailedCounter failedCounterProxy, ILogger logAdapter, INotification notifyAdapter)
        {
            _profileRepo = profileRepo;
            _sha256Adapter = sha256Adapter;
            _otpServiceProxy = otpServiceProxy;
            _failedCounterProxy = failedCounterProxy;
            _logAdapter = logAdapter;
            _notifyAdapter = notifyAdapter;
        }

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _sha256Adapter = new Sha256Adapter();
            _otpServiceProxy = new OtpServiceProxy();
            _failedCounterProxy = new FailedCounterProxy();
            _logAdapter = new LogAdapter();
            _notifyAdapter = new NotifyAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            // if locked, return.
            _profileRepo.CheckAccountIsLocked(accountId);

            // Get DB Hash Password Using SP
            var passwordFromDb = _profileRepo.GetProfile(accountId);

            // Hash Users Key in Password
            var hashedPassword = _sha256Adapter.GetHashed(password);

            // Get Otp From Api
            var currentOpt = _otpServiceProxy.GetCurrentOpt(accountId);

            // 比對 hash password & otp
            if (passwordFromDb == hashedPassword.ToString() && otp == currentOpt)
            {
                // Verify success reset count
                _failedCounterProxy.Reset(accountId);

                return true;
            }

            // Verify failed count add 1
            _failedCounterProxy.Add(accountId);

            // 取得失敗次數，並用 NLog 記錄 log 資訊。
            var failedCount = _failedCounterProxy.Get(accountId);
            _logAdapter.Info(accountId, failedCount);

            // 比對失敗用 Slack 通知使用者
            _notifyAdapter.PushNotify(accountId, $"accountId:{accountId} verify failed.");

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}