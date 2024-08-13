using Autorizacion.Abstracciones.Modelos;


namespace Autorizacion.Abstracciones.BW
{
    public interface IAutorizacionBW
    {
        Task<Usuario> ObtenerUsuario(Usuario usuario);
        Task<IEnumerable<Role>> ObtenerRolesXUsuario(Usuario usuario);
    }
}
