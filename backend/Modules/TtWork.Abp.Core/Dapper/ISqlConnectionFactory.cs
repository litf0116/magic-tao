using System;
using System.Data;

namespace TtWork.Abp.Dapper {
    public interface ISqlConnectionFactory : IDisposable {
        IDbConnection GetOpenConnection();
    }
}