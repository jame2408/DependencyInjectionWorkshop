﻿using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Proxy;
using DependencyInjectionWorkshop.Repo;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IProfile _profile;
        private IOtp _otpService;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private AuthenticationService _authenticationService;
        private const string DefaultAccountId = "joey";
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultPassword = "pw";
        private const string DefaultOtp = "123456";
        private const int DefaultFailedCount = 91;

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _otpService = Substitute.For<IOtp>();
            _hash = Substitute.For<IHash>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();

            _authenticationService = new AuthenticationService(_profile, _hash, _otpService, _failedCounter, _logger, _notification);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid_when_wrong_otp()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");

            ShouldBeInvalid(isValid);
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotifyUser();
        }

        [Test]
        public void Log_account_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);

            WhenInvalid();

            LogShouldContains(DefaultAccountId, DefaultFailedCount);
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCounter();
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount();
        }

        [Test]
        public void account_is_locked()
        {
            _failedCounter.CheckAccountIsLocked(DefaultAccountId).ReturnsForAnyArgs(true);

            TestDelegate action = () => _authenticationService.Verify(DefaultAccountId, DefaultPassword, DefaultOtp);
            Assert.Throws<FailedTooManyTimesException>(action);
        }

        private void ShouldAddFailedCount()
        {
            _failedCounter.ReceivedWithAnyArgs(1).Add(DefaultAccountId);
        }

        private void ShouldResetFailedCounter()
        {
            _failedCounter.Received(1).Reset(Arg.Any<string>());
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void LogShouldContains(string accountId, int failedCount)
        {
            _logger.Received(1).Info(Arg.Is<string>(m => m.Contains(accountId) && m.Contains(failedCount.ToString())));
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.Get(DefaultAccountId).ReturnsForAnyArgs(failedCount);
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            return WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");
        }

        private void ShouldNotifyUser()
        {
            _notification.Received(1).PushMessage(DefaultAccountId, Arg.Any<string>());
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).ReturnsForAnyArgs(otp);
        }

        private void GivenHash(string password, string hash)
        {
            _hash.GetHash(password).ReturnsForAnyArgs(hash);
        }

        private void GivenPassword(string accountId, string password)
        {
            _profile.GetPassword(accountId).ReturnsForAnyArgs(password);
        }
    }
}