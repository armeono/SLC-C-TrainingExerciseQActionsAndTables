namespace QAction_1.Helpers
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.Scripting;
    using Skyline.DataMiner.Utils.Protocol.Extension;

    public static class SLProtocolExtensions
    {

        public static Dictionary<string, object> GetColumnAsDictionary(this SLProtocol protocol, int tablePid, int primaryKeyIndex, int columnPid)
        {

            var columns = protocol.GetColumns(tablePid, new uint[] { (uint)primaryKeyIndex, (uint)columnPid });

            var existingIds = (object[])columns[0];
            var existingStatuses = (object[])columns[1];

            Dictionary<string, object> returnColumns = new Dictionary<string, object>();
            for (int i = 0; i < existingIds.Length; i++)
            {
                var idObj = existingIds[i];

                if (idObj == null)
                {
                    continue;
                }

                returnColumns.Add(Convert.ToString(existingIds[i]), existingStatuses[i]);
            }
            return returnColumns;
        }
    }
}
