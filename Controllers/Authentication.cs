using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Data.SqlClient;
using API.DAL;
using System.Data;

namespace API.Controllers
{
    [Route("Auth")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        private readonly byte[] keyBytes;
        public Authentication(IConfiguration _config)
        {
            this.keyBytes = Encoding.ASCII.GetBytes(_config.GetSection("Jwt").GetSection("signingKey").ToString());
        }
        private readonly SqlConnection _conn = Connection.GetConnection();
        private static string q = "";

        [HttpPost]
        [Route("User")]
        public IActionResult Validar([FromBody] InicioSesion request)
        {
            q = $"EXECUTE usp_iniciarSesion '{request.Documento}', '{request.contraseña}'";
            SqlDataAdapter da = new(q, _conn);
            var dt = new DataTable();
            da.Fill(dt);
            Usuario usuario = new();
            if (dt.Rows.Count == 1)
            {
                FillUser(usuario, dt);
                var claims = new ClaimsIdentity();

                Claim[] claim = new Claim[] {
                new Claim("Foto", usuario.Foto),
                new Claim("Nombre", usuario.Nombre),
                new Claim("Apellido", usuario.Apellido),
                new Claim("Edad", usuario.Edad.ToString()),
                new Claim("Telefono", usuario.Telefono),
                new Claim("Contraseña", usuario.Contraseña),
                new Claim("Correo", usuario.Correo),
                new Claim("Documento", usuario.Documento),
                new Claim("Rol", usuario.Rol)
                };
                claims.AddClaims(claim);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddHours(10),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

                string token = tokenHandler.WriteToken(tokenConfig);

                return Ok(new
                {
                    Token = token,
                    Status = "Ok"
                });
            }
            return Unauthorized("Usuario no encontrado");
        }


        [HttpPost]
        [Route("Driver")]
        public IActionResult GetDriverToken([FromBody] InicioSesion request)
        {
            string q = $"EXECUTE usp_iniciarSesionConductor '{request.Documento}', '{request.contraseña}'";
            SqlDataAdapter da = new(q, _conn);
            DataTable dt = new();
            Conductor conductor = new();
            try
            {
                da.Fill(dt);
                FillDriver(conductor, dt);
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            var claimIde = new ClaimsIdentity();
            Claim[] claims = new Claim[]
            {
                new Claim("CedulaC", conductor.CedulaC),
                new Claim("Foto", conductor.Foto),
                new Claim("Estado", conductor.Estado.ToString()),
                new Claim("Nombre", conductor.Nombre),
                new Claim("Contraseña", conductor.Contraseña),
                new Claim("Apellido", conductor.Apellido),
                new Claim("Telefono", conductor.Telefono),
                new Claim("Correo", conductor.Correo),
                new Claim("FkPlacaBus", conductor.FkPlacaBus),
                new Claim("Edad", conductor.Edad.ToString()),
                new Claim("HoraEntrada", conductor.HoraEntrada),
                new Claim("HoraSalida", conductor.HoraSalida)
            };

            claimIde.AddClaims(claims);


            var descriptor = new SecurityTokenDescriptor
            {
                Subject = claimIde,
                Expires = DateTime.UtcNow.AddHours(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
            };
            var handler = new JwtSecurityTokenHandler();
            var tokenC = handler.CreateToken(descriptor);
            var token = handler.WriteToken(tokenC);
            return Ok(new
            {
                Status = "Ok",
                Response = token
            });
        }


        private static void FillDriver(Conductor conductor, DataTable dt)
        {
            conductor.CedulaC = dt.Rows[0]["CedulaC"].ToString();
            conductor.Foto = dt.Rows[0]["fotoC"].ToString();
            conductor.Estado = (bool)dt.Rows[0]["estadoC"];
            conductor.Nombre = dt.Rows[0]["nombreC"].ToString();
            conductor.Contraseña = dt.Rows[0]["contraseñaC"].ToString();
            conductor.Apellido = dt.Rows[0]["apellidoC"].ToString();
            conductor.Telefono = dt.Rows[0]["telefonoC"].ToString();
            conductor.Correo = dt.Rows[0]["correoC"].ToString();
            conductor.FkPlacaBus = dt.Rows[0]["fkPlacaBus"].ToString();
            conductor.Edad = (int)dt.Rows[0]["edadC"];
            conductor.HoraEntrada = dt.Rows[0]["horaEntrada"].ToString();
            conductor.HoraSalida = dt.Rows[0]["horaSalida"].ToString();
        }

        private static void FillUser(Usuario user, DataTable dt)
        {
            user.Foto = dt.Rows[0]["fotoU"].ToString();
            user.Nombre = dt.Rows[0]["nombreU"].ToString();
            user.Apellido = dt.Rows[0]["apellidoU"].ToString();
            user.Edad = (int)dt.Rows[0]["edadU"];
            user.Telefono = dt.Rows[0]["telefonoU"].ToString();
            user.Contraseña = dt.Rows[0]["contraseñaU"].ToString();
            user.Correo = dt.Rows[0]["correoU"].ToString();
            user.Documento = dt.Rows[0]["DocumentoU"].ToString();
            user.Rol = dt.Rows[0]["rolU"].ToString();
        }
    }
}

