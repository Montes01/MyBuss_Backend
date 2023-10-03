namespace API.Models
{
    public class Conductor : Persona
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
        public bool Estado { get; set; }
        public string FkPlacaBus { get; set; }
        public string HoraEntrada { get; set; }
        public string HoraSalida { get; set; }
    }
}
