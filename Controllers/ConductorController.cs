using API.DAL;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;

namespace API.Controllers
{
    [Route("Conductor")]
    [ApiController]
    public class ConductorController : ControllerBase
    {
        private static readonly SqlConnection _conn = Connection.GetConnection();

        [HttpPost]
        [Route("Crear")]
        public IActionResult CreateADriver([FromBody] RegisterDriver driver)
        {
            string q = "EXEC usp_agregarConductor @CedulaC,@fotoC,@nombreC,@contraseñaC,@apellidoC," +
                        "@telefonoC,@correoC,@fkPlacaBus,@edadC,@horaEntrada,@horaSalida";
            SqlCommand comando = new(q, _conn);
            comando.Parameters.AddWithValue("@CedulaC", driver.CedulaC);
            comando.Parameters.AddWithValue("@fotoC", driver.Foto);
            comando.Parameters.AddWithValue("@nombreC", driver.Nombre);
            comando.Parameters.AddWithValue("@contraseñaC", driver.Contraseña);
            comando.Parameters.AddWithValue("@apellidoC", driver.Apellido);
            comando.Parameters.AddWithValue("@telefonoC", driver.Telefono);
            comando.Parameters.AddWithValue("@correoC", driver.Correo);
            comando.Parameters.AddWithValue("@fkPlacaBus", driver.FkPlacaBus);
            comando.Parameters.AddWithValue("@edadC", driver.Edad);
            comando.Parameters.AddWithValue("@horaEntrada", driver.HoraEntrada);
            comando.Parameters.AddWithValue("@horaSalida", driver.HoraSalida);
            _conn.Open();
            try
            {
                comando.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Usuario registrado correctamente"));
            }
            catch (SqlException ex)
            {
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpDelete]
        [Route("Eliminar")]
        [Authorize]
        public IActionResult DeleteADriver([FromQuery] string cedula)
        {
            string rol = Token.GetUserRol(HttpContext);
            if (rol == null) 
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, "No eres un usuario, no puedes acceder a este punto"));
            if (!(rol == "ADMIN" || rol == "SUPERADMIN"))
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, "Solo usuarios que sean ADMIN o SUPERADMIN pueden eliminar cuentas de conductores"));

            string q = $"EXECUTE usp_eliminarConductor '{cedula}'";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Conductor eliminado correctamente"));

            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpPut]
        [Route("Actualizar")]
        [Authorize]
        public IActionResult UpdateAccount([FromBody] RegisterDriver driver)
        {
            string q = "EXECUTE usp_actualizarCuentaConductor @CedulaC,@fotoC,@nombreC,@contraseñaC,@apellidoC," +
                        "@telefonoC,@correoC,@fkPlacaBus,@edadC,@horaEntrada,@horaSalida";
            SqlCommand comando = new(q, _conn);
            comando.Parameters.AddWithValue("@CedulaC", driver.CedulaC);
            comando.Parameters.AddWithValue("@fotoC", driver.Foto);
            comando.Parameters.AddWithValue("@nombreC", driver.Nombre);
            comando.Parameters.AddWithValue("@contraseñaC", driver.Contraseña);
            comando.Parameters.AddWithValue("@apellidoC", driver.Apellido);
            comando.Parameters.AddWithValue("@telefonoC", driver.Telefono);
            comando.Parameters.AddWithValue("@correoC", driver.Correo);
            comando.Parameters.AddWithValue("@fkPlacaBus", driver.FkPlacaBus);
            comando.Parameters.AddWithValue("@edadC", driver.Edad);
            comando.Parameters.AddWithValue("@horaEntrada", driver.HoraEntrada);
            comando.Parameters.AddWithValue("@horaSalida", driver.HoraSalida);
            _conn.Open();
            try
            {
                comando.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Conductor actualizado correctamente"));
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.DENIED, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.ERROR, ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }

        [HttpPut]
        [Authorize]
        [Route("Activar")]
        public IActionResult ActiveDriver([FromQuery] string cedula)
        {
            string rol = Token.GetUserRol(HttpContext);
            if (rol == null)
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, "No eres un usuario, no puedes acceder a este punto"));


            if (!(rol == "ADMIN" || rol == "SUPERADMIN"))
                return Unauthorized(new ResponseSender(StatusMessages.DENIED, "Solo los usuarios con rol de ADMIN o SUPERADMIN pueden aceptar una cuenta de un conductor"));
            string q = $"EXECUTE usp_activarConductor '{cedula}'";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new ResponseSender(StatusMessages.OK, "Conductor aceptado correctamente"));
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseSender(StatusMessages.DENIED, ex.Message));
            }
            finally
            {
                _conn.Close();
            }
        }


        public class RegisterDriver : Persona
        {
            public string CedulaC { get; set; }
            public override int Edad
            {
                get => base.Edad;
                set
                {
                    if (value < 18) throw new InvalidDataException("La edad no es suficiente");
                    else base.Edad = value;
                }
            }

            public string FkPlacaBus { get; set; }
            public string HoraEntrada { get; set; }
            public string HoraSalida { get; set; }

        }

    }
}
