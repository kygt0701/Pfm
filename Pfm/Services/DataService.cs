using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Pfm.Common;
using Pfm.Services.Interfaces;

namespace Pfm.Services
{
    /// <summary>
    /// 데이터 서비스 구현
    /// </summary>
    public class DataService : IDataService
    {
        public DataTable ExecuteQuery(string sql, params object[] parameters)
        {
            return DbHelper.ExecuteQuery(sql, ConvertToSqlParameters(parameters));
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            return DbHelper.ExecuteNonQuery(sql, ConvertToSqlParameters(parameters));
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql, params object[] parameters)
        {
            return await Task.Run(() => ExecuteQuery(sql, parameters));
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            return await Task.Run(() => ExecuteNonQuery(sql, parameters));
        }

        private SqlParameter[] ConvertToSqlParameters(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return null;

            // 이미 SqlParameter 배열인 경우
            if (parameters is SqlParameter[] sqlParams)
                return sqlParams;

            // SqlParameter 객체들인 경우
            var result = new SqlParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is SqlParameter param)
                    result[i] = param;
            }
            return result;
        }
    }
}
