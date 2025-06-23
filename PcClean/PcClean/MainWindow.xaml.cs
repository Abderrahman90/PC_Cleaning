using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;


namespace PcClean
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public DirectoryInfo winTemp;
        public DirectoryInfo appTemp;
        public MainWindow()
        {
            InitializeComponent();
            winTemp = new DirectoryInfo(@"C:\Windows\Temp"); // pourquoi ne pas mettre le chemin en dur car il est spécifique à chaque personne et cela permet de d'avoir des infos sur la taille du dossier
            appTemp = new DirectoryInfo(System.IO.Path.GetTempPath());

            _ = CheckActuAsync();  // ✅ Supprime l'avertissement, tâche lancée en arrière-plan ou au mieux, dans un `async void Window_Loaded` handler

            //CheckActuAsync().ConfigureAwait(true); ConfigureAwait(true) retourne une Task mais vous ne l'attendez pas avec await 
            //Dans un constructeur(qui n'est pas async), cette ligne ne fait effectivement rien d'utile
            //La tâche se lance mais le constructeur continue sans attendre le résultat


        }
        static readonly HttpClient client = new HttpClient();

        public async Task CheckActuAsync()
        {
            string url = "http://localhost/testWeb/actu.txt";
            try
            {
                using HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // ✅ Vérifier si le contenu est vide ou null
                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    // Fichier vide ou contenu vide
                    ActuTxt.Content = "Aucune actualité disponible pour le moment";
                    ActuTxt.Foreground = new SolidColorBrush(Colors.Yellow);
                    ActuTxt.Visibility = Visibility.Visible;
                    bandeauActu.Visibility = Visibility.Visible;
                }
                else
                {
                    // Serveur accessible avec contenu
                    ActuTxt.Content = responseBody;
                    ActuTxt.Foreground = new SolidColorBrush(Colors.Black); // Couleur normale
                    ActuTxt.Visibility = Visibility.Visible;
                    bandeauActu.Visibility = Visibility.Visible;
                }
            }
            catch (HttpRequestException e)
            {
                // Serveur inaccessible ou fichier inexistant
                ActuTxt.Content = "Service d'actualités indisponible, vérifiez si le serveur WAMP est lancé";
                ActuTxt.Foreground = new SolidColorBrush(Colors.Orange);
                ActuTxt.Visibility = Visibility.Visible;
                bandeauActu.Visibility = Visibility.Visible;
                Console.WriteLine($"Erreur HTTP: {e.Message}");
            }
            catch (Exception e)
            {
                // Autres erreurs
                ActuTxt.Content = "Erreur lors de la récupération des actualités";
                ActuTxt.Foreground = new SolidColorBrush(Colors.GreenYellow);
                ActuTxt.Visibility = Visibility.Visible;
                bandeauActu.Visibility = Visibility.Visible;
                Console.WriteLine($"Erreur générale: {e.Message}");
            }
        }
        // on va crée une méthode pour calculer la taille d'un dossier 
        public long DirSize(DirectoryInfo dir)
        {
            return dir.GetFiles().Sum(fi => fi.Length) + dir.GetDirectories().Sum(di => DirSize(di)); // cela me retourne System.UnauthorizedAccessException : 'Access to the path 'C:\Windows\Temp' is denied.'
                                                                                                      // pour corriger je crée mon fichier manifest pour ajouter les droits admin et pas que 
        }

        public void ClearTempData(DirectoryInfo di)
        {
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                    Console.WriteLine(file.FullName);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Delete(true); // true veut dire en récursive tout ce que contient le dossier et aussi des sous dossiers ainsi de suite
                    Console.WriteLine(dir.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

        }
        private void Button_Maj_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mise à jour éffectuée ! ", "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Button_Histo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO créer une page historique ! ", "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Warning);

        }

        private void Button_Web_Click(object sender, RoutedEventArgs e)
        {
            // pour faire plus simple on peut faire passer par Process.Start() en passant en param l'url => déclencher processus c'est à dire navigateur web

            // attention il existe une bonne pratique en code pour éviter des erreurs si par exemple l'utilisateur n'a pas de navigateur web
            // donc on englobe cela dans un bloc try catch
            try
            {
                Process.Start(new ProcessStartInfo("https://www.touslesdrivers.com/?v_page=29")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erreur" + ex.Message);
            }
        }

        private void Button_Analyser_Click(object sender, RoutedEventArgs e)
        {
            // je dois retourner ma méthode d'analyse 
            AnalyzeFolders();
        }

        public void AnalyzeFolders()
        {
            Console.WriteLine("Début d'analyse... ");
            long totalSize = 0;

            // j'englobe les opérations de calcul sensible car au cas que cela ne donne rien
            try
            {
                totalSize += DirSize(winTemp) / 1000000;
                totalSize += DirSize(appTemp) / 1000000;

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Impossible d'analyser les dossiers {ex.Message}");
            }

            espace.Content = totalSize + " Mb"; // cela me donne le contenu de l'espace à libérer en Mb

            // au niveau de mon label principal après avoir cliqué sur analyser j'affiche mon message
            titre.Content = "Analyse efféctuée ";

            // je veux avoir la dernière date de l'analyse
            //date.Content = DateTime.Today.ToString("dd/MM/yyyy"); // retourne la date du jour, méthode simple mais format fixe

            // pour le clean code
            //date.Content = DateTime.Today.ToString("d", CultureInfo.CurrentCulture);

            // méthode raccourci que je viens d'apprendre
            date.Content = DateTime.Today.ToShortDateString();

        }



        private void Button_Nettoyer_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Nettoyage en cours...");
            btnClean.Content = "NETTOAYGE EN COURS...";

            // je peux aussi nettoyer le press papier (copier coller) qui prend de l'espace
            //Clipboard.Clear();

            // on nettoie nos dossiers temps comme c'est sensible on le met dans un try
            try
            {
                ClearTempData(winTemp);

            }
            catch (Exception ex)
            {

                Console.WriteLine("Erreur le dossier n'existe pas " + ex.Message);
            }

            // je fais pareil avec le dosser appTemp
            try
            {
                ClearTempData(appTemp);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Erreur le dossier n'existe pas " + ex.Message);
            }
            btnClean.Content = "Nettoyage terminé";
            titre.Content = "Nettoyage effectuée !";
            espace.Content = "0 Mb";
        }

        //private void Btn_Click(object sender, RoutedEventArgs e)
        //{
        //    //fenetre.Close();
        //    // on peut cibler aussi les éléments nommés grâce au x:Name="cible"
        //    Btn.Content = "Test";
        //}

        //private void MonBouton (object sender, EventArgs e)
        //{
        //    MessageBox.Show("Vous avez cliqué sur mon bouton cool");
        //    // une fois cliqué sur mon bouton la fenetre se fermera
        //    fenetre.Close();


        //}

    }
}