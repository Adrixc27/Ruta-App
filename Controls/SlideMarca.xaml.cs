using System.Windows.Controls;

namespace Ruta_App
{
    /// <summary>Panel azul de marca: ilustración de la ruta y titular. Primera diapositiva del carrusel.</summary>
    public partial class SlideMarca : UserControl
    {
        public SlideMarca()
        {
            InitializeComponent();
        }

        public string Titular
        {
            get { return TxtTitular.Text; }
            set { TxtTitular.Text = value; }
        }

        public string Subtitular
        {
            get { return TxtSubtitular.Text; }
            set { TxtSubtitular.Text = value; }
        }
    }
}
