using Autorizacion.Abstracciones.DA;
using Autorizacion.Abstracciones.Modelos;
using System.Data.SqlClient;
using Dapper;
using Autorizacion.Helpers;

namespace Autorizacion.DA
{
    public class SeguridadDA : ISeguridadDA
    {
        IRepositorioDapper _repositorioDapper;
        private SqlConnection _sqlConnection;

        public SeguridadDA(IRepositorioDapper repositorioDapper)
        {
            _repositorioDapper = repositorioDapper;
            _sqlConnection = _repositorioDapper.ObtenerRepositorioDapper();
        }

        public async Task<IEnumerable<Role>> ObtenerRolesXUsuario(Usuario usuario)
        {
            string sql = @"[ObtenerRolesXUsuario]";
            var consulta = await _sqlConnection.QueryAsync<Abstracciones.Entidades.Role>(sql,
                new
                {
                    CorreoElectronico = usuario.CorreoElectronico,
                    Nombre = usuario.Nombre
                });
            return Convertidor.ConvertirLista<Abstracciones.Entidades.Role, Abstracciones.Modelos.Role>(consulta);
        }

        public async Task<Usuario> ObtenerUsuario(Usuario usuario)
        {
            string sql = @"[ObtenerUsuario]";
            var consulta = await _sqlConnection.QueryAsync<Abstracciones.Entidades.Usuario>(sql,
                new
                {
                    CorreoElectronico = usuario.CorreoElectronico,
                    Nombre = usuario.Nombre
                });
            return Convertidor.Convertir<Abstracciones.Entidades.Usuario, Abstracciones.Modelos.Usuario>(consulta.FirstOrDefault());
        }


    }
}
