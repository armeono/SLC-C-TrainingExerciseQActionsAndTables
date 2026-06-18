using System;
using System.Collections.Generic;
using System.Linq;
using QAction_1.Helpers;
using QAction_1.Models;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Protocol.Extension;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol)
    {
        try
        {
            var rowKey = protocol.RowKey();

            object[] transportStream = (object[])protocol.GetRow(Parameter.Transportstreams.tablePid, rowKey);

            int transportStreamStatus = Convert.ToInt32(transportStream[Parameter.Transportstreams.Idx.transportstreamsstatus]);


            // If the status is enabled, add the services, else remove them
            if (transportStreamStatus == (int)Status.Enabled)
            {

                AddServices(protocol, rowKey);
                return;
            }

            RemoveServices(protocol, rowKey);



        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }

    private static void RemoveServices(SLProtocol protocol, string rowKey)
    {

        List<string> servicesToRemove = new List<string>();

        var services = protocol.GetColumnAsDictionary(Parameter.Services.tablePid, Parameter.Services.Idx.servicesserviceid, Parameter.Services.Idx.servicestransportstreamsid);

        foreach (var service in services)
        {
            var transportStreamId = service.Value;

            if (Convert.ToString(transportStreamId) == rowKey)
            {
                servicesToRemove.Add(service.Key);
            }
        }

        protocol.DeleteRows(Parameter.Services.tablePid, servicesToRemove);

    }

    private static void AddServices(SLProtocol protocol, string rowKey)
    {
        var mockResponse = new MockDeviceResponse();

        DeviceResponse data = SecureNewtonsoftDeserialization.DeserializeObject<DeviceResponse>(mockResponse.Json);

        var currentTimestamp = DateTime.Now;

        var stream = data.TransportStreams.Find(ts => ts.TsId.ToString() == rowKey);

        var services = stream.Services.Select(s => new ServicesQActionRow
        {
            Servicesserviceid_1051 = s.ServiceId.ToString(),
            Servicesservicename_1052 = s.ServiceName,
            Servicesservicetype_1053 = s.ServiceType,
            Servicesserviceprovider_1054 = s.ServiceProvider,
            Servicestransportstreamsid_1055 = stream.TsId.ToString(),
            Serviceslastpolledat_1056 = currentTimestamp.ToOADate(),
        }.ToObjectArray()).ToList();


        protocol.FillArray(Parameter.Services.tablePid, services, NotifyProtocol.SaveOption.Partial);

    }
}
