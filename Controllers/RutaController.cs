using API.DAL;
using API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("Ruta")]
    [ApiController]
    public class RutaController : ControllerBase
    {
        private readonly SqlConnection _conn = Connection.GetConnection();

        [HttpPost]
        [Authorize]
        [Route("Agregar")]
        public IActionResult AddRute([FromBody] Ruta ruta)
        {
            var header = HttpContext.Request.Headers["Authorization"];
            string jwtToken = header.ToString().Replace("Bearer ", "");
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwtToken);
            string rol = token.Claims.FirstOrDefault(cl => cl.Type == "Rol").Value;
            bool isNotAdmin = !(rol == "ADMIN" || rol == "SUPERADMIN");

            if (isNotAdmin) return Unauthorized(new
            {
                Message = "No estas autorizado a agregar una ruta",
                Rol = rol
            });
            string q = "EXEC usp_añadirRuta @NumeroR, @inicioR, @finR";
            var com = new SqlCommand(q, _conn);
            com.Parameters.AddWithValue("@NumeroR", ruta.NumeroR);
            com.Parameters.AddWithValue("@inicioR", ruta.inicioR);
            com.Parameters.AddWithValue("@finR", ruta.finR);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                _conn.Close();
            }

            return Ok(new
            {
                Status = "Ok",
                RutaAgregada = ruta
            });
        }

        [HttpDelete]
        [Authorize]
        [Route("Eliminar")]
        public IActionResult DeleteRute([FromQuery] int NumeroR)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var finalTok = handler.ReadJwtToken(token);
            string rol = finalTok.Claims.FirstOrDefault(c => c.Type == "Rol").Value;
            bool isNotAdmin = !(rol == "ADMIN" || rol == "SUPERADMIN");
            if (isNotAdmin) return Unauthorized(new
            {
                Message = "No estas autorizado a agregar una ruta",
                Rol = rol
            });
            string q = $"EXECUTE usp_eliminarRuta {NumeroR}";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Ruta eliminada correctamente, todos los buses que tenian esta ruta, pasaron a tener una ruta 0"
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpPut]
        [Route("CambiarEstado")]
        [Authorize]
        public IActionResult UpdateRuteStatus([FromQuery] int NumeroR)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var finalTok = handler.ReadJwtToken(token);
            string rol = finalTok.Claims.FirstOrDefault(c => c.Type == "Rol").Value;
            if (rol != "ADMIN" || rol != "SUPERADMIN")
            {
                return Unauthorized(new
                {
                    Status = "Denegado",
                    Message = "Solo los usuarios con rol de ADMIN o SUPERADMIN pueden actualizar el estado de una ruta"
                });
            }
            string q = $"EXECUTE usp_cambiarEstadoRuta {NumeroR}";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Ruta Activada/Desactivada correctamente"
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    status = "Error",
                    message = ex.Message
                });
            }
            finally
            {
                _conn.Close();
            }

        }

        [HttpPut]
        [Authorize]
        [Route("Cambiar")]
        public IActionResult UpdateRuteWay([FromQuery] int NumeroR, [FromBody] ChangeRute rute)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var finalTok = handler.ReadJwtToken(token);
            string rol = finalTok.Claims.FirstOrDefault(c => c.Type == "Rol").Value;
            if (!(rol == "ADMIN" || rol == "SUPERADMIN"))
            {
                return Unauthorized(new
                {
                    Status = "Denegado",
                    Message = "Solo los usuarios con rol de ADMIN o SUPERADMIN pueden modificar el recorrido de una ruta"
                });
            }
            string q = "EXECUTE usp_actualizarRecorridoRuta @numeroR, @inicio, @fin";
            SqlCommand com = new(q, _conn);
            com.Parameters.AddWithValue("@numeroR", NumeroR);
            com.Parameters.AddWithValue("@inicio", rute.InicioR);
            com.Parameters.AddWithValue("@fin", rute.FinR);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Recorrido de la ruta cambiados correctamente"
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message,
                });
            }
            finally
            {
                _conn.Close();
            }
        }


        [HttpGet]
        [Route("Lista")]
        public IActionResult GetAllRutes()
        {
            string q = "EXECUTE usp_ListarRutas";
            SqlDataAdapter da = new(q, _conn);
            DataTable dt = new();
            List<Ruta> rutas = new();

            try
            {
                da.Fill(dt);
                foreach (DataRow el in dt.Rows)
                {
                    rutas.Add
                    (
                        new Ruta
                        {
                            NumeroR = (int)el["NumeroR"],
                            inicioR = el["inicioR"].ToString(),
                            finR = el["finR"].ToString(),
                            estadoR = (bool)el["estadoR"]
                        }
                    );
                }
                return Ok(new
                {
                    Status = "Ok",
                    Response = rutas
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            }
            finally { _conn.Close(); }
        }

        public class ChangeRute
        {
            public string InicioR { get; set; }
            public string FinR { get; set; }
        }

    }
}
