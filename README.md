[TOC]
# XNode ����ָ��

## ���
XNode�ǻ���.Net Standard 2.0�������������ֲ�ʽ�����ܣ����ṩ��ͨ���������뼰�����ü���ʵ�ֲַ�ʽ�����������XNode��ҵ�����������Խ�С��ͨ��һЩ�����Լ���ָ����AttributeӦ���ڷ���ӿڼ�ʵ���Ͼ���ʹXNode�����������У�������������������ֲ�ʽ���ʱֻ��Ҫ����һЩ�������޸ġ�XNode�ṩ�˱���/Զ�̵���͸������������������Ա����һ��XNode��������ñ��ط���û�����𣬵���Ҫ��һ�����ط�����Զ�̻���ʱ��ֻ���޸���Ӧ�����ö�����Ҫ�����д�������κ��޸ġ�XNodeҲ�ṩ��һЩ��չ���Ա㿪����Ա�ܹ�����ʵ����Ҫ�Թ��ܽ�����չ������Ȩ�ޡ�������ٵȡ�

### XNode����˼ܹ�ͼ
<img src="img/Architecture-Server.png" />

### XNode�ͻ��˼ܹ�ͼ
<img src="img/Architecture-Client.png" />

### XNode����ͼ
<img src="img/Deployment.png" />

### XNodeͨ��Э��
<table>
<tr><td style="width: 80px">��ʼ�ֽ�</td><td style="width: 80px">����</td><td>˵��</td></tr>
<tr><td>0</td><td>4</td><td>У���루32λ����δʹ�ã�</td></tr>
<tr><td>4</td><td>4</td><td>��Ϣ�ܳ��ȣ�32λ����������Ϣͷ����Ϣ��</td></tr>
<tr><td>8</td><td>8</td><td>����ID��64λ��</td></tr>
<tr><td>16</td><td>1</td><td>��Ϣ���ͣ�8λ����0��ҵ��������Ϣ��1��ҵ����Ӧ��Ϣ��2��ҵ��ONE WAY��Ϣ��3������������Ϣ��4������Ӧ����Ϣ��5������������Ϣ��6������Ӧ����Ϣ</td></tr>
<tr><td>17</td><td>1</td><td>��Ϣ���ȼ���8λ����δʹ�ã�</td></tr>
<tr><td>18</td><td>4</td><td>������������</td></tr>
<tr><td>22</td><td>�ɱ�</td><td>�������ݣ�������չ��Ϣͷ��key���ȣ�UTF-8�����+key��UTF-8�����+value����+value</td></tr>
<tr><td>22+�������ݳ���</td><td>4</td><td>��Ϣ�峤��</td></tr>
<tr><td>22+�������ݳ���+4</td><td>��Ϣ�峤��</td><td>��Ϣ��</td></tr>
</table>

## ��һ��XNode����
���ڽ�����һ���򵥵�XNode����ʹ�����߶�XNode�и�ֱ�۵���ʶ���������½ڽ���ϸ�ڽ���������

### ʵ��XNode����
���ȴ���һ����ΪServer��.Net Core 2.0����̨��Ŀ���������Ŀ�н�ʵ��XNode����

��Server��Ŀ��ͨ��Nuget�������������
XNode
XNode.Serializer.ProtoBuf
XNode.Communication.DotNetty
Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.Json
Microsoft.Extensions.Logging.Console

����SampleService.cs�ļ����������´��룺
``` c#
[Service("SampleService", 10001, true)]
public interface ISampleService
{
    [Action("Welcome", 1)]
    string Welcome(string name);
}

public class SampleService : ISampleService
{
    public string Welcome(string name)
    {
        return $"Hello {name}";
    }
}
```
���ϴ���ʵ������ΪSampleService�ķ���ӿڣ����ڽӿ���ͨ��Attributeָ��ΪXNode����

����config.json�ļ��������������ã�
``` javascript
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      }
    }
  }
}
```
�����XNode����������ã�XNode������⹫����IP��˿ڣ������߿��Ը����Լ�������ʵ���������������ý����޸ġ�

��Program.cs�ļ����������´��룺
``` c#
class Program
{
    static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        //���÷������־����
        LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Error);

        //���������ļ�
        string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        var configRoot = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        var serverConfig = configRoot.GetServerConfig();

        //���÷���
        var nodeServer = new NodeServerBuilder()
            .ApplyConfig(serverConfig)
            .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
            .ConfigServiceProvider(GetServiceProvider())
            .UseDotNetty(serverConfig.ServerInfo)
            .Build();

        //��������
        nodeServer.StartAsync().Wait();

        Console.ReadLine();

        //�رշ���
        nodeServer.StopAsync();
    }

    private static XNode.Server.IServiceProvider GetServiceProvider()
    {
        //ע�����
        return new DefaultServiceProvider()
            .RegistService(typeof(ISampleService), typeof(SampleService));
    }
}
```
��δ��������˷���ʹ�õ����л�����Լ��Է������ע�Ტ��ʹ��DotNetty��Ϊ�ײ�ͨ�������Ȼ����������������س�����رշ������˷�����ѿ�����ϡ�

### ʵ��XNode����
�����ٴ���һ����ΪClient��.Net Core 2.0����̨��Ŀ���������Ŀ�н�ʵ��һ���ͻ���������XNode����

��Client��Ŀ��ͨ��Nuget������������� 
XNode
XNode.Serializer.ProtoBuf
XNode.Communication.DotNetty
Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.Json
Microsoft.Extensions.Logging.Console

����ISampleService.cs�ļ����������´��룺
``` c#
[ServiceProxy("SampleService", 10001)]
public interface ISampleService
{
    [ActionProxy("Welcome", 1)]
    string Welcome(string name);
}
```
���ϴ���ͨ��XNode�ṩ��Attributeʵ����SampleService�Ĵ���ӿڡ�

����config.json�ļ��������������ã�
``` javascript
{
  "xnode": {
    "client": {
      "serviceProxies": [
        {
          "proxyName": "SampleService",
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Client.ISampleService,Client",
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
���϶�XNode������������ã�ָ��SampleService���ڵķ�����IP��˿ڣ������˷���IdΪ10001�Ĵ����ಢ����Ϊ����״̬��connections���ýڿ������ö��������Ϣ��Ĭ�������XNode���������������Щ���������𵽸��ؾ�������á�

��Program.cs�ļ����������´��룺
``` c#
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Please input enter to begin.");
        Console.ReadLine();

        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        //���ÿͻ�����־����
        LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Error);

        //���������ļ�
        string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        var configRoot = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        var clientConfig = configRoot.GetClientConfig();

        var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

        var serviceProxyManager = new ServiceProxyManager();

        if (clientConfig.ServiceProxies != null)
        {
            //ע��������
            foreach (var config in clientConfig.ServiceProxies)
            {
                serviceProxyManager
                    .Regist(config)
                    .AddClients(
                        new NodeClientBuilder()
                            .ConfigConnections(config.Connections)
                            .ConfigSerializer(serializer)
                            .UseDotNetty()
                            .Build()
                    );
            }
        }

        try
        {
            //���ӷ���
            serviceProxyManager.ConnectAsync().Wait();
        }
        catch (AggregateException ex)
        {
            foreach (var e in ex.InnerExceptions)
            {
                if (e is NetworkException netEx)
                {
                    Console.WriteLine($"Connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
                }
                else
                {
                    throw e;
                }
            }
        }

        try
        {
            //���÷���
            var serviceProxy = serviceProxyManager.GetServiceProxy(typeof(ISampleService));
            var result = serviceProxy.CallRemoteServiceAsync(GetSampleServiceActionType("Welcome"), new object[] { "XNode" }).Result as string;
            Console.WriteLine(result);
        }
        catch (RequestTimeoutExcption ex)
        {
            Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
        }
        catch (ServiceCallException ex)
        {
            Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.ReadLine();

        //�رշ�������
        serviceProxyManager.CloseAsync();
    }

    private static MethodInfo GetSampleServiceActionType(string methodName)
    {
        var serviceType = typeof(ISampleService);
        return serviceType.GetMethod(methodName);
    }
}
```
��δ���ע���˷������Ȼ�����ӷ��񲢽��е��ã�������س�����ر����ӡ����˿ͻ���Ҳ��������ˡ�

### ����
��Server��Client���б��벢������Ȼ����Client������س��󽫻��Server�е�XNode������е��ã���������������Client��ʾ���񷵻صĽ����Hello XNode��

### ����ʾ������
XNode-Sample/01-QuickStart

## ʹ��AOP����ʹ�������͸����
��ǰ���ʾ���У��ͻ��˵�XNode����Ĺ��̵��б�¶��һЩ����ϸ�ڣ���Ȼ��Щϸ�ڱȽϷ����Ҳ�Ӧ�ó�����ҵ������С�ͨ��ʹ��AOP�������Խ���Щϸ�ڷ�װ������ʹ����ҵ��������XNode��������ñ��ؽӿڷ���һ����.Netƽ̨���ж���֧��AOP�����XNodeĬ���ṩ�˶�Autofac��֧�֣����濴һ�����ʹ��Autofac�Ľ�XNode������á�

### ʹ��Autofac�Ľ�XNode����
������������һ��ʾ���Ļ������иĽ���Server���ֵĴ��벻��Ҫ�����κ��޸ģ�Client������Ҫͨ��Nuget�����µ������
XNode.Autofac

����һ�����ļ�SampleService.cs��
``` c#
public class SampleService : ISampleService
{
    public string Welcome(string name)
    {
        throw new NotImplementedException();
    }
}
```
�ڱ����У�����಻��Ҫ�κ�ʵ�֣���ֻ�Ǳ�����ע�ᵽAutofac�����С���ʵ����Ŀ���������У����԰�����ҵ���߼�ʵ�������У��Ա��ڵ��÷���ʱʵ�ֱ���/Զ��͸��������ϸ������ں������½���������

��Program.cs�ļ��������ڴ���ServiceProxyManagerʵ��֮����ϴ���Autofac�����Ĵ��룺
``` c#
var serviceProxyManager = new ServiceProxyManager();

//����Autofac������ע���������
var container = GetAutofacContainer(serviceProxyManager);
```
GetAutofacContainer�������ڴ���Autofac���������ע�ᣬ���������������ʵ�֣�
``` c#
private static IContainer GetAutofacContainer(IServiceProxyManager serviceProxyManager)
{
    var builder = new ContainerBuilder();
    builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
    builder.RegisterType<SampleService>()
        .As<ISampleService>()
        .EnableInterfaceInterceptors()
        .InterceptedBy(typeof(ServiceProxyInterceptor))
        .SingleInstance();

    var container = builder.Build();
    return container;
}
```
����������У�ServiceProxyInterceptor��ʵ����Autofac�������ӿڡ��������������ʱͨ���������Լ����ÿ��Ծ���������õ��Ǳ���ʵ�ֻ���Զ�̷��񡣽���ע�ᵽAutofac������������ΪXNode�������Ľӿڶ���Ҫͨ��InterceptedBy����ָ����������

��󣬿�һ�¸Ľ�����ε���XNode����
``` c#
try
{
    //���÷���
    var sampleService = container.Resolve<ISampleService>();
    var result = sampleService.Welcome("XNode");
    Console.WriteLine(result);
}
catch (RequestTimeoutExcption ex)
{
    Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
}
catch (ServiceCallException ex)
{
    Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```
���������ʾ����ʱ����XNode��������ñ��ؽӿڷ����Ѿ�û�����𣬺�����½ڻ������ͨ�����������л����ñ���ʵ�ֻ�Զ��XNode����

### ����
��Server��Client���б��벢������Ȼ����Client������س��󽫻��Server�е�XNode������е��ã���������������Client��ʾ���񷵻صĽ����Hello XNode��

### ����˶�Autofac��֧��
ǰ����ʾ��XNode�ͻ��˴������ʹ��Autofac��AOP���ܼ򵥻��Է���ĵ��á�ͬ����XNode�����Ҳ�ṩ�˶�Autofac��֧�֣�������Աֻ��Ҫ������ע�ᵽAutofac�����У�����Ҫ�ظ�����XNode���ٴ�ע�ᡣ����XNode����˶�Autofac��֧��ֻ��Ҫ�����÷�������ʱ�����UseAutofac�������ɣ�
``` c#
......
//���÷���
var nodeServer = new NodeServerBuilder()
    .ApplyConfig(serverConfig)
    .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
    //.ConfigServiceProvider(GetServiceProvider())      //Ĭ�ϵķ���ע�᲻��Ҫ��
    .UseAutofac(container, LoggerManager.ServerLoggerFactory)      //����Autofac֧��
    .UseDotNetty(serverConfig.ServerInfo)
    .Build();
......
```
������ȥ����Ĭ�ϵķ���ע�ᣬ�����˶�UseAutofac�����ĵ��ò�����Autofac����������XNode����˻��Զ���Autofac�������ҵ�XNode���񲢽���ע�ᡣ

### ����ʾ������
XNode-Sample/02-AOP

## ʵ��
��ǰ���ʾ���У������뷵��ֵ���Ǽ����ͣ�XNodeҲ֧�ִ��ݸ���������Ϊ�����뷵��ֵ�����濴һ�����ʹ�ø���������Ϊ�����뷵��ֵ��

### ������Ϊ������ʵ��
��Ȼ����ǰ���ʾ������Ϊʵ��Ķ�����ڷ���˺Ϳͻ�����һ���ģ������½�һ����ΪEntity��Ŀ�����½�һ����ΪName���ࡣ
``` c#
[DataContract]
public class Name
{
    [DataMember(Order = 1)]
    public string FirstName { get; set; }

    [DataMember(Order = 2)]
    public string LastName { get; set; }
}
```
��ʵ�����������Ϸֱ�ʹ��XNode���л�������ʶ���Attribute��ĿǰXNode֧�ֵ����л���ProtoBuf��MsgPack����ʶ��DataContract��DataMember����ʹ�����������л������ܻ�������ͬ��DataMember��Order����Ϊ���ж��������л�ʱ��˳��Ϊ��ͬʱ������2����������Ա���δ�������޷��滻�����1��Ϊ��ʼ�������á�

�޸ķ���˽ӿ��������룬ʹ��Name����Ϊ������
``` c#
[Service("SampleService", 10001, true)]
public interface ISampleService
{
    [Action("Welcome", 1)]
    string Welcome(Name name);
}

public class SampleService : ISampleService
{
    public string Welcome(Name name)
    {
        return $"Hello {name.FirstName} {name.LastName}";
    }
}
```
��Ӧ��Ҳ�޸Ŀͻ��˴��룺
``` c#
[ServiceProxy("SampleService", 10001)]
public interface ISampleService
{
    [ActionProxy("Welcome", 1)]
    string Welcome(Name name);
}

public class SampleService : ISampleService
{
    public string Welcome(Name name)
    {
        throw new NotImplementedException();
    }
}
```
���Է�����ô�������޸ģ�
``` c#
//���÷���
var sampleService = container.Resolve<ISampleService>();
var result = sampleService.Welcome(new Name() { FirstName = "Michael", LastName = "Sun" });
Console.WriteLine(result);
```
��Server��Client���б��벢������Ȼ����Client������س��󽫻��Server�е�XNode������е��ã���������������Client��ʾ���񷵻صĽ����Hello Michael Sun��

### ������Ϊ����ֵ��ʵ��
����ֵʵ�������ʵ��Ĵ�������һ����������Entity��Ŀ���½�2����Order��OrderDetail��
``` c#
[DataContract]
public class Order
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public int CustomerId { get; set; }

    [DataMember(Order = 3)]
    public string CustomerName { get; set; }

    [DataMember(Order = 4)]
    public List<OrderDetail> Detail { get; set; }
}

[DataContract]
public class OrderDetail
{
    [DataMember(Order = 1)]
    public int OrderId { get; set; }

    [DataMember(Order = 2)]
    public int GoodsId { get; set; }

    [DataMember(Order = 3)]
    public string GoodsName { get; set; }

    [DataMember(Order = 4)]
    public decimal Price { get; set; }

    [DataMember(Order = 5)]
    public int Amount { get; set; }
}
```
��Server��Ŀ�е�ISampleService�ӿ�������GetOrders������
``` c#
[Service("SampleService", 10001, true)]
public interface ISampleService
{
    [Action("Welcome", 1)]
    string Welcome(Name name);

    [Action("GetOrders", 2)]
    Task<IList<Order>> GetOrders();
}
```
ͬʱ��SampleService����ʵ�����������
``` c#
public Task<IList<Order>> GetOrders()
{
    return Task.FromResult<IList<Order>>(new List<Order>()
    {
        new Order()
        {
            Id = 1,
            CustomerId = 1,
            CustomerName = "Customer1",
            Detail = new List<OrderDetail>()
            {
                new OrderDetail()
                {
                    OrderId = 1,
                    GoodsId = 1,
                    GoodsName = "A",
                    Price = 12,
                    Amount = 10
                },
                new OrderDetail()
                {
                    OrderId = 1,
                    GoodsId = 2,
                    GoodsName = "B",
                    Price = 26.5M,
                    Amount = 1
                },
                new OrderDetail()
                {
                    OrderId = 1,
                    GoodsId = 3,
                    GoodsName = "C",
                    Price = 5.5M,
                    Amount = 15
                }
            }
        },
        new Order()
        {
            Id = 2,
            CustomerId = 2,
            CustomerName = "Customer2",
            Detail = new List<OrderDetail>()
            {
                new OrderDetail()
                {
                    OrderId = 2,
                    GoodsId = 1,
                    GoodsName = "A",
                    Price = 12,
                    Amount = 3
                }
            }
        }
    });
}
```
�ͻ�����Ӧ�Ĵ���ӿ�����Ҳ������Ӧ�޸ģ�
``` c#
[ServiceProxy("SampleService", 10001)]
public interface ISampleService
{
    [ActionProxy("Welcome", 1)]
    string Welcome(Name name);

    [ActionProxy("GetOrders", 2)]
    Task<IList<Order>> GetOrders();
}

public class SampleService : ISampleService
{
    public string Welcome(Name name)
    {
        throw new NotImplementedException();
    }

    public Task<IList<Order>> GetOrders()
    {
        throw new NotImplementedException();
    }
}
```
�����Program.cs��������Ӧ�ķ�����ô��룺
``` c#
//����GetOrders����
var orders = sampleService.GetOrders().Result;
foreach (var order in orders)
{
    Console.WriteLine($"OrderId={order.Id}, CustomerId={order.CustomerId}, CustomerName={order.CustomerName}");
    foreach (var detail in order.Detail)
    {
        Console.WriteLine($"GoodsId={detail.GoodsId}, GoodsName={detail.GoodsName}, Price={detail.Price}, Amount={detail.Amount}");
    }
    Console.WriteLine("----------------------------------------------------------------");
}
```
�Խ���������б��벢������Ȼ����Client������س��󽫻��Server�е�XNode������е��ã���������������Client��ʾGetOrders���񷵻صĽ����
``` c#
OrderId=1, CustomerId=1, CustomerName=Customer1
GoodsId=1, GoodsName=A, Price=12, Amount=10
GoodsId=2, GoodsName=B, Price=26.5, Amount=1
GoodsId=3, GoodsName=C, Price=5.5, Amount=15
----------------------------------------------------------------
OrderId=2, CustomerId=2, CustomerName=Customer2
GoodsId=1, GoodsName=A, Price=12, Amount=3
----------------------------------------------------------------
```

### ����ʾ������
XNode-Sample/03-Entity

## ���л�
XNode�ķ������ͻ������Գ����Ӳ�����TCPЭ�����ͨ�ţ������Ҫͨ�����л�������������л�����д��䡣XNode���������л�����ӿڣ�ֻҪʵ���˴˽ӿ�XNode�Ϳ��԰��տ�����Ա��ϣ���ķ�ʽ�������л��������ӿڶ������£�
``` c#
/// <summary>
/// ���л����ӿ�
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// ���л�������
    /// </summary>
    string Name { get; }

    /// <summary>
    /// ִ�����л�����
    /// </summary>
    /// <param name="obj">���л������Ķ���</param>
    /// <returns></returns>
    Task<byte[]> SerializeAsync(object obj);

    /// <summary>
    /// ִ�з����л�����
    /// </summary>
    /// <param name="type">�����л���Ŀ������</param>
    /// <param name="data">�����л������Ķ���������</param>
    /// <returns></returns>
    Task<object> DeserializeAsync(Type type, byte[] data);
}
```
Ŀǰ��XNode֧�ֵ����л��������ProtoBuf(������XNode.Serializer.ProtoBuf)��MsgPack(������XNode.Serializer.MsgPack)�����������Աϣ��ʹ�����������л��������ͨ��ʵ��ISerializer�ӿڽ������䡣ǰ���ʾ��������ProtoBuf�������л������潫��ʾ��03-EntityΪ���������޸�����ʾʹ��MsgPack�������л���

### �޸ķ�������л����
�����Ƴ�Server��Ŀ�е�Nuget����XNode.Serializer.ProtoBuf,������XNode.Serializer.MsgPack��Ȼ���Program.cs�ļ��������л����ô�ProtoBufSerializer��ΪMsgPackSerializer��
``` c#
//���÷���
var nodeServer = new NodeServerBuilder()
    .ApplyConfig(serverConfig)
    .ConfigSerializer(new MsgPackSerializer(LoggerManager.ServerLoggerFactory))
    .ConfigServiceProvider(GetServiceProvider())
    .UseDotNetty(serverConfig.ServerInfo)
    .Build();
```

### �޸Ŀͻ������л����
ͬ�����Ƴ�Client��Ŀ�е�Nuget����XNode.Serializer.ProtoBuf,������XNode.Serializer.MsgPack��Ȼ���Program.cs�ļ��������л����ô�ProtoBufSerializer��ΪMsgPackSerializer��
``` c#
var serializer = new MsgPackSerializer(LoggerManager.ClientLoggerFactory);

var serviceProxyManager = new ServiceProxyManager();

//����Autofac������ע���������
var container = GetAutofacContainer(serviceProxyManager);

if (clientConfig.ServiceProxies != null)
{
    //ע��������
    foreach (var config in clientConfig.ServiceProxies)
    {
        serviceProxyManager
            .Regist(config)
            .AddClients(
                new NodeClientBuilder()
                    .ConfigConnections(config.Connections)
                    .ConfigSerializer(serializer)
                    .UseDotNetty()
                    .Build()
            );
    }
}
```
�������ͽ�XNode�����л�������ΪMsgPack�ˡ�

### ����
��Server��Client���б��벢������Ȼ����Client������س��󽫻��Server�е�XNode������е��ã���������������Client��ʾ���񷵻صĽ����
``` c#
Hello Michael Sun

OrderId=1, CustomerId=1, CustomerName=Customer1
GoodsId=1, GoodsName=A, Price=12, Amount=10
GoodsId=2, GoodsName=B, Price=26.5, Amount=1
GoodsId=3, GoodsName=C, Price=5.5, Amount=15
----------------------------------------------------------------
OrderId=2, CustomerId=2, CustomerName=Customer2
GoodsId=1, GoodsName=A, Price=12, Amount=3
----------------------------------------------------------------
```

### ����ʾ������
XNode-Sample/04-Serializer

## ����ʹ�������������
XNode�����������������������úͽ��á�����XNode������Խ�������Service��Ҳ���Խ���Service�е�ĳһ��Action��ͬ��������XNode����Ҳ����ͨ����������ù���ʹ�������ڵ��ñ��ش�����Զ�̴���֮������л���������ڿ����׶ν��г�����Ժ��а�����ͬʱ����һЩ��Ŀ�ڳ���ҵ���������������£����з�����Բ�����ͬһ̨��������������ҵ������������ʹ�÷ֲ�ʽ���𡣱��ڽ�������ζԷ���ʹ���������������ã����ڴ��뽫����02-AOP�����Ͻ����޸ġ�

### ��������/����
XNode�������ͨ��Attribute�������ļ��ķ�ʽ������������ã������ļ��ķ�ʽ���ȼ�����Attribute�ķ�ʽ��

#### ʹ��Attribute�ķ�ʽ����
Attribute�����÷�ʽ����ͨ��ServiceAttribute��ActionAttributeʵ�֣�ServiceAttribute�����Service�������ã���ActionAttribute�������ĳһ��Action�������á�
�ȿ�һ��ServiceAttribute��ԭ�ͣ�
``` c#
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ServiceAttribute : Attribute
{
    public ServiceAttribute(int serviceId, bool enabled = false);
    public ServiceAttribute(string name, int serviceId, bool enabled = false);

    public string Name { get; }
    public int ServiceId { get; }
    public bool Enabled { get; }
}
```
�ӹ��캯������������enabled��Ĭ��ֵΪfalse��������û����ʽ�����ò���enabledΪtrue������Ĭ���ǲ������ģ������޸�һ��Server��Ŀ�нӿ�ISampleService�Ĵ����Ƴ�����enabled��
``` c#
[Service("SampleService", 10001)]
public interface ISampleService
{
    [Action("Welcome", 1)]
    string Welcome(string name);
}
```
���벢���к�ͻ��˻�õ������쳣��Ϣ��
``` c#
Service call exception: ExceptionId=-10003, ExceptionMessage=Service is disabled.
fail: XNode.Client.NodeClient[0]
      Node server has an error, Host=10.246.84.201, Port=9001, ServiceId=10001, ActionId=1, ExceptionId=-10003, ExceptionMessage=Service is disabled.
```
����ΪService��������/���ã�Ҳ����Ϊĳ��������Action�������á���Ҫע����ǣ�һ��Action�Ƿ�����ȡ����Service��Enabled��Action��Enabled�Ľ�����Ҳ����˵��������Service��EnabledΪfalse������Action������ã����Service�µ�����Action���������á���ˣ��Ƚ�ServiceAttribute��enabled������ԭΪtrue��
``` c#
[Service("SampleService", 10001, true)]
public interface ISampleService
{
    [Action("Welcome", 1)]
    string Welcome(string name);
}
```
��ʱ���벢���н����������У���ΪActionAttribute��enabled����Ĭ��ֵΪtrue������ΪActionAttribute��ԭ�ͣ�
``` c#
[AttributeUsage(AttributeTargets.Method)]
public class ActionAttribute : Attribute
{
    public ActionAttribute(int actionId, bool enabled = true);
    public ActionAttribute(string name, int actionId, bool enabled = true);

    public string Name { get; }
    public int ActionId { get; }
    public bool Enabled { get; }
}
```
���ڣ���ActionAttribute��enable����Ϊfalse��
``` c#
[Service("SampleService", 10001, true)]
public interface ISampleService
{
    [Action("Welcome", 1, false)]
    string Welcome(string name);
}
```
���벢���к�ͻ��˻�õ������쳣��Ϣ��
``` c#
Service call exception: ExceptionId=-10003, ExceptionMessage=Service is disabled.
fail: XNode.Client.NodeClient[0]
      Node server has an error, Host=10.246.84.201, Port=9001, ServiceId=10001, ActionId=1, ExceptionId=-10003, ExceptionMessage=Service is disabled.
```

#### ʹ�������ļ��ķ�ʽ����
��ʵ����Ŀ�У�ֱ��ͨ��Attribute���÷�������/���ò�̫�����Կ���ͨ�������ļ��ķ�ʽ�Է���������á������ļ��еķ�������/�������ûḲ��Attribute�е����á������Ƚ�Attribute��enabled�����������Ƴ���ֻʹ��Attribute��Ĭ��ֵ��
``` c#
[Service("SampleService", 10001)]
public interface ISampleService
{
    [Action("Welcome", 1)]
    string Welcome(string name);
}
```
��config.json������services���ýڣ�
``` c#
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      },
      "services": [
        {
          "serviceId": 10001,
          "enabled": true
        }
      ]
    }
  }
}
```
services���ý���һ�����飬�����������������е�XNode����serviceIdΪ����Id��ServiceAttribute�е�serviceId��Ӧ��enabled���������Ƿ�����ָ��Service�����벢���к󣬿ͻ��˽��ɹ�����XNode����

���ý��е�enabledĬ��ֵ��ServiceAttribute��ActionAttribute��һ����ServiceĬ��Ϊfalse��ActionĬ��Ϊtrue����ˣ����ﲢû�ж�Action��enabled�������ã������޸�Action�����ý�����ã�
``` c#
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      },
      "services": [
        {
          "serviceId": 10001,
          "enabled": true,
          "actions": [
            {
              "actionId": 1,
              "enabled": false
            }
          ]
        }
      ]
    }
  }
}
```
actions���ý�ͬ��Ϊ���飬ĳһ��Service�����е�Action�������������ã�actionIdΪActionId��ActionAttribute��actionId��Ӧ��enabled���������Ƿ�����ָ��Action�����벢���к�ͻ��˻�õ������쳣��Ϣ��
``` c#
Service call exception: ExceptionId=-10003, ExceptionMessage=Service is disabled.
fail: XNode.Client.NodeClient[0]
      Node server has an error, Host=10.246.84.201, Port=9001, ServiceId=10001, ActionId=1, ExceptionId=-10003, ExceptionMessage=Service is disabled.
```

### ��������/����
XNode������XNode����һ������ͨ��Attribute�������ļ��ķ�ʽ������������ã������ļ��ķ�ʽ���ȼ�����Attribute�ķ�ʽ���ڿ�ʼ��ʾ��������/����֮ǰ�Ƚ�Server�еķ������û����á�

#### ʹ��Attribute�ķ�ʽ����
ĿǰClient��Ŀ�еĴ������������ļ���ʽע��������ͣ�Ҫ��ʾAttribute��ʽ���ô�������/���ã���Ҫ�������ļ���ʽ��Ϊ��̷�ʽע��������͡����Ƴ������ļ���ע��������Ͳ���--services���ýڣ��޸ĺ�������ļ����£�
``` c#
{
  "xnode": {
    "client": {
      "serviceProxies": [
        {
          "proxyName": "SampleService",
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ]
        }
      ]
    }
  }
}
```
Ȼ�󣬽�ԭ����������ע���ʵ��ע�͵����������µ�ע����룺
``` c#
var clientConfig = configRoot.GetClientConfig();

if (clientConfig.ServiceProxies != null)
{
    //ע��������
    foreach (var config in clientConfig.ServiceProxies)
    {
        serviceProxyManager
            .Regist("SampleService", new List<Type>() { typeof(ISampleService) })
            .AddClients(
                new NodeClientBuilder()
                    .ConfigConnections(config.Connections)
                    .ConfigSerializer(serializer)
                    .UseDotNetty()
                    .Build()
            );
    }

    //ע�������������ļ���ʽ��
    //foreach (var config in clientConfig.ServiceProxies)
    //{
    //    serviceProxyManager
    //        .Regist(config)
    //        .AddClients(
    //            new NodeClientBuilder()
    //                .ConfigConnections(config.Connections)
    //                .ConfigSerializer(serializer)
    //                .UseDotNetty()
    //                .Build()
    //        );
    //}
}
```
��󣬽�ISampleService�ϵ�ServiceProxyAttribute����enabledΪtrue��
``` c#
[ServiceProxy("SampleService", 10001, true)]
public interface ISampleService
{
    [ActionProxy("Welcome", 1)]
    string Welcome(string name);
}
```
���벢���к󣬿ͻ��˽���������XNode����

���������/����һ������ServiceProxyAttribute��enabled��ActionProxyAttribute��enabled����Ϊfalse�Խ��ô�����ĳ��Action��EnabledΪfalse�����ͨ��IServiceProxy.CallRemoteServiceAsync���ô�Action������InvalidOperationException�쳣���������XNode.Autofacͨ��AOP��ʽ���ý�����ñ���ʵ�֡�Ŀǰ�����ǵ�ʾ����ͨ��AOP�������õģ�������ʵ�����£�
``` c#
public class SampleService : ISampleService
{
    public string Welcome(string name)
    {
        throw new NotImplementedException();
    }
}
```
���棬���ǽ�Action����Ϊfalse��
``` c#
[ServiceProxy("SampleService", 10001, true)]
public interface ISampleService
{
    [ActionProxy("Welcome", 1, false)]
    string Welcome(string name);
}
```
���벢���к�ͻ��˵��õ��Ǳ���ʵ�֣������ʾ���£�
``` c#
Error: The method or operation is not implemented.
```

#### ʹ�������ļ��ķ�ʽ����
��������ʾһ��ʹ�������ļ��ķ�ʽ���Ƚ�ServiceAttribute��ActionAttribute��enabled�����Ƴ���
``` c#
[ServiceProxy("SampleService", 10001)]
public interface ISampleService
{
    [ActionProxy("Welcome", 1)]
    string Welcome(string name);
}
```
�����ļ�������ע���������������ò�����enabled��
``` c#
{
  "xnode": {
    "client": {
      "serviceProxies": [
        {
          "proxyName": "SampleService",
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Client.ISampleService,Client",
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
Program.cs�е���ش���Ҳ�������ã�
``` c#
var clientConfig = configRoot.GetClientConfig();

if (clientConfig.ServiceProxies != null)
{
    //ע��������
    //foreach (var config in clientConfig.ServiceProxies)
    //{
    //    serviceProxyManager
    //        .Regist("SampleService", new List<Type>() { typeof(ISampleService) })
    //        .AddClients(
    //            new NodeClientBuilder()
    //                .ConfigConnections(config.Connections)
    //                .ConfigSerializer(serializer)
    //                .UseDotNetty()
    //                .Build()
    //        );
    //}

    //ע�������������ļ���ʽ��
    foreach (var config in clientConfig.ServiceProxies)
    {
        serviceProxyManager
            .Regist(config)
            .AddClients(
                new NodeClientBuilder()
                    .ConfigConnections(config.Connections)
                    .ConfigSerializer(serializer)
                    .UseDotNetty()
                    .Build()
            );
    }
}
```
���벢���У��ͻ��˽���������XNode����
ͬ�����������ļ��е�Action����enabledΪfalse�������ô�����ñ���ʵ�֣�
``` c#
{
  "xnode": {
    "client": {
      "serviceProxies": [
        {
          "proxyName": "SampleService",
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Client.ISampleService,Client",
              "enabled": true,
              "actions": [
                {
                  "actionId": 1,
                  "enabled": false
                }
              ]
            }
          ]
        }
      ]
    }
  }
}
```

### ����ʾ������
XNode-Sample/05-Enabled

## ���������֤
XNode�ṩ�������֤�������ڶԵ���XNode����Ŀͻ��˽��������֤����������ڽ��ܿͻ�����������󣬿ͻ�����Ҫ�ṩ�Ϸ��������Ϣ�������������XNode����XNode������ṩ��ILoginValidator�ӿ����������֤������ΪILoginValidator�ӿڶ��壺
``` c#
/// <summary>
/// ��¼��֤�ӿ�
/// </summary>
public interface ILoginValidator
{
    /// <summary>
    /// ���л���
    /// </summary>
    ISerializer Serializer { set; }

    /// <summary>
    /// ��¼��֤
    /// </summary>
    /// <param name="loginInfo">��¼��Ϣ</param>
    /// <returns></returns>
    Task<LoginAuthResult> Validate(LoginRequestInfo loginInfo);
}
```
XNode�ͻ���Ҳ�ṩ����Ӧ�Ľӿ�ILoginHandler�����ύ��¼��Ϣ�Լ������¼���������ΪILoginHandler�ӿڶ��壺
``` c#
/// <summary>
/// ��¼������
/// </summary>
public interface ILoginHandler
{
    /// <summary>
    /// ��ȡ��¼��Ϣ
    /// </summary>
    /// <returns></returns>
    Task<LoginInfo> GetLoginInfo();

    /// <summary>
    /// ��¼��֤��Ӧ����
    /// </summary>
    /// <param name="loginResponseInfo">��¼��֤��Ӧ��Ϣ</param>
    /// <returns>��¼��֤״̬�루��0��ʾ��֤ʧ�ܣ�1-30ΪXNode����״̬�룩</returns>
    Task<byte> LoginResponseHandle(LoginResponseInfo loginResponseInfo);
}
```
������Ա����ͨ��ʵ����2���ӿ������������֤�߼���

### ʹ��Ĭ�������֤
XNode�����������֤��Ĭ��ʵ�֣���ʵ���ṩ�˻����û��������롢IP����������֤��ʽ������������ʹ��XNodeĬ�ϵ������֤������ʾ������ʾ��05-Enabled�����޸ģ��Ƚ�05-Enabled�����з�������������Ϊ���á�
����˳�ʼ�������£�
``` c#
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      },
      "services": [
        {
          "serviceId": 10001,
          "enabled": true,
          "actions": [
            {
              "actionId": 1,
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
�ͻ��˳�ʼ�������£�
``` c#
{
  "xnode": {
    "client": {
      "serviceProxies": [
        {
          "proxyName": "SampleService",
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Client.ISampleService,Client",
              "enabled": true,
              "actions": [
                {
                  "actionId": 1,
                  "enabled": true
                }
              ]
            }
          ]
        }
      ]
    }
  }
}
```
���ȣ��ڷ���˵������ļ��м����¼��֤��ص����ýڡ���security��
``` c#
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      },
      "services": [
        {
          "serviceId": 10001,
          "enabled": true,
          "actions": [
            {
              "actionId": 1,
              "enabled": true
            }
          ]
        }
      ],
      "security": {
        "loginValidator": {
          "accounts": [
            {
              "accountName": "Test01",
              "accountKey": "123456",
              "ipWhiteList": ["10.246.84.201"]
            }
          ]
        }
      }
    }
  }
}
```
������accountsΪ���Է��ʵ�ǰXNode������˺���Ϣ������accountNameΪ�˺�����accountKeyΪ��Կ��ipWhiteListΪIP�������б����������ipWhiteList�򲻽���IP��������֤��
Ȼ�󣬴�Server��ĿProgram.cs��Ĭ�ϵ�¼��֤���õ�XNode������У�
``` c#
......
var loginValidator = new DefaultLoginValidator(configRoot.GetDefaultLoginValidatorConfig(), LoggerManager.ServerLoggerFactory);     //����LoginValidatorʵ��

//���÷���
var nodeServer = new NodeServerBuilder()
    .ApplyConfig(serverConfig)
    .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
    .ConfigLoginValidator(loginValidator)       //����LoginValidator
    .ConfigServiceProvider(GetServiceProvider())
    .UseDotNetty(serverConfig.ServerInfo)
    .Build();
......
```
���ڿͻ���ҲҪ������Ӧ�����ã������������ļ��м����¼��֤��ص����ýڡ���security��
``` c#
{
  "xnode": {
    "client": {
      "serviceProxies": [
        {
          "proxyName": "SampleService",
          "security": {
            "login": {
              "accountName": "Test01",
              "accountKey": "123456"
            }
          },
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Client.ISampleService,Client",
              "enabled": true,
              "actions": [
                {
                  "actionId": 1,
                  "enabled": true
                }
              ]
            }
          ]
        }
      ]
    }
  }
}
```
Ȼ�󣬴�Client��ĿProgram.cs��Ĭ�ϵ�¼���������õ�XNode�ͻ����У�
``` c#
......
if (clientConfig.ServiceProxies != null)
{
    //ע��������
    foreach (var config in clientConfig.ServiceProxies)
    {
        serviceProxyManager
            .Regist(config)
            .AddClients(
                new NodeClientBuilder()
                    .ConfigConnections(config.Connections)
                    .ConfigSerializer(serializer)
                    .ConfigLoginHandler(new DefaultLoginHandler(configRoot.GetDefaultLoginHandlerConfig(config.ProxyName), serializer))     //���õ�¼������
                    .UseDotNetty()
                    .Build()
            );
    }
}
......
```
���벢���к�ͻ��˽��ɹ�����XNode����������ͻ��˵ĵ�¼��Կ�޸�Ϊ�������Կ�򽫷���˵�IP�������б��޸�Ϊ����IP�����к�ͻ��˽����׳�LoginAuthException�쳣��

### ����ʾ������
XNode-Sample/06-LoginValidate

## ������Ȩ
XNode������ṩ�˷�����Ȩ���ƣ����Զ�ÿһ��Action���е�����Ȩ��������Ա����ͨ��ʵ��IServiceAuthorizer�ӿڡ�����ServiceProcessor��XNode����˵���չ�㣬�����½ڻ���н��ܣ������Ʒ�����Ȩ�߼������ϵ�������Ȩϵͳ��IServiceAuthorizer�ӿڶ������£�
``` c#
/// <summary>
/// ������Ȩ��֤�ӿ�
/// </summary>
public interface IServiceAuthorizer
{
    /// <summary>
    /// ��Ȩ��֤
    /// </summary>
    /// <param name="context">����������</param>
    /// <param name="serviceId">����Id</param>
    /// <param name="actionId">ActionId</param>
    /// <param name="attachments">��������</param>
    Task Validate(ServiceContext context, int serviceId, int actionId, IDictionary<string, byte[]> attachments);
}
```
### ʹ��Ĭ�Ϸ�����Ȩ
��ǰ�������ܵ������֤һ����XNodeҲ��������Ȩ��Ĭ��ʵ�֣��ṩ�˼򵥵ķ�����Ȩ���ܡ��������Ĭ�Ϸ�����Ȩ��ʹ�÷�������ʾ������06-LoginValidate�����޸ġ�

���ȣ���Server��Ŀ�������ļ�����actions���ý�������authorizes���ýڣ�
``` c#
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      },
      "services": [
        {
          "serviceId": 10001,
          "enabled": true,
          "actions": [
            {
              "actionId": 1,
              "enabled": true,
              "authorizes": [
                {
                  "account": "Test01",
                  "dateLimit": "2018-01-01~2030-12-31",
                  "timeLimit": "9:30~19:00"
                }
              ]
            }
          ]
        }
      ],
      "security": {
        "loginValidator": {
          "accounts": [
            {
              "accountName": "Test01",
              "accountKey": "123456",
              "ipWhiteList": [ "10.246.84.201" ]
            }
          ]
        }
      }
    }
  }
}
```
authorizes���ý���һ�����飬��ʾ���б�ʾActionIdΪ1�ķ���������Ȩ���˺�Test01�������ƿɷ�������Ϊ2018��1��1����2030��12��31�գ��Լ�����ÿ�����ʱ���Ϊ9��30����19�㡣�������ƺ�ʱ������Ҳ����ֻ������ʼ�����������"2018-01-01~"��ʾ2018��1��1�տ�ʼ�����޻�"~2030-12-31"��ʾ��2020��12��31��Ϊֹ��

Ȼ�󣬴�Server��Ŀ��Program.cs���ӷ�����Ȩ��������ã�
``` c#
......
var loginValidator = new DefaultLoginValidator(configRoot.GetDefaultLoginValidatorConfig(), LoggerManager.ServerLoggerFactory);
var serviceAuthorizer = new DefaultServiceAuthorizer(configRoot.GetDefaultServiceAuthorizeConfig(), LoggerManager.ServerLoggerFactory);     //����Ĭ�Ϸ�����Ȩʵ��

//���÷���
var nodeServer = new NodeServerBuilder()
    .ApplyConfig(serverConfig)
    .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
    .ConfigLoginValidator(loginValidator)
    .AddServiceProcessor(new ServiceAuthorizeProcessor(serviceAuthorizer))      //����Ĭ�Ϸ�����Ȩ
    .ConfigServiceProvider(GetServiceProvider())
    .UseDotNetty(serverConfig.ServerInfo)
    .Build();
......
```
���벢���У��ͻ��˽���������XNode�����������Ȩ�����޸�һ�£�������˺Ÿ�ΪTest02�����벢���к�ͻ��˽��յ������쳣��Ϣ��
``` c#
Service call exception: ExceptionId=-10005, ExceptionMessage=Service no authorize.
fail: XNode.Client.NodeClient[0]
      Node server has an error, Host=10.246.84.201, Port=9001, ServiceId=10001, ActionId=1, ExceptionId=-10005, ExceptionMessage=Service no authorize.
```
ͬ������������ʱ�������޸�Ϊ�����ϵ���������ֵҲ���յ���Ӧ���쳣��Ϣ��

### ����ʾ������
XNode-Sample/07-ServiceAuthorize

## ����׷��
����׷�ٶ��ڷֲ�ʽ��������˵�Ƿǳ���Ҫ����ɲ��֣�ͨ������׷�ٿ���֪��һ�η�����þ�����Щ�ڵ㡢�����˶���ʱ�䡢���ĸ��ڵ�������쳣�ȵȡ�XNodeĿǰ�ṩ�˶Կ�Դ�ķֲ�ʽ׷��ϵͳZipkin��֧�֣��й�Zipkin��ص����Ͽ��Բ鿴������https://zipkin.io/�����⣬XNodeʹ���˿�Դ���Zipkin.net��Ϊ�ͻ��˷���Zipkin�ӿڣ�https://github.com/d-collab/zipkin.net��

### ʾ���ṹ
Ϊ���ܸ��õ����ַ���ĵ������̣���ʾ��������2��XNode����ˣ��ֱ��ṩCustomer�����Order���񡣷����������Ϊ��Client->Server->Server2��ʾ�����������5����Ŀ��ɣ�
<table>
<tr><td>��Ŀ����</td><td>��Ŀ����</td><td>˵��</td></tr>
<tr><td>Client</td><td>.Net Core����̨</td><td>�ͻ��ˣ�������Customer��������Contract��Service</td></tr>
<tr><td>Server</td><td>.Net Core����̨</td><td>����ˣ��ṩCustomer�����ڲ������Order�����ȡOrder��Ϣ������Contract��Service</td></tr>
<tr><td>Server2</td><td>.Net Core����̨</td><td>����ˣ��ṩOrder��������Contract��Service</td></tr>
<tr><td>Contract</td><td>.Net Core���</td><td>�ṩXNode������Լ���ӿڣ�</td></tr>
<tr><td>Service</td><td>.Net Core���</td><td>�ṩ����ʵ��</td></tr>
</table>

### Contractʵ��
�ڽ�������д�����ΪContract��.Net Core�����Ŀ�������
XNode
��Ŀ�д���Customer��Order��ʵ���ࣺ
``` c#
//Customer.cs
[DataContract]
public class Customer
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public string Name { get; set; }

    [DataMember(Order = 3)]
    public List<Order> Orders { get; set; }
}
```
``` c#
//Order.cs
[DataContract]
public class Order
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public int CustomerId { get; set; }

    [DataMember(Order = 3)]
    public List<OrderDetail> Detail { get; set; }
}

[DataContract]
public class OrderDetail
{
    [DataMember(Order = 1)]
    public int OrderId { get; set; }

    [DataMember(Order = 2)]
    public int GoodsId { get; set; }

    [DataMember(Order = 3)]
    public string GoodsName { get; set; }

    [DataMember(Order = 4)]
    public decimal Price { get; set; }

    [DataMember(Order = 5)]
    public int Amount { get; set; }
}
```
��Ŀ�д���ICustomerService��IOrderService�ӿڣ�
``` c#
//ICustomerService.cs
[Service("CustomerService", 10001, true)]
[ServiceProxy("CustomerService", 10001)]
public interface ICustomerService
{
    [Action("GetCustomers", 1)]
    [ActionProxy("GetCustomers", 1)]
    Task<Customer> GetCustomers(int customerId);
}
```
``` c#
//IOrderService.cs
[Service("OrderService", 20001, true)]
[ServiceProxy("OrderService", 20001)]
public interface IOrderService
{
    [Action("GetOrders", 1)]
    [ActionProxy("GetOrders", 1)]
    Task<List<Order>> GetOrders(int customerId);
}
```

### Serviceʵ��
�ڽ�������д�����ΪService��.Net Core�����Ŀ�������
Contract
��Ŀ�д�������ʵ��CustomerService��OrderService��
``` c#
//CustomerService.cs
public class CustomerService : ICustomerService
{
    #region Data

    private IList<Customer> customers = new List<Customer>()
    {
        new Customer()
        {
            Id = 1,
            Name = "Customer01"
        },
        new Customer()
        {
            Id = 2,
            Name = "Customer02"
        },
        new Customer()
        {
            Id = 3,
            Name = "Customer03"
        }
    };

    #endregion

    private IOrderService _orderService;

    public CustomerService(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public Task<Customer> GetCustomers(int customerId)
    {
        var customer = customers.Where(c => c.Id == customerId).SingleOrDefault();

        if (customer == null)
        {
            return Task.FromResult<Customer>(null);
        }

        customer.Orders = _orderService.GetOrders(customerId).Result;

        return Task.FromResult(customer);
    }
}
```
``` c#
//OrderService.cs
#region Data

private IList<Order> orders = new List<Order>()
{
    new Order()
    {
        Id = 1,
        CustomerId = 1,
        Detail = new List<OrderDetail>()
        {
            new OrderDetail()
            {
                OrderId = 1,
                GoodsId = 1,
                GoodsName = "A",
                Price = 12,
                Amount = 10
            },
            new OrderDetail()
            {
                OrderId = 1,
                GoodsId = 2,
                GoodsName = "B",
                Price = 26.5M,
                Amount = 1
            },
            new OrderDetail()
            {
                OrderId = 1,
                GoodsId = 3,
                GoodsName = "C",
                Price = 5.5M,
                Amount = 15
            }
        }
    },
    new Order()
    {
        Id = 2,
        CustomerId = 2,
        Detail = new List<OrderDetail>()
        {
            new OrderDetail()
            {
                OrderId = 2,
                GoodsId = 1,
                GoodsName = "A",
                Price = 12M,
                Amount = 3
            }
        }
    },
    new Order()
    {
        Id = 3,
        CustomerId = 1,
        Detail = new List<OrderDetail>()
        {
            new OrderDetail()
            {
                OrderId = 3,
                GoodsId = 1,
                GoodsName = "C",
                Price = 5.5M,
                Amount = 5
            }
        }
    }
};

#endregion

public Task<List<Order>> GetOrders(int customerId)
{
    return Task.FromResult(orders.Where(o => o.CustomerId == customerId).ToList());
}
```

### OrderServerʵ��
�ڽ�������д�����ΪServer2��.Net Core����̨��Ŀ�������
Contract
Service
Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.Json
Microsoft.Extensions.Logging.Console
XNode
XNode.Serializer.ProtoBuf
XNode.Communication.DotNetty
XNode.Zipkin
���������ļ�config.json��
``` c#
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9002"
      }
    }
  }
}
```
�������ļ����Կ�����Order����ʹ����9002�˿ڡ�
��Program.cs��
1.����һЩ��ʼ�������������ļ��ļ��أ�
``` c#
Console.WriteLine("Please input enter to start order server.");
Console.ReadLine();

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

//���÷������־������Ϊ���ܿ����������ϸ�ڣ��˴�����־��������ΪInformation
LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Information);

//���������ļ�
string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
var configRoot = new ConfigurationBuilder()
    .AddJsonFile(path)
    .Build();
    
var serverConfig = configRoot.GetServerConfig();
```
2.��XNode����������ú�ע�᣺
``` c#
//���÷���
var nodeServer = new NodeServerBuilder()
    .ApplyConfig(configRoot.GetServerConfig())
    .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
    .AddServiceProcessor(new ZipkinProcessor())     //���ZipkinProcessor
    .ConfigServiceProvider(GetServiceProvider())
    .UseDotNetty(serverConfig.ServerInfo)
    .Build();
```
``` c#
private static XNode.Server.IServiceProvider GetServiceProvider()
{
    //ע�����
    return new DefaultServiceProvider()
        .RegistService(typeof(IOrderService), typeof(OrderService));
}
```
���������.AddServiceProcessor(new ZipkinProcessor())��������ʾ��XNode����������Zipkin��չ��
3.��Zipkin�������ã�
``` c#
//Zipkin����
new ZipkinBootstrapper("OrderServer")
    .ZipkinAt("192.168.108.131")
    .WithSampleRate(1.0)
    .Start();
```
4.XNode����������ر�
``` c#
//��������
nodeServer.StartAsync().Wait();

Console.ReadLine();

//�رշ���
nodeServer.StopAsync();
```

### CustomerServerʵ��
�ڽ�������д�����ΪServer��.Net Core����̨��Ŀ�������
Contract
Service
Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.Json
Microsoft.Extensions.Logging.Console
XNode
XNode.Autofac
XNode.Serializer.ProtoBuf
XNode.Communication.DotNetty
XNode.Zipkin
���������ļ�config.json��
``` c#
{
  "xnode": {
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      }
    },
    "client": {
      "serviceProxies": [
        {
          "proxyName": "OrderService",
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9002",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 20001,
              "typeName": "Contract.IOrderService,Contract",
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
����CustomerServer�������ڲ������OrderServer�ṩ�ķ�������������XNode�����Ҳ��XNode�ͻ��ˡ���ˣ��������ļ��зֱ���server��client�����ýڡ�Customer����ʹ����9001�˿ڡ�
��Program.cs��
1.����һЩ��ʼ�������������ļ��ļ��أ�
``` c#
Console.WriteLine("Please input enter to start customer server.");
Console.ReadLine();

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

//���÷������־������Ϊ���ܿ����������ϸ�ڣ��˴�����־��������ΪInformation
LoggerManager.ServerLoggerFactory.AddConsole(LogLevel.Information);

//���ÿͻ�����־������Ϊ���ܿ����������ϸ�ڣ��˴�����־��������ΪInformation
LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Information);

//���������ļ�
string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
var configRoot = new ConfigurationBuilder()
    .AddJsonFile(path)
    .Build();
```
2.����ServiceProxyManagerʵ����ע��Autofac�����Ա�ʹ�÷������
``` c#
var serviceProxyManager = new ServiceProxyManager();
var container = GetAutofacContainer(serviceProxyManager);
```
``` c#
private static IContainer GetAutofacContainer(IServiceProxyManager serviceProxyManager)
{
    var builder = new ContainerBuilder();
    builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
    builder.RegisterType<CustomerService>()
        .As<ICustomerService>()
        .SingleInstance();
    builder.RegisterType<OrderService>()
        .As<IOrderService>()
        .EnableInterfaceInterceptors()
        .InterceptedBy(typeof(ServiceProxyInterceptor))
        .SingleInstance();

    var container = builder.Build();
    return container;
}
```
����ֻ��OrderService���������أ���Ϊֻ��OrderService��Զ�̷��񣬶�CustomerServiceΪ�����ṩʵ�֡�
3.XNode�ͻ������ã�
``` c#
var clientConfig = configRoot.GetClientConfig();

var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

var serviceCaller = new ServiceCallerBuilder()
    .Append(new ZipkinCaller(serializer))       //���ZipkinCaller
    .UseDefault()
    .Build();

if (clientConfig.ServiceProxies != null)
{
    foreach (var config in clientConfig.ServiceProxies)
    {
        serviceProxyManager
            .Regist(config, serviceCaller)
            .AddClients(
                new NodeClientBuilder()
                    .ConfigConnections(config.Connections)
                    .ConfigSerializer(serializer)
                    .UseDotNetty()
                    .Build()
            );
    }
}
```
����serviceProxyManager.Regist����������serviceCaller������ServiceCaller��XNode�������Ĺ�����չ�㣬XNode��������Zipkin��֧�־���ͨ��ServiceCallerʵ�ֵġ�ͨ������ServiceCallerBuilder.Append(new ZipkinCaller(new ProtoBufSerializer()))����������Zipkin��չ��ServiceCallerBuilder.UseDefault������ʾʹ��Ĭ�ϵ�Զ�̷������ʵ�֣����Regist����ʾ����ServiceCaller�ͱ������UseDefault���������ʵ����Զ�̷�����õ�ServiceCaller������Զ�̷��񽫲��ᱻ���á�
4.XNode��������ã�
``` c#
var serverConfig = configRoot.GetServerConfig();

//���÷���
var nodeServer = new NodeServerBuilder()
    .ApplyConfig(serverConfig)
    .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
    .AddServiceProcessor(new ZipkinProcessor())     //���ZipkinProcessor
    .UseAutofac(container)
    .UseDotNetty(serverConfig.ServerInfo)
    .Build();
```
���������.AddServiceProcessor(new ZipkinProcessor())��������ʾ��XNode����������Zipkin��չ��
5.��Zipkin�������ã�
``` c#
//Zipkin����
new ZipkinBootstrapper("CustomerServer")
    .ZipkinAt("192.168.108.131")
    .WithSampleRate(1.0)
    .Start();
```
6.����Զ��XNode����
``` c#
try
{
    //���ӷ���
    serviceProxyManager.ConnectAsync().Wait();
}
catch (AggregateException ex)
{
    foreach (var e in ex.InnerExceptions)
    {
        if (e is NetworkException netEx)
        {
            Console.WriteLine($"Connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
        }
        else
        {
            throw e;
        }
    }
}
```
7.XNode����������رգ�
``` c#
//��������
nodeServer.StartAsync().Wait();

Console.ReadLine();

//�رշ�������
serviceProxyManager.CloseAsync();

//�رշ���
nodeServer.StopAsync();
```

### Clientʵ��
�ڽ�������д�����ΪClient��.Net Core����̨��Ŀ�������
Contract
Service
Microsoft.Extensions.Configuration.Binder
Microsoft.Extensions.Configuration.Json
Microsoft.Extensions.Logging.Console
XNode
XNode.Autofac
XNode.Serializer.ProtoBuf
XNode.Communication.DotNetty
XNode.Zipkin
���������ļ�config.json��
``` c#
{
  "xnode": {
    "client": {
      "serviceProxies": [
        {
          "proxyName": "CustomerService",
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Contract.ICustomerService,Contract",
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
����Client��˵����ֻ��Ҫ֪��CustomerService�Ĵ��ڡ�
��Program.cs��
1.����һЩ��ʼ�������������ļ��ļ��أ�
``` c#
Console.WriteLine("Please input enter to begin.");
Console.ReadLine();

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

//���ÿͻ�����־����
LoggerManager.ClientLoggerFactory.AddConsole(LogLevel.Error);

//���������ļ�
string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
var configRoot = new ConfigurationBuilder()
    .AddJsonFile(path)
    .Build();
```
2.����ServiceProxyManagerʵ����ע��Autofac�����Ա�ʹ�÷������
``` c#
var serviceProxyManager = new ServiceProxyManager();
var container = GetAutofacContainer(serviceProxyManager);
```
``` c#
private static IContainer GetAutofacContainer(IServiceProxyManager serviceProxyManager)
{
    var builder = new ContainerBuilder();
    builder.Register(c => new ServiceProxyInterceptor(serviceProxyManager));
    builder.RegisterType<CustomerService>()
        .As<ICustomerService>()
        .EnableInterfaceInterceptors()
        .InterceptedBy(typeof(ServiceProxyInterceptor))
        .SingleInstance();
    builder.RegisterType<OrderService>()
        .As<IOrderService>()
        .EnableInterfaceInterceptors()
        .InterceptedBy(typeof(ServiceProxyInterceptor))
        .SingleInstance();

    var container = builder.Build();
    return container;
}
```
����Client������IOrderService�������ڱ�����CustomerService��ʵ����������IOrderService��Ϊ������ȷ�������д���ICustomerService��ʵ��������Ҳ��IOrderService������ע�ᣬʵ�����й�����OrderService�����ᱻ���á���������ע��IOrderService��������Client��Ŀ�����Ӷ�ICustomerService�ӿڵ�ʵ�ֲ�������IOrderService��
3.XNode�ͻ������ã�
``` c#
#region �ͻ�������

var clientConfig = configRoot.GetClientConfig();

var serializer = new ProtoBufSerializer(LoggerManager.ClientLoggerFactory);

var serviceCaller = new ServiceCallerBuilder()
    .Append(new ZipkinCaller(serializer))       //���ZipkinCaller
    .UseDefault()
    .Build();

if (clientConfig.ServiceProxies != null)
{
    foreach (var config in clientConfig.ServiceProxies)
    {
        serviceProxyManager
            .Regist(config, serviceCaller)
            .AddClients(
                new NodeClientBuilder()
                    .ConfigConnections(config.Connections)
                    .ConfigSerializer(serializer)
                    .UseDotNetty()
                    .Build()
            );
    }
}

#endregion
```
4.��Zipkin�������ã�
``` c#
//Zipkin����
new ZipkinBootstrapper("Client")
    .ZipkinAt("192.168.108.131")
    .WithSampleRate(1.0)
    .Start();
```
5.����Զ��XNode����
``` c#
try
{
    //���ӷ���
    serviceProxyManager.ConnectAsync().Wait();
}
catch (AggregateException ex)
{
    foreach (var e in ex.InnerExceptions)
    {
        if (e is NetworkException netEx)
        {
            Console.WriteLine($"Connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
        }
        else
        {
            throw e;
        }
    }
}
```
6.����XNode����
``` c#
try
{
    //���÷���
    var customerService = container.Resolve<ICustomerService>();
    var customer = customerService.GetCustomers(1).Result;
    Console.WriteLine($"Id = {customer.Id}, Name = {customer.Name}");
    Console.WriteLine("Orders:");
    foreach (var order in customer.Orders)
    {
        Console.WriteLine($"OrderId = {order.Id}");
        Console.WriteLine($"Detail:");
        foreach (var detail in order.Detail)
        {
            Console.WriteLine($"GoodsId = {detail.GoodsId}, GoodsName = {detail.GoodsName}, Price = {detail.Price}, Amount = {detail.Amount}");
        }
        Console.WriteLine("-----------------------------------");
    }
}
catch (RequestTimeoutExcption ex)
{
    Console.WriteLine($"Timeout: RequestId={ex.Request.Id}");
}
catch (ServiceCallException ex)
{
    Console.WriteLine($"Service call exception: ExceptionId={ex.ExceptionId}, ExceptionMessage={ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.ReadLine();

//�رշ�������
serviceProxyManager.CloseAsync();
```

### ����
���벢���к󣬰�����˳����3������̨��Ŀ������س���
1.Server2
2.Server
3.Client
�������к���Server��Server2�Ŀ���̨�п��Կ�������ִ�е���ϸ��Ϣ����Client�п��Կ�������ִ�н����
``` c#
Id = 1, Name = Customer01
Orders:
OrderId = 1
Detail:
GoodsId = 1, GoodsName = A, Price = 12, Amount = 10
GoodsId = 2, GoodsName = B, Price = 26.5, Amount = 1
GoodsId = 3, GoodsName = C, Price = 5.5, Amount = 15
-----------------------------------
OrderId = 3
Detail:
GoodsId = 1, GoodsName = C, Price = 5.5, Amount = 5
-----------------------------------
```
��Zipkin����̨�п��Կ������������Ϣ��
<img src="img/Zipkin.png" />

### ����ʾ������
XNode-Sample/08-ServiceTrace

## ��չ��
XNode����˺Ϳͻ��˷ֱ��ṩ�˽ӿ����ڹ��ܵ���չ��������Ա����ʵ����չ�ӿ�ΪXNode��������Ҫ�Ĺ��ܡ�

### �������չ�ӿ�
XNode�����ͨ��IServiceProcessor�ӿڽ��й��ܵ���չ���ӿ�ԭ�����£�
``` c#
/// <summary>
/// ���������ӿ�
/// </summary>
public interface IServiceProcessor
{
    /// <summary>
    /// Э��ջ����
    /// </summary>
    IProtocolStackFactory ProtocolStackFactory { set; }

    /// <summary>
    /// ���л���
    /// </summary>
    ISerializer Serializer { set; }

    /// <summary>
    /// ���������
    /// </summary>
    IServiceInvoker ServiceInvoker { set; }

    /// <summary>
    /// ��һ����������
    /// </summary>
    IServiceProcessor Next { get; set; }

    /// <summary>
    /// �Է���������д���
    /// </summary>
    /// <param name="context">���������ģ�ÿ�η�����ù���һ��ʵ��</param>
    /// <returns></returns>
    Task<ServiceProcessResult> ProcessAsync(ServiceContext context);
}
```
IServiceProcessor�ӿڱ����Ϊ���ߵķ�ʽ���е��ã��ڷ���˳�ʼ����ʱ�����ָ�����ServiceProcessor����XNode����˽��յ���������ʱ����һ��ServiceProcessor���ᱻ���ã���֮���ServiceProcessor��ǰһ��ServiceProcessor�����Ƿ�ᱻ���á�ͨ��ServiceProcessor����ʵ�ַ�����Ȩ��֤������׷�١���־��¼�ȹ��ܣ�������ԱҲ���Ը����Լ�������ʵ����Ӧ��ServiceProcessor������Ĵ�����ʾ���������ServiceProcessor��
``` c#
......
//���÷���
var nodeServer = new NodeServerBuilder()
    .ApplyConfig(serverConfig)
    .ConfigSerializer(new ProtoBufSerializer(LoggerManager.ServerLoggerFactory))
    .ConfigLoginValidator(loginValidator)
    .AddServiceProcessor(new ServiceAuthorizeProcessor(serviceAuthorizer))      //�������ڷ�����Ȩ��ServiceProcessor
    .ConfigServiceProvider(GetServiceProvider())
    .UseDotNetty(serverConfig.ServerInfo)
    .Build();
......
```

### �ͻ�����չ�ӿ�
XNode�ͻ���ͨ��IServiceCaller�ӿڽ��й��ܵ���չ���ӿ�ԭ�����£�
``` c#
/// <summary>
/// �Թ��߷�ʽ���÷���
/// </summary>
public interface IServiceCaller
{
    /// <summary>
    /// ��һ�����������
    /// </summary>
    IServiceCaller Next { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="nodeClientContainer">NodeClient����</param>
    /// <param name="info">���������Ϣ</param>
    /// <returns></returns>
    Task<ServiceCallResult> CallAsync(INodeClientContainer nodeClientContainer, ServiceCallInfo info);
}
```
IServiceCaller�ӿ�ͬ��Ҳ�����Ϊ���ߵķ�ʽ���е��ã��ͻ��˳�ʼ��ʱҲ�������ö��ServiceCaller�����ͻ��������˷����������ʱ��ServiceCaller���ᱻ���ã����÷�ʽ��ServiceProcessor���ơ�ͨ��ServiceCaller����ʵ�ַ���׷�١���־��¼�ȹ��ܡ�����Ĵ�����ʾ���������ServiceCaller��
``` c#
......
var serviceCaller = new ServiceCallerBuilder()
    .Append(new ZipkinCaller(new ProtoBufSerializer()))       //�������ڷ���׷�ٵ�ServiceCaller
    .UseDefault()
    .Build();
    
if (clientConfig.ServiceProxies != null)
{
    foreach (var config in clientConfig.ServiceProxies)
    {
        serviceProxyManager
            .Regist(config, serviceCaller)      //ע��ServiceCaller
            .AddClients(
                new NodeClientBuilder()
                    .ConfigConnections(config.Connections)
                    .ConfigSerializer(serializer)
                    .UseDotNetty()
                    .Build()
            );
    }
}
......
```

## Demo����
���½�ͨ��������ʾ������ʾXNode�����Web��Ŀ��ʹ�á�

### Demo����
��ʾ�����ṩ��ѯ�ͻ�����Ʒ��������Web API����Ŀ���þ��������ܹ���ƣ��ṹ���£�
<table>
<tr><td style="width: 120px">��Ŀ����</td><td style="width: 220px">��Ŀ����</td><td>������</td><td>˵��</td></tr>
<tr><td>Entity</td><td>.Net Core���</td><td></td><td>�ṩʵ����</td></tr>
<tr><td>Contract</td><td>.Net Core���</td><td>Entity</td><td>�ṩ������ִ��Ľӿ�</td></tr>
<tr><td>Service</td><td>.Net Core���</td><td>Contract,Entity</td><td>�ṩ����ʵ��</td></tr>
<tr><td>Repository</td><td>.Net Core���</td><td>Contract,Entity</td><td>�ṩ�ִ�ʵ��</td></tr>
<tr><td>Web</td><td>ASP.Net Core WebӦ�ó���</td><td>Contract,Entity,Service,Repository</td><td>ʵ��Web API</td></tr>
<tr><td>Launcher</td><td>.Net Core����̨Ӧ��</td><td>Contract,Entity,Service,Repository</td><td>���ڷ�������</td></tr>
</table>
Demo��ͨ��WebAPI�ṩ�ͻ�����Ʒ�Ĳ�ѯ���񣬲��ҽ�ʵ��WebAPI���ͻ�������Ʒ���񡢶��������ܸ��Ե��������ڲ�ͬ�ķ������ϡ�������������ϵ���£�
<table>
<tr><td style="width: 120px">��������</td><td style="width: 200px">��������</td><td>˵��</td></tr>
<tr><td>WebAPI</td><td>�ͻ�������Ʒ����</td><td>�ṩ�ͻ�����Ʒ��Web����</td></tr>
<tr><td>�ͻ�����</td><td>��������</td><td>�ṩ�ͻ���ѯ���񣬵���ѯ�����ͻ���Ϣʱͬʱ���ظ��û��Ķ����б� </td></tr>
<tr><td>��Ʒ����</td><td></td><td>�ṩ��Ʒ��ѯ����</td></tr>
<tr><td>��������</td><td>��Ʒ����</td><td>�ṩ������ѯ���񣬶�����Ϣ�а��������Ʒ����ϸ��Ϣ</td></tr>
</table>

### Entity
Entity��Ŀ�ṩ��ʵ�����ʵ�֣���Ӧ����XNode���л�����Ҫ��Attribute����Demo��ʵ����3��ʵ���࣬Customer��Goods��Order��
``` c#
[DataContract]
public class Customer
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public string Name { get; set; }

    [DataMember(Order = 3)]
    public List<Order> Orders { get; set; }
}
```
``` c#
[DataContract]
public class Goods
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public string Name { get; set; }

    [DataMember(Order = 3)]
    public decimal Price { get; set; }
}
```
``` c#
[DataContract]
public class Order
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public int CustomerId { get; set; }

    [DataMember(Order = 3)]
    public List<OrderDetail> Detail { get; set; }
}

[DataContract]
public class OrderDetail
{
    [DataMember(Order = 1)]
    public int GoodsId { get; set; }

    [DataMember(Order = 2)]
    public decimal Discount { get; set; }

    [DataMember(Order = 3)]
    public int Amount { get; set; }

    [DataMember(Order = 4)]
    public Goods GoodsInfo { get; set; }
}
```

### Contract
Contract��Ŀ�´�����2��Ŀ¼�ֱ��ŷ���Ͳִ��Ľӿ�������Service��Repository��
ServiceĿ¼�°�����3���ӿ�������
``` c#
[Service("GoodService", 10001)]
[ServiceProxy("GoodService", 10001)]
public interface IGoodsService
{
    [Action("GetAllGoods", 1)]
    [ActionProxy("GetAllGoods", 1)]
    Task<List<Goods>> GetAll();

    [Action("GetGoods", 2)]
    [ActionProxy("GetGoods", 2)]
    Task<Goods> Get(int goodsId);
}
```
``` c#
[Service("OrderService", 10002)]
[ServiceProxy("OrderService", 10002)]
public interface IOrderService
{
    [Action("GetOrders", 1)]
    [ActionProxy("GetOrders", 1)]
    Task<List<Order>> GetOrders(int customerId);
}
```
``` c#
[Service("CustomerService", 10003)]
[ServiceProxy("CustomerService", 10003)]
public interface ICustomerService
{
    [Action("GetAllCustomer", 1)]
    [ActionProxy("GetAllCustomer", 1)]
    Task<List<Customer>> GetAll();

    [Action("GetCustomer", 2)]
    [ActionProxy("GetCustomer", 2)]
    Task<Customer> Get(int customerId);
}
```
��Щ�ӿڶ�����ΪXNode�������ʽ��������Ӧ����XNode�����Attribute��ͬʱ����Щ�ӿ���ҲӦ��XNode�����Attribute�Ա���ΪXNode�������á���һ���ӿ���ͬʱӦ��XNode����Attribute��XNode����Attribute���Ա����ظ������ӿڣ�����һ�㳡������һ�ֱȽ��Ƽ��ķ�ʽ��

RepositoryĿ¼��Ҳ������3���ӿ�������
``` c#
public interface IGoodsRepository
{
    Task<List<Goods>> GetAll();

    Task<Goods> Get(int id);
}
```
``` c#
public interface IOrderRepository
{
    Task<List<Order>> GetOrders(int customerId);
}
```
``` c#
public interface ICustomerRepository
{
    Task<List<Customer>> GetAll();

    Task<Customer> Get(int id);
}
```
��Щ������ͨ�Ľӿ����������������ܹ��еĲִ��㣬��XNode����û�й�ϵ��

### Repository
Repository��Ŀ�ǲִ����ʵ�֣��ڳ�������Ŀ����Ҫ���ṩ�����ݿ������ʵ�֡���ʾ��Ϊ�˼���������ڴ�ģ�����ݿ⡣����ǰ��������Repository�ӿڣ��˴�Ҳʵ����3����Ӧ���ࣺ
``` c#
public class GoodsRepository : IGoodsRepository
{
    #region Data

    private IList<Goods> Goods
    {
        get
        {
            return new List<Goods>()
            {
                new Goods()
                {
                    Id = 1,
                    Name = "Goods01",
                    Price = 10.5M
                },
                new Goods()
                {
                    Id = 2,
                    Name = "Goods02",
                    Price = 22
                }
            };
        }
    }

    #endregion

    public Task<Goods> Get(int id)
    {
        return Task.FromResult(Goods.Where(g => g.Id == id).SingleOrDefault());
    }

    public Task<List<Goods>> GetAll()
    {
        return Task.FromResult(Goods.ToList());
    }
}
```
``` c#
public class OrderRepository : IOrderRepository
{
    #region Data

    private IList<Order> Orders
    {
        get
        {
            return new List<Order>()
            {
                new Order()
                {
                    Id = 1,
                    CustomerId = 1,
                    Detail = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            GoodsId = 1,
                            Amount = 5,
                            Discount = 1
                        },
                        new OrderDetail()
                        {
                            GoodsId = 2,
                            Amount = 2,
                            Discount = 0.8M
                        }
                    }
                },
                new Order()
                {
                    Id = 2,
                    CustomerId = 1,
                    Detail = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            GoodsId = 1,
                            Amount = 10,
                            Discount = 0.5M
                        }
                    }
                },
                new Order()
                {
                    Id = 3,
                    CustomerId = 2,
                    Detail = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            GoodsId = 2,
                            Amount = 25,
                            Discount = 0.6M
                        }
                    }
                }
            };
        }
    }

    #endregion

    public Task<List<Order>> GetOrders(int customerId)
    {
        return Task.FromResult(Orders.Where(o => o.CustomerId == customerId).ToList());
    }
}
```
``` c#
public class CustomerRepository : ICustomerRepository
{
    #region Data

    private IList<Customer> Customers
    {
        get
        {
            return new List<Customer>()
            {
                new Customer()
                {
                    Id = 1,
                    Name = "Customer01"
                },
                new Customer()
                {
                    Id = 2,
                    Name = "Customer02"
                },
                new Customer()
                {
                    Id = 3,
                    Name = "Customer03"
                }
            };
        }
    }

    #endregion

    public Task<Customer> Get(int id)
    {
        return Task.FromResult(Customers.Where(c => c.Id == id).SingleOrDefault());
    }

    public Task<List<Customer>> GetAll()
    {
        return Task.FromResult(Customers.ToList());
    }
}
```

### Service
Service��Ŀ�ṩXNode����ľ���ҵ��ʵ�֣������Ƕ�Ӧ3��XNode����ӿڵ�ʵ���ࣺ
``` c#
public class GoodsService : IGoodsService
{
    private IGoodsRepository _goodsRepository;

    public GoodsService(IGoodsRepository goodsRepository)
    {
        _goodsRepository = goodsRepository;
    }

    public Task<Goods> Get(int goodsId)
    {
        return _goodsRepository.Get(goodsId);
    }

    public Task<List<Goods>> GetAll()
    {
        return _goodsRepository.GetAll();
    }
}
```
��������һ��XNode�����ʵ�֣�����������ͨ��Serviceûʲô���𣬿�����Ա��Ȼ�Դ�ͳ�ķ�ʽ���п����������Demo�н�ʹ��Autofac��������ע�룬��Ϊ��Ʒ����������Ʒ�ִ��ӿڣ������ڹ��캯���н�����������
``` c#
public class OrderService : IOrderService
{
    private IOrderRepository _orderRepository;

    private IGoodsService _goodsService;

    public OrderService(IOrderRepository orderRepository,
        IGoodsService goodsService)
    {
        _orderRepository = orderRepository;
        _goodsService = goodsService;
    }

    public async Task<List<Order>> GetOrders(int customerId)
    {
        var orders = await _orderRepository.GetOrders(customerId);
        foreach (var order in orders)
        {
            foreach (var detail in order.Detail)
            {
                detail.GoodsInfo = await _goodsService.Get(detail.GoodsId);
            }
        }
        return orders;
    }
}
```
��Ϊ������ϸ�а�������Ʒ��Ϣ����������������������ִ��⻹������Ʒ�������Թ��캯���������˶���Ʒ�����������ֵ��һ����ǣ�����������IGoodsService�ӿ�������ʱ�������õĲ�ͬ�������ǵ��ñ���ʵ�֣�Ҳ�����ǵ���Զ�̷��񣬶���һ�ж��ڶ���������˵��͸���ģ������֮ǰ�½����ᵽ��XNode��������AOP����ʵ�ַ������͸������ͨ�����ڿ����׶ο��Խ�XNode��������Ϊ���ã��������д��붼���ڱ���ִ�У���Ԫ�����뵥�����Զ���ԭ���ķ�ʽ��ͬ�����Ϳ�ܵ�ѧϰ�ɱ���������Ŀ�Ŷ���˵�������ɼܹ�ʦ��������Ա��ƽӿڡ�����XNode������ͨ����Ա�����ڲ�֪��XNode���ڵ������ʵ��ҵ���߼���
``` c#
public class CustomerService : ICustomerService
{
    private ICustomerRepository _customerRepository;

    private IOrderService _orderService;

    public CustomerService(ICustomerRepository customerRepository,
        IOrderService orderService)
    {
        _customerRepository = customerRepository;
        _orderService = orderService;
    }

    public async Task<Customer> Get(int customerId)
    {
        var customer = await _customerRepository.Get(customerId);
        customer.Orders = await _orderService.GetOrders(customerId);
        return customer;
    }

    public Task<List<Customer>> GetAll()
    {
        return _customerRepository.GetAll();
    }
}
```
�ڷ��ص����ͻ��ķ����л�ͬʱ���ظÿͻ������ж�����Ϣ�����Կͻ�����������������

��Щ�����ʵ�ִ����봫ͳ�ķ�ʽ��ûʲô���𣬵���Ҫע���������ʹ����XNode����Щ������ܻ��Էֲ�ʽ�ķ�ʽ���в��������Щ�����ڷ����п��ܾ��޷�����ִ���ˡ����磬���ݿ�������XNode�����п��ܾ��޷�����ʹ���ˡ���֮��ʹ��XNode֮�󿪷���Ա��Ҫ��ʶ���Լ��������ĳ���δ�����п����Էֲ�ʽ�ķ�ʽ������ѡ��һЩ����ʱ��Ҫ�����Ƿ�֧�ֲַ�ʽ��

### Web
Web��Ŀ�ṩ��WebAPI��ʵ�֣�����ʹ��XNode������÷��񲢽������JSON��ʽ���ظ�ǰ�ˡ�Web��Ŀ��Ҫ��Ϊ3������--DTO���塢����/����ע�ᡢControllerʵ�֡�

#### DTO����
����WebAPI��˵�����ظ�ǰ�˵����ݲ�����ֱ��ʹ��ʵ�壬��Ϊʵ���п��ܰ�����һЩ����Ҫ�Ļ���ܵ����ݣ�����ͨ����ͨ��ת��ΪDTO֮���ٷ��ء�������DTO�Ķ��壺
``` c#
public class GoodsDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public static GoodsDTO From(Goods goods)
    {
        if (goods == null)
        {
            return null;
        }

        return new GoodsDTO()
        {
            Id = goods.Id,
            Name = goods.Name,
            Price = goods.Price
        };
    }

    public static List<GoodsDTO> From(List<Goods> list)
    {
        if (list == null)
        {
            return null;
        }

        var result = new List<GoodsDTO>();

        foreach (var goods in list)
        {
            result.Add(From(goods));
        }

        return result;
    }
}
```
``` c#
public class OrderDTO
{
    public int Id { get; set; }

    public List<OrderDetailDTO> Detail { get; set; }

    public static OrderDTO From(Order order)
    {
        if (order == null)
        {
            return null;
        }

        return new OrderDTO()
        {
            Id = order.Id,
            Detail = OrderDetailDTO.From(order.Detail)
        };
    }

    public static List<OrderDTO> From(List<Order> list)
    {
        if (list == null)
        {
            return null;
        }

        var result = new List<OrderDTO>();

        foreach (var order in list)
        {
            result.Add(From(order));
        }

        return result;
    }
}

public class OrderDetailDTO
{
    public int GoodsId { get; set; }

    public decimal Discount { get; set; }

    public int Amount { get; set; }

    public GoodsDTO GoodsInfo { get; set; }

    public decimal Price
    {
        get
        {
            return GoodsInfo.Price * Discount;
        }
    }

    public static OrderDetailDTO From(OrderDetail detail)
    {
        if (detail == null)
        {
            return null;
        }

        return new OrderDetailDTO()
        {
            Amount = detail.Amount,
            Discount = detail.Discount,
            GoodsId = detail.GoodsId,
            GoodsInfo = GoodsDTO.From(detail.GoodsInfo)
        };
    }

    public static List<OrderDetailDTO> From(List<OrderDetail> list)
    {
        if (list == null)
        {
            return null;
        }

        var result = new List<OrderDetailDTO>();

        foreach (var detail in list)
        {
            result.Add(From(detail));
        }

        return result;
    }
}
```
``` c#
public class CustomerDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<OrderDTO> Orders { get; set; }

    public static CustomerDTO From(Customer customer)
    {
        if (customer == null)
        {
            return null;
        }

        return new CustomerDTO()
        {
            Id = customer.Id,
            Name = customer.Name,
            Orders = OrderDTO.From(customer.Orders)
        };
    }

    public static List<CustomerDTO> From(List<Customer> list)
    {
        if (list == null)
        {
            return null;
        }

        var result = new List<CustomerDTO>();

        foreach (var customer in list)
        {
            result.Add(From(customer));
        }

        return result;
    }
}
```

#### ����/����ע��
���ȣ�����AutofacModule�ཫServiceע�ᵽAutofac�������Ա�ʹ��Autofac��IoC�����Լ�XNode�����ܣ�
``` c#
public class AutofacModule : Module
{
    private IServiceProxyManager _serviceProxyManager;

    public AutofacModule(IServiceProxyManager serviceProxyManager)
    {
        _serviceProxyManager = serviceProxyManager;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => new ServiceProxyInterceptor(_serviceProxyManager))
                .SingleInstance();

        builder.RegisterType<CustomerRepository>()
            .As<ICustomerRepository>()
            .SingleInstance();
        builder.RegisterType<GoodsRepository>()
            .As<IGoodsRepository>()
            .SingleInstance();
        builder.RegisterType<OrderRepository>()
            .As<IOrderRepository>()
            .SingleInstance();

        builder.RegisterType<CustomerService>()
            .As<ICustomerService>()
            .EnableInterfaceInterceptors()
            .InterceptedBy(typeof(ServiceProxyInterceptor))
            .SingleInstance();
        builder.RegisterType<GoodsService>()
            .As<IGoodsService>()
            .EnableInterfaceInterceptors()
            .InterceptedBy(typeof(ServiceProxyInterceptor))
            .SingleInstance();
        builder.RegisterType<OrderService>()
            .As<IOrderService>()
            .EnableInterfaceInterceptors()
            .InterceptedBy(typeof(ServiceProxyInterceptor))
            .SingleInstance();
    }
}
```
Ȼ�󣬴��������ļ�xnode.json���������£�
``` c#
{
  "name": "XNode-Demo",
  "xnode": {
    "global": {
      "minWorkThreads": 100
    },
    "client": {
      "serviceProxies": [
        {
          "proxyName": "CustomerService",
          "security": {
            "login": {
              "accountName": "Test01",
              "accountKey": "123456"
            }
          },
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9001",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10003,
              "typeName": "Contract.Service.ICustomerService,Contract",
              "enabled": true
            }
          ]
        },
        {
          "proxyName": "GoodsService",
          "security": {
            "login": {
              "accountName": "Test01",
              "accountKey": "123456"
            }
          },
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9002",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Contract.Service.IGoodsService,Contract",
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
����Web��Ŀ��˵ֻ�Ƿ�������ѷ������������ļ���ֻ�����ͻ�����ص����á���ΪWebAPIֻ�ṩ�ͻ�����Ʒ��صķ������Ҳֻ��������2������Ĵ��������ļ����и�minWorkThreads���ã����ǿ�ѡ��������С�����߳��������ã���������ѹ���ϴ�ʱ������Ӧ����ߣ�����ֻ��������ʾ������100��

Ȼ�󣬴���XNodeBootstrap���������XNodeע������еĴ��룺
``` c#
public class XNodeBootstrap
{
    public void Run(ILoggerFactory loggerFactory, IServiceProxyManager serviceProxyManager)
    {
        var logger = loggerFactory.CreateLogger<XNodeBootstrap>();

        var dir = Directory.GetCurrentDirectory();
        var fileName = "xnode.json";
        string path = Path.Combine(dir, fileName);

        var configRoot = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        var name = configRoot.GetValue<string>("name");
        var globalConfig = configRoot.GetGlobalConfig();
        GlobalSettings.Apply(globalConfig);

        #region Client

        var clientConfig = configRoot.GetClientConfig();

        if (clientConfig.ServiceProxies == null || clientConfig.ServiceProxies.Count == 0)
        {
            return;
        }

        XNode.Logging.LoggerManager.ClientLoggerFactory = loggerFactory;

        var serializer = new ProtoBufSerializer(XNode.Logging.LoggerManager.ClientLoggerFactory);

        var serviceCaller = new ServiceCallerBuilder()
            .UseDefault()
            .Build();

        foreach (var config in clientConfig.ServiceProxies)
        {
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

        serviceProxyManager.ConnectAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                if (task.Exception is AggregateException)
                {
                    foreach (var e in task.Exception.InnerExceptions)
                    {
                        if (e is NetworkException netEx)
                        {
                            logger.LogError(netEx, $"XNode client connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
                        }
                    }
                }
                else
                {
                    logger.LogError(task.Exception, $"XNode client connect has error. Message={task.Exception.Message}");
                }
                throw task.Exception;
            }
        });

        #endregion
    }
}
```
��Щ����ϸ����֮ǰ���½��ж�����ϸ˵��������Ͳ����ظ������ˡ�����Ҳ�������ļ�һ����ֻ��XNode�ͻ�����صĴ��롣

��󣬴�Startup.cs�ļ������Autofac��XNode��ش��룺
``` c#
......

public IServiceProxyManager ServiceProxyManager { get; } = new ServiceProxyManager();

// This method gets called by the runtime. Use this method to add services to the container.
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    // ʹ��Autofac��ΪIoC����
    var containerBuilder = new ContainerBuilder();
    containerBuilder.RegisterModule(new AutofacModule(ServiceProxyManager));
    containerBuilder.Populate(services);
    return new AutofacServiceProvider(containerBuilder.Build());
}

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseMvc();

    //ִ��XNode��ʼ��
    new XNodeBootstrap().Run(LoggerFactory, ServiceProxyManager);
}
```

#### Controllerʵ��
Controller���ֱȽϼ򵥣��ṩ�˿ͻ�����Ʒ�Ĳ�ѯ����
``` c#
[Route("api/[controller]/[action]")]
public class DemoController : Controller
{
    private ICustomerService _customerService;

    private IOrderService _orderService;

    private IGoodsService _goodsService;

    public DemoController(ICustomerService customerService,
        IOrderService orderService,
        IGoodsService goodsService)
    {
        _customerService = customerService;
        _orderService = orderService;
        _goodsService = goodsService;
    }

    [HttpGet]
    public async Task<List<CustomerDTO>> GetAllCustomers()
    {
        return CustomerDTO.From(await _customerService.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<CustomerDTO> GetCustomer(int id)
    {
        return CustomerDTO.From(await _customerService.Get(id));
    }

    [HttpGet]
    public async Task<List<GoodsDTO>> GetAllGoods()
    {
        return GoodsDTO.From(await _goodsService.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<GoodsDTO> GetGoods(int id)
    {
        return GoodsDTO.From(await _goodsService.Get(id));
    }
}
```

### Launcher
Launcher��Ŀ��һ������̨������������XNode������Ŀ��Ҳ��ͨ��Autofacʵ������ע�룬���Ҳ����AutofacModule�࣬ʵ����Web��Ŀ�е�һ�¡�ͬ��������XNodeBootstrap��ʵ��XNode����/�����ע������У���Web��Ŀ�в�ͬ����������XNode����˵�����:
``` c#
public class XNodeBootstrap
{
    public void Run(ILoggerFactory loggerFactory, IServiceProxyManager serviceProxyManager, IContainer container)
    {
        loggerFactory.AddConsole(LogLevel.Information);
        var logger = loggerFactory.CreateLogger<XNodeBootstrap>();

        var dir = Directory.GetCurrentDirectory();
        var fileName = "xnode.json";
        string path = Path.Combine(dir, fileName);

        var configRoot = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        var name = configRoot.GetValue<string>("name");
        var globalConfig = configRoot.GetGlobalConfig();
        GlobalSettings.Apply(globalConfig);

        #region Server

        XNode.Logging.LoggerManager.ServerLoggerFactory = loggerFactory;

        var loginValidator = new DefaultLoginValidator(configRoot.GetDefaultLoginValidatorConfig(), XNode.Logging.LoggerManager.ServerLoggerFactory);

        var serverConfig = configRoot.GetServerConfig();
        var nodeServer = new NodeServerBuilder()
            .ApplyConfig(serverConfig)
            .ConfigSerializer(new ProtoBufSerializer(XNode.Logging.LoggerManager.ServerLoggerFactory))
            .ConfigLoginValidator(loginValidator)
            .UseAutofac(container)
            .UseDotNetty(serverConfig.ServerInfo)
            .Build();

        nodeServer.StartAsync().Wait();

        #endregion

        #region Client

        var clientConfig = configRoot.GetClientConfig();

        if (clientConfig.ServiceProxies == null || clientConfig.ServiceProxies.Count == 0)
        {
            return;
        }

        XNode.Logging.LoggerManager.ClientLoggerFactory = loggerFactory;

        var serializer = new ProtoBufSerializer(XNode.Logging.LoggerManager.ClientLoggerFactory);

        var serviceCaller = new ServiceCallerBuilder()
            .UseDefault()
            .Build();

        foreach (var config in clientConfig.ServiceProxies)
        {
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

        serviceProxyManager.ConnectAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                if (task.Exception is AggregateException)
                {
                    foreach (var e in task.Exception.InnerExceptions)
                    {
                        if (e is NetworkException netEx)
                        {
                            logger.LogError(netEx, $"XNode client connect has net error. Host={netEx.Host}, Port={netEx.Port}, Message={netEx.Message}");
                        }
                    }
                }
                else
                {
                    logger.LogError(task.Exception, $"XNode client connect has error. Message={task.Exception.Message}");
                }
                throw task.Exception;
            }
        });

        #endregion
    }
}
```
��Program.cs�ļ�������ִ��Autofac��XNode���õĴ��룺
``` c#
static void Main(string[] args)
{
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    Console.InputEncoding = Encoding.UTF8;
    Console.OutputEncoding = Encoding.UTF8;

    var serviceProxyManager = new ServiceProxyManager();
    var containerBuilder = new ContainerBuilder();
    containerBuilder.RegisterModule(new AutofacModule(serviceProxyManager));
    var container = containerBuilder.Build();

    new XNodeBootstrap().Run(new LoggerFactory(), serviceProxyManager, container);

    Console.ReadLine();
}
```
����Demo������Ʒ���������ͻ�3�����񣬵�Launcher��һ��ͨ�õ���Ŀ����Launcher������Ժ���Բ����ڲ�ͬ�ķ������ϣ�ͨ�������ļ�����������Щ�����������Ʒ���������ͻ�3������ֱ���в���ÿ������ʹ�ò�ͬ�Ķ˿�ģ�ⲿ���ڲ�ͬ�ķ������ϡ�

���ȣ�����GoodsService����Ʒ�������������������ֻ��Ҫ����XNode����˼��ɣ�
``` c#
{
  "name": "GoodsService",
  "xnode": {
    "global": {
      "minWorkThreads": 100
    },
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9002"
      },
      "services": [
        {
          "serviceId": 10001,
          "enabled": true
        }
      ],
      "security": {
        "loginValidator": {
          "accounts": [
            {
              "accountName": "Test01",
              "accountKey": "123456",
              "ipWhiteList": []
            }
          ]
        }
      }
    }
  }
}
```
GoodsServiceʹ����9002�˿ڲ���������IdΪ10001�ķ���

Ȼ�󣬲���OrderService����������������Ʒ�������XNode����˺Ϳͻ��˶���Ҫ�������ã�
``` c#
{
  "name": "OrderService",
  "xnode": {
    "global": {
      "minWorkThreads": 100
    },
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9003"
      },
      "services": [
        {
          "serviceId": 10002,
          "enabled": true
        }
      ],
      "security": {
        "loginValidator": {
          "accounts": [
            {
              "accountName": "Test01",
              "accountKey": "123456",
              "ipWhiteList": []
            }
          ]
        }
      }
    },
    "client": {
      "serviceProxies": [
        {
          "proxyName": "GoodsService",
          "security": {
            "login": {
              "accountName": "Test01",
              "accountKey": "123456"
            }
          },
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9002",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10001,
              "typeName": "Contract.Service.IGoodsService,Contract",
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
OrderServiceʹ����9003�˿ڲ���������IdΪ10002�ķ���ͬʱ������Ʒ����������á�

��󣬲���CustomerService���ͻ�����������������
``` c#
{
  "name": "CustomerService",
  "xnode": {
    "global": {
      "minWorkThreads": 100
    },
    "server": {
      "serverInfo": {
        "host": "10.246.84.201",
        "port": "9001"
      },
      "services": [
        {
          "serviceId": 10003,
          "enabled": true
        }
      ],
      "security": {
        "loginValidator": {
          "accounts": [
            {
              "accountName": "Test01",
              "accountKey": "123456",
              "ipWhiteList": []
            }
          ]
        }
      }
    },
    "client": {
      "serviceProxies": [
        {
          "proxyName": "OrderService",
          "security": {
            "login": {
              "accountName": "Test01",
              "accountKey": "123456"
            }
          },
          "connections": [
            {
              "host": "10.246.84.201",
              "port": "9003",
              "localHost": "10.246.84.201"
            }
          ],
          "services": [
            {
              "serviceId": 10002,
              "typeName": "Contract.Service.IOrderService,Contract",
              "enabled": true
            }
          ]
        }
      ]
    }
  }
}
```
OrderServiceʹ����9001�˿ڲ���������IdΪ10003�ķ���ͬʱ���Զ�������������á�

### ����
��ǰ���������б���Ͳ���֮����1��Web�����3������̨�������ȸ��ݷ�����������ϵ��������GoodsService��OrderService��CustomerService��Ȼ������Web����ͨ��Postman����ʵ�ֵļ���WebAPI����ִ�гɹ�Postman��õ���Ӧ��JSON���ݣ���3������̨������Ҳ����ʾ����ĵ��������

### ����ʾ������
XNode-Demo

## ��������
Author: Junjie Sun
Mail: junjie_coding@163.com