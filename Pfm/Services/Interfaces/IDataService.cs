using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Pfm.Services.Interfaces
{
    /// <summary>
    /// 데이터 서비스 인터페이스
    /// </summary>
    public interface IDataService
    {
        DataTable ExecuteQuery(string sql, params object[] parameters);
        int ExecuteNonQuery(string sql, params object[] parameters);
        Task<DataTable> ExecuteQueryAsync(string sql, params object[] parameters);
        Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters);
    }
}
