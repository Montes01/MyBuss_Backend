namespace API.Models
{
    public abstract class Persona
    {
        public string Foto { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public virtual int Edad { get; set; }
        public string Telefono { get; set; }
        public string Contraseña { get; set; }
        public string Correo{ get; set; }
    }
}
