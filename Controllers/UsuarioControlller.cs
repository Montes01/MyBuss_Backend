using API.DAL;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("User")]
    [ApiController]
    public class UsuarioControlller : ControllerBase
    {
        private static readonly SqlConnection _conn = Connection.GetConnection();
        [HttpPost]
        [Route("Add")]
        public IActionResult AddAnUser(UserRegister usuario)
        {
            string q = "EXECUTE usp_agregarUsuario @DocumentoU,@fotoU,@nombreU,@apellidoU," +
                "@edadU,@telefonoU,@contraseñaU,@correoU";
            SqlCommand comando = new(q, _conn);
            comando.Parameters.AddWithValue("@DocumentoU", usuario.Documento);
            comando.Parameters.AddWithValue("@fotoU", usuario.Foto);
            comando.Parameters.AddWithValue("@nombreU", usuario.Nombre);
            comando.Parameters.AddWithValue("@apellidoU", usuario.Apellido);
            comando.Parameters.AddWithValue("@edadU", usuario.Edad);
            comando.Parameters.AddWithValue("@telefonoU", usuario.Telefono);
            comando.Parameters.AddWithValue("@contraseñaU", usuario.Contraseña);
            comando.Parameters.AddWithValue("@correoU", usuario.Correo);
            _conn.Open();
            try
            {
                comando.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Usuario correctamente agregado"
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(400, new
                {
                    Status = "Denied",
                    message = ex.Message
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
            finally
            {
                _conn.Close();
            }
        }

        [HttpDelete]
        [Route("Remove")]
        [Authorize]
        public IActionResult RemoveMyAccount()
        {
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            var RolClaim = token.Claims.FirstOrDefault(c => c.Type == "Rol");
            if (RolClaim == null) { return BadRequest(new { Status = "Denegado", Message = "Este metodo es unicamente para eliminar usuarios no conductores" }); }
            string? Documento = token.Claims.FirstOrDefault(C => C.Type == "Documento").Value;
            string rol = RolClaim.Value;
            string query = "EXECUTE ";
            switch (rol)
            {
                case "SUPERADMIN":
                    query += $"usp_elminarUsuarioSuperAdmin {Documento}";
                    break;
                case "ADMIN":
                    query += $"usp_eliminarUsuarioAdmin {Documento}";
                    break;
                default:
                    query = $"usp_eliminarUsuario {Documento}";
                    break;
            }
            SqlCommand com = new(query, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Usuario eliminado correctamente"
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(400, new
                {
                    Status = "Error",
                    message = ex.Message
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
            finally
            {
                _conn.Close();
            }
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult UpdateAccount(UserRegister user)
        {
            string q = "EXECUTE usp_actualizarUsuario @DocumentoU,@fotoU,@nombreU,@apellidoU,@edadU,@telefonoU,@contraseñaU,@correoU";
            SqlCommand com = new(q, _conn);
            string? tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            if (token.Claims.FirstOrDefault(C => C.Type == "Rol") == null) return Unauthorized(new { Status = "Denegado", Message = "Este Endpoint es unicamente para actualizar usuarios" });
            string Documento = token.Claims.FirstOrDefault(C => C.Type == "Documento").Value;
            string Contraseña = token.Claims.FirstOrDefault(C => C.Type == "Contraseña").Value;
            if (Documento != user.Documento && Contraseña != user.Contraseña)
            {
                return Unauthorized(new
                {
                    Status = "Denegado",
                    Message = "No puedes actualizar una cuenta que no sea la tuya"
                });
            }
            com.Parameters.AddWithValue("@DocumentoU", user.Documento);
            com.Parameters.AddWithValue("@fotoU", user.Foto);
            com.Parameters.AddWithValue("@nombreU", user.Nombre);
            com.Parameters.AddWithValue("@apellidoU", user.Apellido);
            com.Parameters.AddWithValue("@edadU", user.Edad);
            com.Parameters.AddWithValue("@telefonoU", user.Telefono);
            com.Parameters.AddWithValue("@contraseñaU", user.Contraseña);
            com.Parameters.AddWithValue("@correoU", user.Correo);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = "Cuenta actualizada correctamente"
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
            catch (Exception ex)
            {
                return StatusCode(400, new
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


        [HttpPatch]
        [Route("ChangeRol")]
        [Authorize]
        public IActionResult CambiarRolDeUsuario([FromQuery] string Documento, [FromQuery] string nuevoRol) 
        {
            string tokenS = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            if (token.Claims.FirstOrDefault(C => C.Type == "Rol") == null) return Unauthorized(new {Status = "Denegado", Message = "Solo los usuarios pueden usar este metodo"});
            Claim DocumentoClaim = token.Claims.FirstOrDefault(C => C.Type == "Documento");
            string rol = DocumentoClaim.Value;
            string q = $"EXECUITE usp_cambiarRolDeUsuario '{Documento}', '{nuevoRol}', '{rol}'";
            SqlCommand com = new(q, _conn);
            _conn.Open();
            try
            {
                com.ExecuteNonQuery();
                return Ok(new
                {
                    Status = "Ok",
                    Message = $"Usuario correctamente seteado como {rol}"
                });
            } catch (SqlException ex)
            {
                return Unauthorized(new
                {
                    Status = "Denegado",
                    message = ex.Message
                });
            } catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    message = ex.Message
                });
            } finally
            {
                _conn.Close();
            }
        }
        public class UserRegister : Persona
        {
            public string Documento { get; set; }
        }
    }
}
