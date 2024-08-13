using Autorizacion.Abstracciones.BW;
using Autorizacion.Abstracciones.Modelos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;



namespace Autorizacion.Middleware
{
    public class ClaimsRoles
    {
        //Obtiene durante el flujo intersepta y permite seguir el flujo
        private readonly RequestDelegate _next;

        private readonly IConfiguration _configuration;
        private IAutorizacionBW _autorizacionBW;

        public ClaimsRoles(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        //Se crea la interseccion cuando el Midelware insecta en el flujo de vida de la aplicacion
        //Se captura la informacion
        public async Task InvokeAsync(HttpContext httpContext, IAutorizacionBW autorizacionBW)
        {
            _autorizacionBW = autorizacionBW;
            ClaimsIdentity appIdentity = await verificarAutorizacion(httpContext);
            httpContext.User.AddIdentity(appIdentity);

            //Devolvemos el Flujo modificado y que continue
            await _next(httpContext);
        }

        private async Task<ClaimsIdentity> verificarAutorizacion(HttpContext httpContext)
        {
            var claims = new List<Claim>();
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
            {
                await GetUser(httpContext, claims);
                await GetRole(httpContext, claims);
            }
            var appIdentity = new ClaimsIdentity(claims);
            return appIdentity;
        }

        private async Task GetRole(HttpContext httpContext, List<Claim> claims)
        {
            //Obtenemos todos los perfiles
            var Roles = await GetInformationRole(httpContext);
            if (Roles != null && Roles.Any())
            {
                foreach (var role in Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.RoleID.ToString()));
                }
            }
        }

        //Vamos uy obtnemos lo Roles
        private async Task<IEnumerable<Role>> GetInformationRole(HttpContext httpContext)
        {
            return await _autorizacionBW.ObtenerRolesXUsuario(
                new Abstracciones.Modelos.Usuario
                {
                    Nombre = httpContext.User.Claims.Where(c => c.Type == "usuario").FirstOrDefault().Value
                });
        }

        private async Task GetUser(HttpContext httpContext, List<Claim> claims)
        {
            var usuario = await GetInformationUser(httpContext);

            //Se procede con la validacion de que el susuario no venga vacio
            //Si todo bien con la informacion se agrega esto a los Claims
            if (usuario is not null && !string.IsNullOrEmpty(usuario.UsuarioID.ToString()) && !string.IsNullOrEmpty(usuario.Nombre.ToString()) && !string.IsNullOrEmpty(usuario.CorreoElectronico.ToString()))
            {
                //Usamos Claims por defecto y uno propio personalizado
                claims.Add(new Claim(ClaimTypes.Email, usuario.CorreoElectronico));
                claims.Add(new Claim(ClaimTypes.Name, usuario.Nombre));
                claims.Add(new Claim("UsuarioID", usuario.UsuarioID.ToString()));
            }

        }

        //Para ir a optener el usuario
        //Le pasamos el parametro ya que puede ser usuario o correo
        private async Task<Usuario> GetInformationUser(HttpContext httpContext)
        {
            return await _autorizacionBW.ObtenerUsuario(
                new Abstracciones.Modelos.Usuario
                {
                    Nombre = httpContext.User.Claims.Where(c => c.Type == "usuario").FirstOrDefault().Value
                });
        }
    }

    //Code para publicar paquete
    public static class ClaimsUsuarioMiddlewareExtensions
    {
        public static IApplicationBuilder AutorizacionClaims(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsRoles>();
        }
    }
}
