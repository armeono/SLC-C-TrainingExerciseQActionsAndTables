using System;
using System.Collections.Generic;
using QAction_1.Helpers;
using QAction_1.Models;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

/// <summary>
/// DataMiner QAction Class: Poll Data.
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
            var mockResponse = new MockDeviceResponse();

            DeviceResponse data = SecureNewtonsoftDeserialization.DeserializeObject<DeviceResponse>(mockResponse.Json);

            TransportstreamsQActionRow streams = new TransportstreamsQActionRow();

            List<object[]> transportsStreamsToInsert = new List<object[]>();
            List<object[]> servicesToInsert = new List<object[]>();

            var currentTimestamp = DateTime.Now;


            var existingStreams = protocol.GetColumnAsDictionary(Parameter.Transportstreams.tablePid, Parameter.Transportstreams.Idx.transportstreamsid, Parameter.Transportstreams.Idx.transportstreamsstatus);

            foreach (var ts in data.TransportStreams)
            {
                bool exists = existingStreams.TryGetValue(Convert.ToString(ts.TsId), out var streamStatusObj);

                int streamStatus = Convert.ToInt32(streamStatusObj);

                // If the stream does not exist, we add it with the default status and add its services as well.
                if (!exists)
                {

                    transportsStreamsToInsert.Add(CreateTransportStreamsInsertObject(ts, currentTimestamp, Status.Enabled));

                    ProcessServices(servicesToInsert, ts.Services, currentTimestamp, Convert.ToString(ts.TsId));

                    continue;
                }

                // If the stream exists and is enabled, we update its timestamp and add its services as well. If it's disabled, we only update its timestamp.
                transportsStreamsToInsert.Add(CreateTransportStreamsInsertObject(ts, currentTimestamp, (Status)streamStatus));

                if (streamStatus == (int)Status.Enabled)
                {
                    ProcessServices(servicesToInsert, ts.Services, currentTimestamp, Convert.ToString(ts.TsId));
                }

            }

            PopulateTables(protocol, transportsStreamsToInsert, servicesToInsert);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }


    private static void PopulateTables(SLProtocol protocol, List<object[]> transportStreamsToInsert, List<object[]> servicesToInsert)
    {
        protocol.FillArray(Parameter.Transportstreams.tablePid, transportStreamsToInsert, NotifyProtocol.SaveOption.Partial);
        protocol.FillArray(Parameter.Services.tablePid, servicesToInsert, NotifyProtocol.SaveOption.Partial);
    }

    private static TransportstreamsQActionRow CreateTransportStreamsInsertObject(TransportStream data, DateTime currentTimestamp, Status status)
    {
        return new TransportstreamsQActionRow
        {
            Transportstreamsid_1001 = Convert.ToString(data.TsId),
            Transportstreamsname_1002 = data.TsName,
            Transportstreamsmulticast_1003 = data.Multicast,
            Transportstreamssourceip_1004 = data.SourceIp,
            Transportstreamsnetworkid_1005 = data.NetworkId,
            Transportstreamslastpolledat_1006 = currentTimestamp.ToOADate(),
            Transportstreamsstatus_1007 = status,
        };
    }

    private static ServicesQActionRow CreateServicesInsertObject(Service data, DateTime currentTimestamp, string streamId)
    {
        return new ServicesQActionRow
        {
            Servicesserviceid_1051 = Convert.ToString(data.ServiceId),
            Servicesservicename_1052 = data.ServiceName,
            Servicesservicetype_1053 = data.ServiceType,
            Servicesserviceprovider_1054 = data.ServiceProvider,
            Servicestransportstreamsid_1055 = streamId,
            Serviceslastpolledat_1056 = currentTimestamp.ToOADate(),
        };
    }

    private static void ProcessServices(List<object[]> servicesToInsert, List<Service> services, DateTime currentTimestamp, string streamId)
    {
        foreach (var service in services)
        {
            servicesToInsert.Add(CreateServicesInsertObject(service, currentTimestamp, streamId));
        }
    }
}