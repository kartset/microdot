﻿using System;
using Gigya.Microdot.Hosting.Environment;
using Gigya.Microdot.Interfaces.Events;
using Gigya.Microdot.Interfaces.Logging;
using Gigya.Microdot.Interfaces.SystemWrappers;
using Gigya.Microdot.SharedLogic;
using Ninject;

namespace Gigya.Microdot.Ninject
{
    public class MicrodotInitializer : IDisposable
    {
        public MicrodotInitializer(string appName, ILoggingModule loggingModule, Action<IKernel> additionalBindings = null)
        {
            Kernel = new StandardKernel();
            Kernel.Load<MicrodotModule>();

            Kernel
                .Bind<IEnvironment>()
                .ToConstant(new HostEnvironment(new TestHostEnvironmentSource(appName: appName)))
                .InSingletonScope();

            Kernel
                .Bind<CurrentApplicationInfo>()
                .ToConstant(new CurrentApplicationInfo(appName, Environment.UserName, System.Net.Dns.GetHostName()))
                .InSingletonScope();

            loggingModule.Bind(Kernel.Rebind<ILog>(), Kernel.Rebind<IEventPublisher>(), Kernel.Rebind<Func<string, ILog>>());
            // Set custom Binding 
            additionalBindings?.Invoke(Kernel);

            Kernel.Get<SystemInitializer.SystemInitializer>().Init();
        }

        public IKernel Kernel { get; private set; }

        public void Dispose()
        {
            Kernel?.Dispose();
        }
    }
}