using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpLab8
{
    interface IView
    {
        string SourceDirectory();
        string TargetDirectory();

        void TryToSynchronize(List<string> message);

        event EventHandler<EventArgs> SyncronizeDirectoriesEvent;
    }

    class Model
    {
        public List<string> SynchronizeDirectories(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo source = new DirectoryInfo(sourceDirectory);
            DirectoryInfo target = new DirectoryInfo(targetDirectory);
            List<string> resultOfSynchronization = InnerSynchronizeDirectory(source, target);

            return resultOfSynchronization;
        }

        private List<string> InnerSynchronizeDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            bool isNeedToSynchronize = false;
            List<string> innerResultOfSynchronization = new List<string>();

            foreach (FileInfo file in source.GetFiles())
            {
                FileInfo otherDirectoryFile = new FileInfo(Path.Combine(target.FullName, file.Name));

                if (!otherDirectoryFile.Exists || otherDirectoryFile.LastWriteTime < file.LastWriteTime)
                {
                    File.Copy(file.FullName, otherDirectoryFile.FullName, true);
                    innerResultOfSynchronization.Add($"File {file.Name} changed");
                    isNeedToSynchronize = true;
                }
            }

            foreach (FileInfo file in target.GetFiles())
            {
                FileInfo mainDirectoryFile = new FileInfo(Path.Combine(source.FullName, file.Name));

                if (!mainDirectoryFile.Exists)
                {
                    file.Delete();
                    innerResultOfSynchronization.Add($"File {file.Name} deleted");
                    isNeedToSynchronize = true;
                }
            }

            if (!isNeedToSynchronize)
            {
                innerResultOfSynchronization.Add("Seems like there is no need in synchronization");
            }

            return innerResultOfSynchronization;
        }
    }

    class Presenter
    {
        private IView view;
        private Model model;


        public Presenter(IView inputView)
        {
            view = inputView;
            model = new Model();

            view.SyncronizeDirectoriesEvent += new EventHandler<EventArgs>(Synchronize);
        }

        private void Synchronize(object sender, EventArgs inputEvent)
        {
            List<string> resultOfSynchronization = model.SynchronizeDirectories(view.SourceDirectory(), view.TargetDirectory());

            view.TryToSynchronize(resultOfSynchronization);
        }
    }
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}