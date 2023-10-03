using API.DAL;
using API.Helpers;
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

            string rol = GetRol.GetUserRol(HttpContext);
            if (!(rol == "ADMIN" || rol == "SUPERADMIN")) return Unauthorized(new ResponseSender("Denied", "No estas autorizado a agregar una ruta"));
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
                return BadRequest(new ResponseSender("Error",ex.Message));
            }
            finally
            {
                _conn.Close();
            }

            return Ok(new ResponseSender("Ok", "Ruta agregada correctamente"));
        }

        [HttpDelete]
        [Authorize]
        [Route("Eliminar")]
        public IActionResult DeleteRute([FromQuery] int NumeroR)
        {

            string rol = GetRol.GetUserRol(HttpContext);
            if (!(rol == "ADMIN" || rol == "SUPERADMIN")) return Unauthorized(new ResponseSender("Denied", $"No estas autorizado a eliminar una ruta"));
            string q = $"EXECUTE usp_eliminarRuta {NumeroR}";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender("Ok", "Ruta eliminada correctamente, todos los buses que tenian esta ruta, pasaron a tener una ruta 0"));
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender("Error", ex.Message));
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

            string rol = GetRol.GetUserRol(HttpContext);
            if (rol != "ADMIN" || rol != "SUPERADMIN")
            {
                return Unauthorized(new ResponseSender("Denied", "Solo los usuarios con rol de ADMIN o SUPERADMIN pueden actualizar el estado de una ruta"));
            }
            string q = $"EXECUTE usp_cambiarEstadoRuta {NumeroR}";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender("Ok", "Ruta Activada/Desactivada correctamente"));
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender("Error", ex.Message));
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

            string rol = GetRol.GetUserRol(HttpContext);
            if (!(rol == "ADMIN" || rol == "SUPERADMIN"))
                return Unauthorized(new ResponseSender("Denied", "Solo los usuarios con rol de ADMIN o SUPERADMIN pueden modificar el recorrido de una ruta"));
            
            string q = "EXECUTE usp_actualizarRecorridoRuta @numeroR, @inicio, @fin";
            SqlCommand com = new(q, _conn);
            com.Parameters.AddWithValue("@numeroR", NumeroR);
            com.Parameters.AddWithValue("@inicio", rute.InicioR);
            com.Parameters.AddWithValue("@fin", rute.FinR);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender("Ok", "Recorrido de la ruta cambiados correctamente"));
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender("Error", ex.Message));
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
            DataTable dt = new();
            List<Ruta> rutas = new();

            try
            {
                new SqlDataAdapter(q, _conn).Fill(dt);
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
                return Ok(new ResponseSender("Ok", rutas));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender("Error", ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }

        public class ChangeRute
        {
            public string InicioR { get; set; }
            public string FinR { get; set; }
        }

    }
}
