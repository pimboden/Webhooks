using System.Data;

namespace Webhooks.Infrastructure.Abstractions
{
    public interface ISqlConnectionFactory
    {
        //Usefull when used togheter with Dapper, as it needs an open connection to work with
        Task<IDbConnection> CreateConnectionAsync();
    }
}
