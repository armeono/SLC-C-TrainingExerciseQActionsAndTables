using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QAction_1.Helpers;
using QAction_1.Models;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Protocol.Extension;

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

            var transportStreamStatus = Convert.ToString(transportStream[Parameter.Transportstreams.Idx.transportstreamsstatus]);


            // If the status is enabled, add the services, else remove them
            if (transportStreamStatus == "1")
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
        var serviceIds = protocol.GetKeys(Parameter.Services.tablePid, NotifyProtocol.KeyType.Index);

        List<string> servicesToRemove = new List<string>();

        foreach (var serviceId in serviceIds)
        {

            object[] service = (object[])protocol.GetRow(Parameter.Services.tablePid, serviceId);

            if (service == null)
            {
                continue;
            }

            var serviceTransportStreamId = service[Parameter.Services.Idx.servicestransportstreamsid].ToString();

            if (serviceTransportStreamId == rowKey)
            {
                servicesToRemove.Add(serviceId);
            }
        }

        protocol.DeleteRows(Parameter.Services.tablePid, servicesToRemove);

    }

    private static void AddServices(SLProtocol protocol, string rowKey)
    {
        var mockResponse = new MockDeviceResponse();

        DeviceResponse data = JsonConvert.DeserializeObject<DeviceResponse>(mockResponse.json);

        var currentTimestamp = DateTime.Now;

        var stream = data.TransportStreams.Find(ts => ts.TsId.ToString() == rowKey);

        var services = stream.Services.Select(s => new object[]
        {
           s.ServiceId.ToString(),
           s.ServiceName,
           s.ServiceType,
           s.ServiceProvider,
           stream.TsId.ToString(),
           currentTimestamp.ToOADate(),
        }).ToList();


        protocol.FillArray(Parameter.Services.tablePid, services, NotifyProtocol.SaveOption.Partial);

    }
}
