using System;
using System.Collections.Generic;
using System.Text;

namespace SNMPAgent.Utils
{
    public class DBGeneral
    {
        private SQLiteAPI sqlite = new SQLiteAPI();

        private const string TB_HISTORY = "upload_history";


        public int AddRecord()
        {
            var sql = $"insert into {TB_HISTORY} (date) values (CURRENT_TIMESTAMP)";
            var sql2 = $"select id from {TB_HISTORY} order by id desc limit 1";
            var dt = sqlite.InsertSelect(sql, sql2);

            if (dt == null || dt.Rows.Count == 0)
                return 0;

            return Convert.ToInt32(dt.Rows[0]["id"].ToString());
        }

        public void SetUploadStatus(int id, string status = "success", string description = "")
        {
            sqlite.InsertQueryParams($"update {TB_HISTORY} " +
                $"set status = '{status}', " +
                $"description = '{description}' " +
                $"where id = {id}");
        }

        public DataTableEx GetLastSuccessfulUpload()
        {
            var dt = new DataTableEx();
            var sql = $"SELECT * FROM {TB_HISTORY} " +
                        $"WHERE status = 'success' " +
                        $"ORDER BY id DESC " +
                        $"LIMIT 1";

            sqlite.QueryParams(sql, dt);

            if (dt.Rows.Count == 0)
                return null;

            return dt;
        }

        public DataTableEx GetUploadHistory()
        {
            var dt = new DataTableEx();
            var sql = $"SELECT * FROM {TB_HISTORY}";

            sqlite.QueryParams(sql, dt);

            if (dt == null)
                return null;

            return dt;
        }

    }
}
