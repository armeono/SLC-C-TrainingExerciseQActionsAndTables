using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QAction_1.Helpers;
using QAction_1.Models;
using Skyline.DataMiner.Scripting;

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

            DeviceResponse data = JsonConvert.DeserializeObject<DeviceResponse>(mockResponse.json);

            List<object[]> transportsStreamsToInsert = new List<object[]>();
            List<object[]> servicesToInsert = new List<object[]>();

            var currentTimestamp = DateTime.Now;


            foreach (var ts in data.TransportStreams)
            {
                object[] existingStream = (object[])protocol.GetRow(Parameter.Transportstreams.tablePid, ts.TsId.ToString());

                var statusObj = existingStream[Parameter.Transportstreams.Idx.transportstreamsstatus];
                var streamStatus = statusObj?.ToString() ?? "1";

                if (existingStream.Length == 0)
                {
                    transportsStreamsToInsert.Add(CreateTransportStreamsInsertObject(ts, currentTimestamp, streamStatus));

                    foreach (var service in ts.Services)
                    {
                        servicesToInsert.Add(CreateServicesInsertObject(service, currentTimestamp, ts.TsId.ToString()));
                    }

                    continue;

                }

                transportsStreamsToInsert.Add(CreateTransportStreamsInsertObject(ts, currentTimestamp, streamStatus));

                if (streamStatus == "1")
                {
                    foreach (var service in ts.Services)
                    {
                        servicesToInsert.Add(CreateServicesInsertObject(service, currentTimestamp, ts.TsId.ToString()));
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
        protocol.FillArray(Parameter.Transportstreams.tablePid, transportStreamsToInsert, NotifyProtocol.SaveOption.Full);
        protocol.FillArray(Parameter.Services.tablePid, servicesToInsert, NotifyProtocol.SaveOption.Full);
    }

    private static object[] CreateTransportStreamsInsertObject(TransportStream data, DateTime currentTimestamp, string status)
    {
        return new object[]
                       {
                        data.TsId.ToString(),
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
                        data.ServiceId.ToString(),
                        data.ServiceName,
                        data.ServiceType,
                        data.ServiceProvider,
                        streamId,
                        currentTimestamp.ToOADate(),
                        };
    }
}