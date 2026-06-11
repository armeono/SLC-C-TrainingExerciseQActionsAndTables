using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                transportsStreamsToInsert.Add(new object[]
                   {
                        ts.TsId.ToString(),
                        ts.TsName,
                        ts.Multicast,
                        ts.SourceIp,
                        ts.NetworkId,
                        currentTimestamp.ToOADate(),
                   });

                foreach(var service in ts.Services)
                {
                    servicesToInsert.Add(new object[]
                    {
                        service.ServiceId.ToString(),
                        service.ServiceName,
                        service.ServiceType,
                        service.ServiceProvider,
                        ts.TsId.ToString(),
                        currentTimestamp.ToOADate(),
                    });
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
}