﻿using DependencyInjectionWorkshop.Adapters;
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
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profileRepo, IHash sha256Adapter, IOtp otpServiceProxy, IFailedCounter failedCounterProxy, ILogger logAdapter)
        {
            _profile = profileRepo;
            _hash = sha256Adapter;
            _otpService = otpServiceProxy;
            _failedCounter = failedCounterProxy;
            _logger = logAdapter;
        }

        //public AuthenticationService()
        //{
        //    _profile = new ProfileRepo();
        //    _hash = new Sha256Adapter();
        //    _otpService = new OtpServiceProxy();
        //    _failedCounter = new FailedCounterProxy();
        //    _logger = new LogAdapter();
        //    _notification = new NotifyAdapter();
        //}

        public bool Verify(string accountId, string password, string otp)
        {
            // if locked, return.
            if (_failedCounter.CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            // Get DB Hash Password Using SP
            var passwordFromDb = _profile.GetPassword(accountId);

            // Hash Users Key in Password
            var hashedPassword = _hash.GetHash(password);

            // Get Otp From Api
            var currentOpt = _otpService.GetCurrentOtp(accountId);

            // 比對 hash password & otp
            if (passwordFromDb == hashedPassword.ToString() && otp == currentOpt)
            {
                // Verify success reset count
                _failedCounter.Reset(accountId);

                return true;
            }

            // Verify failed count add 1
            _failedCounter.Add(accountId);

            // 取得失敗次數，並用 NLog 記錄 log 資訊。
            var failedCount = _failedCounter.Get(accountId);
            _logger.Info($"Verify Failed. AccountId: {accountId}, Failed Times: {failedCount}");

            // 比對失敗用 Slack 通知使用者
            //_notificationDecorator.NotificationVerify(accountId);

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}