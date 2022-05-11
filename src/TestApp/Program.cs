
using MyNoSqlServer.DataWriter;
using Newtonsoft.Json;
using ProtoBuf.Grpc.Client;
using Service.Liquidity.ConverterMarkups.Domain;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using MarkupProfileConsts = Service.Liquidity.ConverterMarkups.Domain.Models.MarkupProfileConsts;

GrpcClientFactory.AllowUnencryptedHttp2 = true;

Console.Write("Press enter to start");
Console.ReadLine();
            
var groupWriter = new MyNoSqlServerDataWriter<MarkupProfilesNoSqlEntity>(GetUrl, MarkupProfilesNoSqlEntity.TableName, false);

var profiles = new List<string>();
profiles.Add(MarkupProfileConsts.DefaultProfile);
await groupWriter.InsertOrReplaceAsync(MarkupProfilesNoSqlEntity.Create(profiles));

await InitMarkups();
await InitAuto();
await InitOverview();
await InitAutoSettings();

Console.WriteLine("End");
Console.ReadLine();



static async Task InitMarkups()
{
    
    var oldWriter = new MyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity>(GetUrl, ConverterMarkupNoSqlEntity.TableName.Replace("-v2",""), false);
    var entities = await oldWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(entities));

    var newWriter = new MyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity>(GetUrl, ConverterMarkupNoSqlEntity.TableName, false);
    foreach (var entity in entities.Select(t => t.ConverterMarkup))
    {
        entity.ProfileId = MarkupProfileConsts.DefaultProfile;
        Console.WriteLine(JsonConvert.SerializeObject(entity));
        await newWriter.InsertOrReplaceAsync(ConverterMarkupNoSqlEntity.Create(entity));
    }
    var newEntities = await newWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(newEntities));
}
static async Task InitOverview()
{
    
    var oldWriter = new MyNoSqlServerDataWriter<ConverterMarkupOverviewNoSqlEntity>(GetUrl, ConverterMarkupOverviewNoSqlEntity.TableName.Replace("-v2",""), false);
    var entities = await oldWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(entities));

    var newWriter = new MyNoSqlServerDataWriter<ConverterMarkupOverviewNoSqlEntity>(GetUrl, ConverterMarkupOverviewNoSqlEntity.TableName, false);
    foreach (var entity in entities.Select(t => t.MarkupOverview))
    {
        entity.ProfileId = MarkupProfileConsts.DefaultProfile;
        Console.WriteLine(JsonConvert.SerializeObject(entity));

        await newWriter.InsertOrReplaceAsync(ConverterMarkupOverviewNoSqlEntity.Create(entity));
    }
    var newEntities = await newWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(newEntities));
}

static async Task InitAuto()
{
    
    var oldWriter = new MyNoSqlServerDataWriter<AutoMarkupNoSqlEntity>(GetUrl, AutoMarkupNoSqlEntity.TableName.Replace("-v2",""), false);
    var entities = await oldWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(entities));

    var newWriter = new MyNoSqlServerDataWriter<AutoMarkupNoSqlEntity>(GetUrl, AutoMarkupNoSqlEntity.TableName, false);
    foreach (var entity in entities.Select(t => t.AutoMarkup))
    {
        entity.ProfileId = MarkupProfileConsts.DefaultProfile;
        Console.WriteLine(JsonConvert.SerializeObject(entity));
        await newWriter.InsertOrReplaceAsync(AutoMarkupNoSqlEntity.Create(entity));
    }
    var newEntities = await newWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(newEntities));
}

static async Task InitAutoSettings()
{
    
    var oldWriter = new MyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity>(GetUrl, AutoMarkupSettingsNoSqlEntity.TableName.Replace("-v2",""), false);
    var entities = await oldWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(entities));

    var newWriter = new MyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity>(GetUrl, AutoMarkupSettingsNoSqlEntity.TableName, false);
    foreach (var entity in entities.Select(t => t.AutoMarkupSettings))
    {
        entity.ProfileId = MarkupProfileConsts.DefaultProfile;
        Console.WriteLine(JsonConvert.SerializeObject(entity));
        await newWriter.InsertOrReplaceAsync(AutoMarkupSettingsNoSqlEntity.Create(entity));
    }
    var newEntities = await newWriter.GetAsync();
    Console.WriteLine(JsonConvert.SerializeObject(newEntities));
}

static string GetUrl()
{
    return "http://192.168.70.80:5123"; //writer
}