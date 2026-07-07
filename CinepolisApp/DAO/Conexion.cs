
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CinepolisApp.DAO
{
    public class Conexion
    {
        private readonly string _cadenaSQL;

        public Conexion()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            // Esta es la forma correcta y estandarizada de leer cadenas en .NET Core
            _cadenaSQL = configuration.GetConnectionString("CadenaSQL");
        }

        public string GetCadenaSQL()
        {
            return _cadenaSQL;
        }
    }
}