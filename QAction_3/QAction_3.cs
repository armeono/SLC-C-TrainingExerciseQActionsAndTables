using System;
using System.Collections.Generic;
using QAction_1.Helpers;
using QAction_1.Models;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Protocol.Extension;
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


            var existingStreams = GetExistingStreams(protocol);

            foreach (var ts in data.TransportStreams)
            {
                bool exists = existingStreams.TryGetValue(Convert.ToString(ts.TsId), out string streamStatus);

                if (!exists)
                {
                    transportsStreamsToInsert.Add(CreateTransportStreamsInsertObject(ts, currentTimestamp, streamStatus));

                    foreach (var service in ts.Services)
                    {
                        servicesToInsert.Add(CreateServicesInsertObject(service, currentTimestamp, Convert.ToString(ts.TsId)));
                    }

                    continue;
                }


                transportsStreamsToInsert.Add(CreateTransportStreamsInsertObject(ts, currentTimestamp, streamStatus));

                if (streamStatus == "1")
                {
                    foreach (var service in ts.Services)
                    {
                        servicesToInsert.Add(CreateServicesInsertObject(service, currentTimestamp, Convert.ToString(ts.TsId)));
                    }
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

    private static object[] CreateTransportStreamsInsertObject(TransportStream data, DateTime currentTimestamp, string status)
    {
        return new object[]
                       {
                        Convert.ToString(data.TsId),
                        data.TsName,
                        data.Multicast,
                        data.SourceIp,
                        data.NetworkId,
                        currentTimestamp.ToOADate(),
                        status, // Assuming 1 is the default value for the last column in the TransportStreams table
                       };
    }

    private static object[] CreateServicesInsertObject(Service data, DateTime currentTimestamp, string streamId)
    {
        return new object[]
                        {
                        Convert.ToString(data.ServiceId),
                        data.ServiceName,
                        data.ServiceType,
                        data.ServiceProvider,
                        streamId,
                        currentTimestamp.ToOADate(),
                        };
    }

    private static Dictionary<string, string> GetExistingStreams(SLProtocol protocol)
    {
        var existingIds = protocol.GetColumn(Parameter.Transportstreams.tablePid, Parameter.Transportstreams.Idx.transportstreamsid);
        var existingStatuses = protocol.GetColumn(Parameter.Transportstreams.tablePid, Parameter.Transportstreams.Idx.transportstreamsstatus);

        Dictionary<string, string> existingStreams = new Dictionary<string, string>();
        for (int i = 0; i < existingIds.Length; i++)
        {
            var idObj = existingIds[i];
            var statusObj = existingStatuses[i];
            if (idObj == null)
            {
                continue;
            }
            var id = Convert.ToString(idObj);
            var status = Convert.ToString(statusObj) ?? "1";
            existingStreams[id] = status;
        }
        return existingStreams;
    }
}