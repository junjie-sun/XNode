using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XNode.Autofac;
using XNode.Client.Configuration;
using XNode.Client.Console.Services;
using XNode.Client.ServiceCallers;
using XNode.Communication;
using XNode.Communication.DotNetty;
using XNode.Logging;
using XNode.Security;
using XNode.Serializer.ProtoBuf;

namespace XNode.Client.Console
{
    public static class SimpleClientTest
    {
        private static IContainer container;

        public static void Test()
        {
            System.Console.ReadLine();

            var serviceProxyManager = InitWithConfig();

            System.Console.ReadLine();

            DoTest();

            System.Console.ReadLine();

            serviceProxyManager.CloseAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    System.Console.WriteLine(task.Exception.InnerException.Message);
                    return;
                }
                System.Console.WriteLine("Closed");
            }).ContinueWith(task => BootstrapManager.Disable());
        }

        private static IServiceProxyManager InitWithConfig()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config_simple.json");

            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var globalConfig = configRoot.GetGlobalConfig();
            GlobalSettings.Apply(globalConfig);

            var clientConfig = configRoot.GetClientConfig();

            LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Error);

            //var serializer = new MsgPackSerializer(LoggerManager.ClientLoggerFactory);
            var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

            var serviceCaller = new ServiceCallerBuilder()
                .UseDefault()
                .Build();

            var serviceProxyManager = new ServiceProxyManager();

            foreach (var config in clientConfig.ServiceProxies)
            {
                var loggerFactory = LoggerManager.ClientLoggerFactory;
                serviceProxyManager
                    .Regist(config, serviceCaller)
                    .AddClients(
                        new NodeClientBuilder()
                            .ConfigConnections(config.Connections)
                            .ConfigSerializer(serializer)
                            .ConfigLoginHandler(new DefaultLoginHandler(configRoot.GetDefaultLoginHandlerConfig(config.ProxyName), serializer))
                            .UseDotNetty()
                            .Build()
                    );
            }

            serviceProxyManager.ConnectAsync().Wait();

            var builder = new ContainerBuilder();
            builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
            builder.RegisterType<SimpleService>()
               .As<ISimpleService>()
               .EnableInterfaceInterceptors()       //接口拦截
               .InterceptedBy(typeof(ServiceProxyInterceptor))
               .SingleInstance();

            container = builder.Build();
            return serviceProxyManager;
        }

        private static void DoTest()
        {
            System.Console.WriteLine("------------------------------SimpleService----------------------------------");

            try
            {
                var simpleService = container.Resolve<ISimpleService>();

                Action action = () =>
                {
                    var random = new Random((int)DateTime.Now.Ticks);
                    var name = "Michael";
                    for (var i = 0; i < 100; i++)
                    {
                        var id = random.Next();
                        var result = simpleService.Test(new SimpleInfo()
                        {
                            Id = id,
                            Name = name
                        }).Result;
                        if (result != (id + "-" + name))
                        {
                            throw new Exception("Error");
                        }

                        var id2 = random.Next();
                        var result2 = simpleService.Test2(new SimpleInfo()
                        {
                            Id = id2,
                            Name = name
                        }).Result;
                        if (result2 != (id2 + "-" + name + "-SimpleService-SimpleService2"))
                        {
                            throw new Exception("Error");
                        }
                    }
                };

                int taskCount = 10;

                do
                {
                    var beginTime = DateTime.Now;

                    Task[] tasks = new Task[taskCount];

                    for (var i = 0; i < taskCount; i++)
                    {
                        tasks[i] = Task.Run(action);
                    }

                    Task.WhenAll(tasks).ContinueWith(task =>
                    {
                        if (task.Exception == null)
                        {
                            System.Console.WriteLine("Success");
                        }
                        else
                        {
                            System.Console.WriteLine("Failed");
                        }
                    }).Wait();

                    var endTime = DateTime.Now;
                    System.Console.WriteLine($"Start time: {beginTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                    System.Console.WriteLine($"End time: {endTime.ToString("yyyy-MM-dd HH:mm:ss")} Used time: {endTime - beginTime}");
                } while (System.Console.ReadLine() == "y");

            }
            catch (RequestTimeoutExcption ex)
            {
                System.Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
            }
            catch (ServiceCallException ex)
            {
                System.Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
