using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using TUD = Doppler.HtmlEditorApi.Test.Utils.TestUsersData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Doppler.HtmlEditorApi.Test.Utils;

public static class WebApplicationFactoryExtensions
{
    private static HttpClient CreateSutClient(
        this WebApplicationFactory<Startup> factory,
        IEnumerable<(Type, object)> servicesToOverride,
        string token)
    {
        token ??= TUD.TOKEN_TEST1_EXPIRE_20330518;
        var client = factory
            .WithWebHostBuilder(c =>
            {
                c.ConfigureServices(s =>
                {
                    foreach (var (type, instance) in servicesToOverride)
                    {
                        s.AddSingleton(type, instance);
                    }
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // TODO: use code generation for these generic methods
    public static HttpClient CreateSutClient(
        this WebApplicationFactory<Startup> factory,
        string token = null)
        => factory.CreateSutClient(
            servicesToOverride: ToTypeObjectPairArray(),
            token);
    public static HttpClient CreateSutClient<T1>(
        this WebApplicationFactory<Startup> factory,
        T1 serviceToOverride1,
        string token = null)
        => factory.CreateSutClient(
            servicesToOverride: ToTypeObjectPairArray(serviceToOverride1),
            token);
    public static HttpClient CreateSutClient<T1, T2>(
        this WebApplicationFactory<Startup> factory,
        T1 serviceToOverride1,
        T2 serviceToOverride2,
        string token = null)
        => factory.CreateSutClient(
            servicesToOverride: ToTypeObjectPairArray(serviceToOverride1, serviceToOverride2),
            token);
    public static HttpClient CreateSutClient<T1, T2, T3>(
        this WebApplicationFactory<Startup> factory,
        T1 serviceToOverride1,
        T2 serviceToOverride2,
        T3 serviceToOverride3,
        string token = null)
        => factory.CreateSutClient(
            servicesToOverride: ToTypeObjectPairArray(serviceToOverride1, serviceToOverride2, serviceToOverride3),
            token);
    public static HttpClient CreateSutClient<T1, T2, T3, T4>(
        this WebApplicationFactory<Startup> factory,
        T1 serviceToOverride1,
        T2 serviceToOverride2,
        T3 serviceToOverride3,
        T4 serviceToOverride4,
        string token = null)
        => factory.CreateSutClient(
            servicesToOverride: ToTypeObjectPairArray(serviceToOverride1, serviceToOverride2, serviceToOverride3, serviceToOverride4),
            token);
    public static HttpClient CreateSutClient<T1, T2, T3, T4, T5>(
        this WebApplicationFactory<Startup> factory,
        T1 serviceToOverride1,
        T2 serviceToOverride2,
        T3 serviceToOverride3,
        T4 serviceToOverride4,
        T5 serviceToOverride5,
        string token = null)
        => factory.CreateSutClient(
            servicesToOverride: ToTypeObjectPairArray(serviceToOverride1, serviceToOverride2, serviceToOverride3, serviceToOverride4, serviceToOverride5),
            token);

    private static (Type, object) ToTypeObjectPair<T1>(T1 obj)
        => (typeof(T1), (object)obj);

    private static IEnumerable<(Type, object)> ToTypeObjectPairArray()
        => new (Type, object)[0];
    private static IEnumerable<(Type, object)> ToTypeObjectPairArray<T1>(T1 obj1)
        => ToTypeObjectPairArray().Append(ToTypeObjectPair(obj1));
    private static IEnumerable<(Type, object)> ToTypeObjectPairArray<T1, T2>(T1 obj1, T2 obj2)
        => ToTypeObjectPairArray(obj1).Append(ToTypeObjectPair(obj2));
    private static IEnumerable<(Type, object)> ToTypeObjectPairArray<T1, T2, T3>(T1 obj1, T2 obj2, T3 obj3)
        => ToTypeObjectPairArray(obj1, obj2).Append(ToTypeObjectPair(obj3));
    private static IEnumerable<(Type, object)> ToTypeObjectPairArray<T1, T2, T3, T4>(T1 obj1, T2 obj2, T3 obj3, T4 obj4)
        => ToTypeObjectPairArray(obj1, obj2, obj3).Append(ToTypeObjectPair(obj4));
    private static IEnumerable<(Type, object)> ToTypeObjectPairArray<T1, T2, T3, T4, T5>(T1 obj1, T2 obj2, T3 obj3, T4 obj4, T5 obj5)
        => ToTypeObjectPairArray(obj1, obj2, obj3, obj4).Append(ToTypeObjectPair(obj5));
}
