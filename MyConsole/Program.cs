using Autofac;
using DependencyInjectionWorkshop.Adapters;
using DependencyInjectionWorkshop.Decorator;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Proxy;
using DependencyInjectionWorkshop.Repo;
using System;

namespace MyConsole
{
    internal class Program
    {
        private static IContainer _container;

        private static void Main(string[] args)
        {
            // 版本一：不用 DI Framework
            //IProfile profile = new FakeProfile();
            //IHash hash = new FakeHash();
            //IOtp otpService = new FakeOtp();
            //INotification notification = new FakeSlack();
            //IFailedCounter failedCounter = new FakeFailedCounter();
            //ILogger logger = new FakeLog();

            //var authenticationService = new AuthenticationService(profile, hash, otpService);
            //var notificationDecorator = new NotificationDecorator(authenticationService, notification);
            //var failedCounterDecorator = new FailedCounterDecorator(notificationDecorator, failedCounter);
            //var logDecorator = new LoggerDecorator(failedCounterDecorator, logger, failedCounter);

            //var finalAuthentication = logDecorator;
            //var isValid = finalAuthentication.Verify("joey", "pw", "123456");

            // 版本二：使用 DI Framework
            RegisterContainer();

            IAuthentication authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.Verify("joey", "pw", "123457");

            Console.WriteLine(isValid);
        }

        private static void RegisterContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<FakeOtp>()
                .As<IOtp>();

            containerBuilder.RegisterType<FakeHash>()
                .As<IHash>();

            containerBuilder.RegisterType<FakeProfile>()
                .As<IProfile>();

            containerBuilder.RegisterType<FakeSlack>()
                .As<INotification>();

            containerBuilder.RegisterType<FakeLog>()
                .As<ILogger>();

            containerBuilder.RegisterType<FakeFailedCounter>()
                .As<IFailedCounter>();

            containerBuilder.RegisterType<LoggerDecorator>();
            containerBuilder.RegisterType<NotificationDecorator>();
            containerBuilder.RegisterType<FailedCounterDecorator>();

            containerBuilder.RegisterType<AuthenticationService>()
                .AsSelf()
                .As<IAuthentication>();

            containerBuilder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            containerBuilder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            containerBuilder.RegisterDecorator<LoggerDecorator, IAuthentication>();

            _container = containerBuilder.Build();
        }
    }

    internal class FakeLog : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"{nameof(FakeLog)}.{nameof(Info)}({message})");
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string accountId, string message)
        {
            Console.WriteLine($"{nameof(FakeSlack)}.{nameof(PushMessage)}({message})");
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
        }

        public int Get(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({accountId})");
            return 91;
        }

        public bool CheckAccountIsLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(CheckAccountIsLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtp
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string GetHash(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(GetHash)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}