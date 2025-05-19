using System.Data; // Necesitas referenciar System.Data.Common o un paquete que lo provea

namespace CampusLove.Core.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}